using Microsoft.Maui.Controls;
using RoadsApp2.Database;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using static RoadsApp2.Utils.Structs;
using CommunityToolkit.Maui.Alerts;
using System.Threading;
using CommunityToolkit.Maui.Core;
using System.Xml.Linq;

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

    private async void Button_SaveDocument_Clicked(object sender, EventArgs e) =>
        await Navigation.PushAsync(new AccidentRegistrationPage());

    private List<RoadAccidentItem> RoadAccidentItems { get; set; } = new List<RoadAccidentItem>();

    private async void RefreshGrid()
    {
        RoadAccidentsTableSelection.Clear();
        List<RoadAccidentItem> list;
        list = await MainPage.RoadAccidentDatabase.GetRoadAccidentItemsAsync();
        foreach (RoadAccidentItem item in list)
        {
            TextCell textCell = new()
            {
                DetailColor = Colors.White,
                TextColor = Colors.White,
                Detail = item.DateTime,
                Text = item.Name,
                Height = 50
            };
            textCell.Tapped += TextCellRoadAccident_Tapped;
            RoadAccidentsTableSelection.Add(textCell);
        }
    }

    private async void ContentPage_Appearing(object sender, EventArgs e)
    {
        Debug.WriteLine("ContentPage_Appearing");
        List <RoadAccidentItem> list = await MainPage.RoadAccidentDatabase.GetRoadAccidentItemsAsync();
        if (list.Count > RoadAccidentItems.Count)
        {
            RefreshGrid();
            RoadAccidentItems = list;
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
        string action = await DisplayActionSheet("Выберите действие с проектом",
            "Отмена", null, "Посмотреть данные о ДТП", "Загрузить", "Удалить");
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
                    RoadAccidentsTableSelection.Remove(textCell);
                }
            }
            else if (action == "Загрузить")
            {
                bool answer = await DisplayAlert("Загрузка проекта", "Вы действительно хотите загрузить этот проект? Все несохраненные данные будут утеряны", "Да", "Отмена");
                Debug.WriteLine(answer);
                if (answer)
                {
                    string xml = "";
                    foreach (var item in RoadAccidentItems)
                    {
                        if (item.Name == textCell.Text && item.DateTime == textCell.Detail)
                        {
                            xml = item.SchemaXml;
                            break;
                        }
                    }
                    LoadXmlToEditor(xml);

                }
            }
        }       
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
        string result = "";
        while (string.IsNullOrEmpty(result))
        {
            if (result == "Cancel")
                return;
            result = await DisplayPromptAsync("Сохранить файл на диск", "Введите имя файла");
        }
        if (result != "Cancel")
        {
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
            Debug.WriteLine(targetDir);
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
}