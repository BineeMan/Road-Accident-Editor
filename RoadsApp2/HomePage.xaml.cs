
using RoadsApp2.Database;
using System.Diagnostics;
using System.Xml;
using static RoadsApp2.Utils.Structs;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;


namespace RoadsApp2;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();

#if WINDOWS
        VerticalStackLayoutMain.WidthRequest = 500;
        this.Title = "";
        TableViewRoadAccidents.Margin = new Thickness(0, -50, 0, 0);
#endif

    }

    public static RoadAccidentItem CurrentRoadAccidentItem { get; set; }

    private async void Button_SaveDocument_Clicked(object sender, EventArgs e) =>
        await Navigation.PushAsync(new AccidentRegistrationPage());

    private List<RoadAccidentItem> RoadAccidentItems { get; set; } = new List<RoadAccidentItem>();

    private void RefreshGrid()
    {
        RoadAccidentsTableSelection.Clear();
        foreach (RoadAccidentItem item in RoadAccidentItems)
        {
            TextCell textCell = new()
            {
                DetailColor = Colors.White,
                TextColor = Colors.White,
                Detail = item.DateTime,
                Text = item.Name,
            };
            
            textCell.Tapped += TextCellRoadAccident_Tapped;
            RoadAccidentsTableSelection.Add(textCell);
        }
    }

    private async void ContentPage_Appearing(object sender, EventArgs e)
    {
        List<RoadAccidentItem> list = await MainPage.RoadAccidentDatabase.GetRoadAccidentItemsAsync();
        if (list.Count > RoadAccidentItems.Count)
        {
            RoadAccidentItems = list;
            RefreshGrid();
        }
    }

    private RoadAccidentItem GetRoadAccidentItem(string name, string dateTime)
    {
        foreach (RoadAccidentItem item in RoadAccidentItems)
        {
            if (item.DateTime == dateTime && item.Name == name)
            {
                return item;
            }
        }
        return new RoadAccidentItem();
    }

    private async void TextCellRoadAccident_Tapped(object sender, EventArgs e)
    {
        string action = await DisplayActionSheet("Выберите действие\nс проектом",
            "Отмена", null, "Посмотреть данные о ДТП", "Загрузить", "Преобразовать в файл отчета", "Удалить");
        TextCell textCell = (TextCell)sender;
        if (action != null)
        {
            if (action == "Посмотреть данные о ДТП")
            {
                RoadAccidentItem roadAccident1 = GetRoadAccidentItem(textCell.Text, textCell.Detail);
                List<ParticipantItem> participantItems =
                    await MainPage.RoadAccidentDatabase.GetParticipantItemsByRoadAcidentAsync(roadAccident1);
                string participants = "";

                foreach (ParticipantItem participantItem1 in participantItems)
                {
                    participants += $"{participantItem1.SecondName} {participantItem1.FirstName} {participantItem1.LastName}, {participantItem1.CarName} {participantItem1.CarNumber} \n";
                }
                string roadAccidentData = $"Название ДТП: {roadAccident1.Name}\n" +
                    $"Ближайший адрес: {roadAccident1.Address}\n" +
                    $"Дата и время: {roadAccident1.DateTime}\n" +
                    $"Описание: {roadAccident1.Description}\n" +
                    $"Участники:\n" +
                    $"{participants}";
                await DisplayAlert("Данные о ДТП", roadAccidentData, "Ок");
            }
            else if (action == "Удалить")
            {
                bool answer = await DisplayAlert("Удаление проекта", "Вы действительно хотите удалить этот проект? Восстановление будет невозможно.", "Да", "Отмена");
                if (answer)
                {
                    RoadAccidentItem roadAccident1 = GetRoadAccidentItem(textCell.Text, textCell.Detail);
                    await MainPage.RoadAccidentDatabase.DeleteRoadAccidentItemAsync(roadAccident1);
                    foreach (RoadAccidentItem roadAccidentItem in RoadAccidentItems)
                    {
                        if (roadAccidentItem.Name == textCell.Text && roadAccidentItem.DateTime == textCell.Detail)
                        {
                            RoadAccidentItems.Remove(roadAccidentItem);
                            break;
                        }
                    }      
                    RoadAccidentsTableSelection.Remove(textCell);
                }
            }
            else if (action == "Загрузить")
            {
                bool answer = await DisplayAlert("Загрузка проекта", "Вы действительно хотите загрузить этот проект? Все несохраненные данные будут утеряны", "Да", "Отмена");
                if (answer)
                {
                    string xml = "";
                    foreach (var item in RoadAccidentItems)
                    {
                        if (item.Name == textCell.Text && item.DateTime == textCell.Detail)
                        {
                            xml = item.SchemaXml;
                            CurrentRoadAccidentItem = item;
                            break;
                        }                  
                    }
                    try
                    {
                        LoadXmlToEditor(xml);

                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Ошибка", "Возникла непридвиденная ошибка\n при загрузке файла", "Ок");
                    }

                }
            }
            else if (action == "Преобразовать в файл отчета")
            {
                string result = await DisplayPromptAsync("Преобразовать в файл отчета", "Введите название файла");
                if (result != null)
                {
                    try
                    {
                        RoadAccidentItem roadAccident1 = GetRoadAccidentItem(textCell.Text, textCell.Detail);
                        string documentsDir = "";
#if WINDOWS
                        documentsDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
#endif
#if ANDROID
                        documentsDir = "/storage/emulated/0/Documents";
#endif
                        List<ParticipantItem> participantItems = await MainPage.RoadAccidentDatabase.GetParticipantItemsByRoadAcidentAsync(roadAccident1);
                        byte[] bytes = System.Convert.FromBase64String(roadAccident1.SchemaImage);
                        await File.WriteAllBytesAsync(Path.Combine(documentsDir, result + ".png"), bytes);

                        XmlDocument htmlDoc = GetReportHtmlFile(roadAccident1, participantItems, result);
                        htmlDoc.Save(Path.Combine(documentsDir, result + ".html"));

                        await DisplayToast($"Файл {result}.html сохранен в папке Документы");
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Ошибка", "При сохранении файла\n возникла непредвиденная ошибка", "Ок");
                    }
                }

            }
        }
    }


    private XmlDocument GetReportHtmlFile(RoadAccidentItem roadAccidentItem, List<ParticipantItem> participantItems, string imageFileName)
    {
        XmlDocument XmlDocumentSchema = new XmlDocument();
        void AddNewAttribute(string name, string value, XmlElement ElementAppendTo)
        {
            XmlAttribute xmlAttribute = XmlDocumentSchema.CreateAttribute(name);
            XmlText xmlAttributeText = XmlDocumentSchema.CreateTextNode(value);
            xmlAttribute.AppendChild(xmlAttributeText);
            ElementAppendTo.Attributes.Append(xmlAttribute);
        }
        XmlElement htmlElement = XmlDocumentSchema.CreateElement("html");
        XmlDocumentSchema.AppendChild(htmlElement);

        XmlElement headElement = XmlDocumentSchema.CreateElement("head");
        htmlElement.AppendChild(headElement);

        XmlElement metaElement = XmlDocumentSchema.CreateElement("meta");
        AddNewAttribute("charset", "utf-8", metaElement);
        headElement.AppendChild(metaElement);

        XmlElement titleElement = XmlDocumentSchema.CreateElement("title");
        XmlText nameText = XmlDocumentSchema.CreateTextNode(roadAccidentItem.Name);
        titleElement.AppendChild(nameText);
        headElement.AppendChild(titleElement);

        XmlElement styleElement = XmlDocumentSchema.CreateElement("style");
        AddNewAttribute("type", "text/css", styleElement);
        XmlText cssText = XmlDocumentSchema.CreateTextNode(".column2 {width: 1000px;  background: white;  padding: 5px;  padding-right: 20px;  padding-bottom: 300px;  padding-left: 25px;  margin-left:200px; float: middle; color: #5e5e5e; font-family: Times New Roman;font-size: 16pt;  position: relative;  left: auto; } .column1 {width: 1000px;  background: white; padding-right: 20px; padding-left: 25px;  margin-left:200px; float: middle; color: #5e5e5e; font-family: Times New Roman;font-size: 16pt; position: relative; left: auto; }");
        styleElement.AppendChild(cssText);
        headElement.AppendChild(styleElement);

        XmlElement bodyElement = XmlDocumentSchema.CreateElement("body");
        htmlElement.AppendChild(bodyElement);

        XmlElement divElement1 = XmlDocumentSchema.CreateElement("div");
        AddNewAttribute("class", "column1", divElement1);
        bodyElement.AppendChild(divElement1);

        XmlElement h1Element0 = XmlDocumentSchema.CreateElement("h1");
        h1Element0.AppendChild(nameText);
        divElement1.AppendChild(h1Element0);

        XmlElement imgElement = XmlDocumentSchema.CreateElement("img");
#if ANDROID
        AddNewAttribute("height", "1200", imgElement);
#endif
        AddNewAttribute("src", $"data:image/png;base64, {roadAccidentItem.SchemaImage}", imgElement);
        bodyElement.AppendChild(imgElement);

        XmlElement divElement2 = XmlDocumentSchema.CreateElement("div");
        AddNewAttribute("class", "column2", divElement2);
        bodyElement.AppendChild(divElement2);

        XmlElement h2Element = XmlDocumentSchema.CreateElement("h2");
        XmlText h2Text = XmlDocumentSchema.CreateTextNode($"Дата: {roadAccidentItem.DateTime}");
        h2Element.AppendChild(h2Text);
        divElement2.AppendChild(h2Element);

        XmlElement h2Element2 = XmlDocumentSchema.CreateElement("h2");
        XmlText h2Text2 = XmlDocumentSchema.CreateTextNode($"Ближайший адрес: {roadAccidentItem.Address}");
        h2Element2.AppendChild(h2Text2);
        divElement2.AppendChild(h2Element2);

        XmlElement h2Element3 = XmlDocumentSchema.CreateElement("h2");
        XmlText h2Text3 = XmlDocumentSchema.CreateTextNode("Участники");
        h2Element3.AppendChild(h2Text3);
        divElement2.AppendChild(h2Element3);

        foreach (ParticipantItem participantItem in participantItems)
        {
            XmlElement liElement = XmlDocumentSchema.CreateElement("li");
            XmlText liText = XmlDocumentSchema.CreateTextNode($"{participantItem.SecondName} {participantItem.FirstName} {participantItem.LastName}, {participantItem.CarNumber} {participantItem.CarNumber}");
            liElement.AppendChild(liText);
            divElement2.AppendChild(liElement);
        }
        divElement2.AppendChild(h2Element2);

        XmlElement h2Element4 = XmlDocumentSchema.CreateElement("h2");
        XmlText h2Text4 = XmlDocumentSchema.CreateTextNode("Описание");
        h2Element4.AppendChild(h2Text4);
        divElement2.AppendChild(h2Element4);

        XmlElement bElement = XmlDocumentSchema.CreateElement("b");
        XmlText bText = XmlDocumentSchema.CreateTextNode(roadAccidentItem.Description);
        bElement.AppendChild(bText);
        divElement2.AppendChild(bElement);

        return XmlDocumentSchema;
    }

    private void LoadXmlToEditor(string xml)
    {
        MainPage.XMLConverterMainPage.ClearAbsoluteLayout();

        MainPage.XMLConverterMainPage.ConvertXmlToViews(xml,
        out List<Node> nodes, out List<Image> images, out List<Link> links);

        MainPage.XMLConverterMainPage.MainPage.Nodes = nodes;
        MainPage.XMLConverterMainPage.MainPage.RoadObjects = images;
        MainPage.XMLConverterMainPage.MainPage.Links = links;
    }

    private async void Button_OpenHelpPage_Clicked(object sender, EventArgs e) =>
        await Navigation.PushAsync(new InfoPage());

    private async void Button_SaveFileOnDevice_Clicked(object sender, EventArgs e)
    {
        string result = await DisplayPromptAsync("Сохранить файл на диск", "Введите имя файла");
        if (result != null)
        {
            if (result == "Cancel")
                return;
            while (string.IsNullOrEmpty(result))
            {
                if (result != null)
                {
                    if (result == "Cancel")
                        return;
                    result = await DisplayPromptAsync("Сохранить файл на диск", "Введите имя файла");
                }
                else
                {
                    return;
                }
            }
            MainPage.XMLConverterMainPage.ConvertLinesToXML();
            MainPage.XMLConverterMainPage.ConvertNodesToXML();
            MainPage.XMLConverterMainPage.ConvertImagesToXML();
            XmlDocument xmlSchema = MainPage.XMLConverterMainPage.XmlDocumentSchema;
            string documentsDir = "";
#if WINDOWS
            documentsDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
#endif
#if ANDROID
            documentsDir = "/storage/emulated/0/Documents";
#endif
            string targetDir = Path.Combine(documentsDir, result);
            xmlSchema.Save(targetDir + ".xml");

            await DisplayToast("Файл сохранен в папке Документы");
        }
    }

    private async Task DisplayToast(string text)
    {
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        ToastDuration duration = ToastDuration.Short;
        double fontSize = 14;
        var toast = Toast.Make(text, duration, fontSize);
        await toast.Show(cancellationTokenSource.Token);
    }


    private async void Button_OpenFileFromDevice_Clicked(object sender, EventArgs e)
    {
        var customFileType = new FilePickerFileType(
            new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] { "xml" } },
                { DevicePlatform.WinUI, new[] { "xml" } },
            });

        PickOptions options = new()
        {
            PickerTitle = "Выберите XML файл проекта",
            FileTypes = customFileType,
        };

        try
        {
            var result = await FilePicker.Default.PickAsync();
            if (result != null)
            {
                bool answer = await DisplayAlert("Загрузка проекта",
                    "Вы действительно хотите загрузить этот файла? Все несохраненные данные будут утеряны",
                    "Да", "Отмена");
                if (answer)
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.Load(result.FullPath);
                    LoadXmlToEditor(xmlDocument.OuterXml);
                    await DisplayToast("Файл успешно загружен");
                }   
            }
            else
            {
                await DisplayToast("Файл не был загружен");
            }
        }
        catch (Exception ex)
        {
            await DisplayToast("Файл не был загружен");
        }
    }

    private void Button_ConvertFileToReportFile_Clicked(object sender, EventArgs e)
    {

    }
}