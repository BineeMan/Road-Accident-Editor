using Microsoft.Maui.Controls.Platform;
using System.Diagnostics;
using System.Text.RegularExpressions;
using RoadsApp2.ViewModels;

namespace RoadsApp2;

public partial class AccidentRegistrationPage : ContentPage
{
    private List<string> ParticipantsNames { get; set; } = new List<string>();

	public AccidentRegistrationPage()
	{
		InitializeComponent();
        BindingContext = new AccidentRegistrationViewModel();
	}

    private void OnDeleteSwipeItem_Invoked(object sender, EventArgs e)
    {
        SwipeItem swipeItem = (SwipeItem)sender;
        SwipeItems swipeItems = (SwipeItems)swipeItem.Parent;
        SwipeView swipeView = (SwipeView)swipeItems.Parent;
        Frame frame = (Frame)swipeView.Content;
        Label label = (Label)frame.Content;

        ParticipantsNames.Remove(label.Text);
        ParticipantsStackLayout.Remove(swipeView);
        
    }

    private void ImageButtonPlus_Clicked(object sender, EventArgs e)
    {
        ParticipantEntry.Placeholder = "¬ведите значение";
        ParticipantEntry.PlaceholderColor = Color.FromArgb("808080");

        string participantName = ParticipantEntry.Text;

        if (string.IsNullOrEmpty(participantName))
        {
            ParticipantEntry.Placeholder = "¬ведите значение";
            ParticipantEntry.PlaceholderColor = Color.FromArgb("#C00000");
            return;
        }

        ParticipantsNames.Add(participantName);

        SwipeItem deleteSwipeItem = new SwipeItem
        {
            IconImageSource = "delete.png",
            BackgroundColor = Colors.LightPink
        };
        deleteSwipeItem.Invoked += OnDeleteSwipeItem_Invoked;

        List<SwipeItem> swipeItems = new List<SwipeItem>() { deleteSwipeItem };

        // SwipeView content

        Frame frame = new Frame()
        {
            BackgroundColor = Color.FromArgb("#E1E1E1"),
            BorderColor = Color.FromArgb("#A3A3A3"),
            CornerRadius = 30,
            Padding = new Thickness(10, 0),
            WidthRequest = 215,
            Margin = new Thickness(0, 10, 50, 0),
            MinimumHeightRequest = 40,
        };
        Label label = new Label()
        {
            FontAttributes = FontAttributes.Bold,
            FontSize = 16,
            Margin = new Thickness(10),
            HorizontalOptions = LayoutOptions.Start,
            FontFamily = "Rubik",
            TextColor = Color.FromArgb("#000000"),
            Text = participantName
        };
        frame.Content = label;

        SwipeView swipeView = new SwipeView
        {
            RightItems = new SwipeItems(swipeItems),
            Content = frame
        };
        ParticipantsStackLayout.Add(swipeView);

        ParticipantEntry.Text = "";
    }

    private void ButtonSave_Clicked(object sender, EventArgs e)
    {

    }
}