using Microsoft.Maui.Controls.Platform;
using System.Diagnostics;
using System.Text.RegularExpressions;
using RoadsApp2.ViewModels;

namespace RoadsApp2;

public partial class AccidentRegistrationPage : ContentPage
{
    public static List<string> ParticipantsNames { get; set; } = new List<string>();

    AccidentRegistrationViewModel bindingContext = new AccidentRegistrationViewModel();

    public AccidentRegistrationPage()
    {
        InitializeComponent();
        BindingContext = bindingContext;
#if WINDOWS
		VerticalStackLayoutMain.WidthRequest = 500;
		this.Title = "";
#endif
    }

    private void OnDeleteSwipeItem_Invoked(object sender, EventArgs e)
    {
        SwipeItem swipeItem = (SwipeItem)sender;
        SwipeItems swipeItems = (SwipeItems)swipeItem.Parent;
        SwipeView swipeView = (SwipeView)swipeItems.Parent;
        Frame frame = (Frame)swipeView.Content;
        Label label = (Label)frame.Content;

        ParticipantsNames.Remove(label.Text);
        //ParticipantsStackLayout.Remove(swipeView);

    }


    private void ButtonSave_Clicked(object sender, EventArgs e)
    {

    }

    //private async void ButtonNewParticipant_Clicked(object sender, EventArgs e) =>
    //    await Navigation.PushAsync(new NewParticipantPage());

}