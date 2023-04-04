namespace RoadsApp2;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		Routing.RegisterRoute(nameof(NewParticipantPage), typeof(NewParticipantPage));

        Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
    }


}
