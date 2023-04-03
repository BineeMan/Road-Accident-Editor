using RoadsApp2.Database;
using System.Collections.Generic;
using System.Diagnostics;

using static RoadsApp2.Utils.Structs;

namespace RoadsApp2;

public partial class HomePage : ContentPage
{
	public HomePage()
	{
		InitializeComponent();

#if WINDOWS
		VerticalStackLayoutMain.WidthRequest = 500;
		this.Title = "";
        TableViewRoadAccidents.Margin = new Thickness(0, -50);
#endif
    }



    private async void Button_SaveDocument_Clicked(object sender, EventArgs e) =>
        await Navigation.PushAsync(new AccidentRegistrationPage());

    private List<RoadAccidentItem> RoadAccidentItems = new List<RoadAccidentItem>();

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
        List <RoadAccidentItem> list = await MainPage.RoadAccidentDatabase.GetRoadAccidentItemsAsync();
        if (list.Count > RoadAccidentItems.Count)
        {
            RefreshGrid();
            RoadAccidentItems = list;
        }
    }

    private async void TextCellRoadAccident_Tapped(object sender, EventArgs e)
    {
        bool answer = await DisplayAlert("Загрузка проекта", "Вы действительно хотите загрузить этот проект? Все несохраненные данные будут утеряны", "Да", "Отмена");
        if (answer)
        {
            TextCell textCell = (TextCell)sender;
            string xml = "";
            foreach (var item in RoadAccidentItems)
            {
                if (item.Name == textCell.Text && item.DateTime == textCell.Detail)
                {
                    xml = item.SchemaXml;
                    break;
                }
            }
            MainPage.XMLConverterMainPage.ClearAbsoluteLayout();
            MainPage.XMLConverterMainPage.ConvertXmlToViews(xml,
                out List<Node> nodes, out List<Image> images, out List<Link> links);

            MainPage.XMLConverterMainPage.MainPage.Nodes = nodes;
            MainPage.XMLConverterMainPage.MainPage.RoadObjects = images;
            MainPage.XMLConverterMainPage.MainPage.Links = links;
        }
    }
}