namespace RoadsApp2;

public partial class InfoPage : ContentPage
{
	public InfoPage()
	{
		InitializeComponent();
#if WINDOWS
		this.Title = "";
		VerticalStackLayoutMain.WidthRequest = 500;
#endif
#if ANDROID
		VerticalStackLayoutMain.Margin = new Thickness(10, 0);
#endif
    }


}