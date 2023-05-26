
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
        string action = await DisplayActionSheet("�������� ��������\n� ��������",
            "������", null, "���������� ������ � ���", "���������", "������������� � ���� ������", "�������");
        TextCell textCell = (TextCell)sender;
        if (action != null)
        {
            if (action == "���������� ������ � ���")
            {
                RoadAccidentItem roadAccident1 = GetRoadAccidentItem(textCell.Text, textCell.Detail);
                List<ParticipantItem> participantItems =
                    await MainPage.RoadAccidentDatabase.GetParticipantItemsByRoadAcidentAsync(roadAccident1);
                string participants = "";

                foreach (ParticipantItem participantItem1 in participantItems)
                {
                    participants += $"{participantItem1.SecondName} {participantItem1.FirstName} {participantItem1.LastName}, {participantItem1.CarName} {participantItem1.CarNumber} \n";
                }
                string roadAccidentData = $"�������� ���: {roadAccident1.Name}\n" +
                    $"��������� �����: {roadAccident1.Address}\n" +
                    $"���� � �����: {roadAccident1.DateTime}\n" +
                    $"��������: {roadAccident1.Description}\n" +
                    $"���������:\n" +
                    $"{participants}";
                await DisplayAlert("������ � ���", roadAccidentData, "��");
            }
            else if (action == "�������")
            {
                bool answer = await DisplayAlert("�������� �������", "�� ������������� ������ ������� ���� ������? �������������� ����� ����������.", "��", "������");
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
            else if (action == "���������")
            {
                bool answer = await DisplayAlert("�������� �������", "�� ������������� ������ ��������� ���� ������? ��� ������������� ������ ����� �������", "��", "������");
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
                        await DisplayAlert("������", "�������� �������������� ������\n ��� �������� �����", "��");
                    }

                }
            }
            else if (action == "������������� � ���� ������")
            {
                string result = await DisplayPromptAsync("������������� � ���� ������", "������� �������� �����");
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

                        await DisplayToast($"���� {result}.html �������� � ����� ���������");
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("������", "��� ���������� �����\n �������� �������������� ������", "��");
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
        XmlText h2Text = XmlDocumentSchema.CreateTextNode($"����: {roadAccidentItem.DateTime}");
        h2Element.AppendChild(h2Text);
        divElement2.AppendChild(h2Element);

        XmlElement h2Element2 = XmlDocumentSchema.CreateElement("h2");
        XmlText h2Text2 = XmlDocumentSchema.CreateTextNode($"��������� �����: {roadAccidentItem.Address}");
        h2Element2.AppendChild(h2Text2);
        divElement2.AppendChild(h2Element2);

        XmlElement h2Element3 = XmlDocumentSchema.CreateElement("h2");
        XmlText h2Text3 = XmlDocumentSchema.CreateTextNode("���������");
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
        XmlText h2Text4 = XmlDocumentSchema.CreateTextNode("��������");
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
        string result = await DisplayPromptAsync("��������� ���� �� ����", "������� ��� �����");
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
                    result = await DisplayPromptAsync("��������� ���� �� ����", "������� ��� �����");
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

            await DisplayToast("���� �������� � ����� ���������");
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
            PickerTitle = "�������� XML ���� �������",
            FileTypes = customFileType,
        };

        try
        {
            var result = await FilePicker.Default.PickAsync();
            if (result != null)
            {
                bool answer = await DisplayAlert("�������� �������",
                    "�� ������������� ������ ��������� ���� �����? ��� ������������� ������ ����� �������",
                    "��", "������");
                if (answer)
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.Load(result.FullPath);
                    LoadXmlToEditor(xmlDocument.OuterXml);
                    await DisplayToast("���� ������� ��������");
                }   
            }
            else
            {
                await DisplayToast("���� �� ��� ��������");
            }
        }
        catch (Exception ex)
        {
            await DisplayToast("���� �� ��� ��������");
        }
    }

    private void Button_ConvertFileToReportFile_Clicked(object sender, EventArgs e)
    {

    }
}