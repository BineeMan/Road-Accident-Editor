using Microsoft.Maui.Graphics.Text;
using RoadsApp2.Database;
using RoadsApp2.ViewModels;
using System.Diagnostics;
using TextColors = RoadsApp2.DataClasses.TextColors;
namespace RoadsApp2;

public partial class NewParticipantPage : ContentPage
{

    public NewParticipantPage()
	{
		InitializeComponent();
		BindingContext = new NewParticipantViewModel();
#if WINDOWS
		this.Title = "";
		VerticalStackLayoutMain.WidthRequest = 500;
		this.Title = "";
#endif
    }

    private void ButtonAdd_Clicked(object sender, EventArgs e)
    {
        bool isValid = true;
        if (string.IsNullOrEmpty(FullNameEntry.Text))
        {
            FullNameEntry.TextColor = TextColors.ErrorColor;
            FullNameEntry.PlaceholderColor = TextColors.ErrorColor;
            isValid = false;
        }
        else
        {
            string[] fullNameSplitted = FullNameEntry.Text.Split(new char[] { ' ' });
            if (fullNameSplitted.Length < 3)
            {
                FullNameEntry.TextColor = TextColors.ErrorColor;
                FullNameEntry.PlaceholderColor = TextColors.ErrorColor;
                isValid = false;
            }
        }
        if (string.IsNullOrEmpty(CarNameEntry.Text))
        {
            CarNameEntry.TextColor = TextColors.ErrorColor;
            CarNameEntry.PlaceholderColor = TextColors.ErrorColor;
            isValid = false;
        }
        if (string.IsNullOrEmpty(CarNumberEntry.Text))
        {
            CarNumberEntry.TextColor = TextColors.ErrorColor;
            CarNumberEntry.PlaceholderColor = TextColors.ErrorColor;
            isValid = false;
        }
        if (!isValid)
            DisplayAlert("Ошибка входных данных", "Проверьте значения полей", "Ок");

    }

    private void Entry_Focused(object sender, FocusEventArgs e)
    {
        Entry entry = (Entry)sender;
        if (entry.TextColor == TextColors.ErrorColor)
        {
            entry.TextColor = TextColors.NormalColorText;
            entry.PlaceholderColor = TextColors.NormalColorPlaceholder;
        }
    }
}