namespace RoadsApp2;

public partial class HomePage : ContentPage
{
	public HomePage()
	{
		InitializeComponent();

#if WINDOWS
		VerticalStackLayoutMain.WidthRequest = 500;
		this.Title = "";
#endif
    }

    private async void Button_SaveDocument_Clicked(object sender, EventArgs e) =>
        await Navigation.PushAsync(new AccidentRegistrationPage());
}