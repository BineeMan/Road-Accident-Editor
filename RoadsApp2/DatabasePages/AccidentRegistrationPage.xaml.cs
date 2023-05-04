using System.Diagnostics;
using RoadsApp2.ViewModels;
using RoadsApp2.DataClasses;
using RoadsApp2.Database;
using TextColors = RoadsApp2.DataClasses.TextColors;
using System.Xml;

namespace RoadsApp2;

[QueryProperty(nameof(Participant), "Participant")]
public partial class AccidentRegistrationPage : ContentPage
{
    public AccidentRegistrationPage()
    {
        InitializeComponent();
        BindingContext = new AccidentRegistrationViewModel();
        Participants = new List<ParticipantItem>();       
#if WINDOWS
		VerticalStackLayoutMain.WidthRequest = 500;
		this.Title = "";
        AccidentTimePicker.Margin = new Thickness(0, 13, 0, 0);
        TableViewParticipant.Margin = new Thickness(0, -50);
#endif
        AccidentTimePicker.Format = "HH:mm";
    }

    ParticipantItem participant;
    public ParticipantItem Participant
    {
        get => participant;
        set
        {
            participant = value;
            AddParticipantToView(value);
            Participants.Add(participant);
            OnPropertyChanged();
        }
    }

    private List<ParticipantItem> Participants;

    private void AddParticipantToView(ParticipantItem participant)
    {
        TextCell textCell = new()
        {
            DetailColor = Color.FromArgb("#737373"),
            TextColor = Colors.Black,
            Detail = $"{participant.CarName}, {participant.CarNumber}",
            Text = $"{participant.SecondName} {participant.FirstName} {participant.LastName}",
            Height = 50
        };
        textCell.Tapped += TextCellParticipant_Tapped;
        ParticipantsTableSection.Add(textCell);
    }

    private async void TextCellParticipant_Tapped(object sender, EventArgs e)
    {
        bool answer = await DisplayAlert("Удаление участника",
            "Вы действительно хотите удалить участника?", "Да", "Отмена");
        if (answer)
        {
            TextCell textCell = (TextCell)sender;
            string[] fullNameSplitted = textCell.Text.Split(new char[] { ' ' });
            foreach (ParticipantItem participantItem in Participants)
            {
                if (participantItem.FirstName == fullNameSplitted[1]
                    && participantItem.SecondName == fullNameSplitted[0]
                    && participantItem.LastName == fullNameSplitted[2])
                {
                    Participants.Remove(participantItem);
                    break;
                }
            }
            ParticipantsTableSection.Remove(textCell);
        }
    }

    private async void ButtonSave_Clicked(object sender, EventArgs e)
    {
        bool isValid = true;
        if (string.IsNullOrEmpty(NameEntry.Text))
        {
            NameEntry.TextColor = TextColors.ErrorColor;
            NameEntry.PlaceholderColor = TextColors.ErrorColor;
            isValid = false;
        }
        if (string.IsNullOrEmpty(AddressEntry.Text))
        {
            AddressEntry.TextColor = TextColors.ErrorColor;
            AddressEntry.PlaceholderColor = TextColors.ErrorColor;
            isValid = false;
        }
        if (string.IsNullOrEmpty(DescriptionEditor.Text))
        {
            DescriptionEditor.TextColor = TextColors.ErrorColor;
            DescriptionEditor.PlaceholderColor = TextColors.ErrorColor;
            isValid = false;
        }
        if (AccidentRegistrationViewModel.participantsCount < 1)
        {
            isValid = false;
        }
        if (isValid)
        {
            try
            {
                MainPage.XMLConverterMainPage.ConvertLinesToXML();
                MainPage.XMLConverterMainPage.ConvertNodesToXML();
                MainPage.XMLConverterMainPage.ConvertImagesToXML();
                XmlDocument xmlSchema = MainPage.XMLConverterMainPage.XmlDocumentSchema;

                await Shell.Current.GoToAsync(nameof(MainPage));
                await Task.Delay(500);
                RoadAccidentItem roadAccidentItem = new()
                {
                    Name = NameEntry.Text,
                    DateTime = $"{DatePicker.Date:dd.MM.yyyy} {AccidentTimePicker.Time}",
                    Address = AddressEntry.Text,
                    Description = DescriptionEditor.Text,
                    SchemaXml = xmlSchema.OuterXml,

                };
                IScreenshotResult screen = await MainPage.XMLConverterMainPage.FormAbsoluteLayout.CaptureAsync();
                Stream stream = await screen.OpenReadAsync();
                MemoryStream memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);

#if WINDOWS
#if DEBUG
            await File.WriteAllBytesAsync("G:\\C#\\RoadsApp2\\RoadsApp2\\Saved\\DebugImage.png", memoryStream.ToArray());
#endif
#endif
                var result = memoryStream.ToArray();
                string imageBase64 = Convert.ToBase64String(result);
                roadAccidentItem.SchemaImage = imageBase64;

                await MainPage.RoadAccidentDatabase.SaveRoadAccidentItemAsync(roadAccidentItem);

                foreach (var participant in Participants)
                {
                    await MainPage.RoadAccidentDatabase.SaveParticipantItemAsync(participant);
                    RoadAccidentParticipantItem roadAccidentParticipantItem = new()
                    {
                        ID_Participant = participant.ID_Participant,
                        ID_RoadAccident = roadAccidentItem.ID_RoadAccident
                    };
                    await MainPage.RoadAccidentDatabase.SaveRoadAccidentParticipantItemAsync(roadAccidentParticipantItem);
                };
                HomePage.CurrentRoadAccidentItem = roadAccidentItem;
                await Task.Delay(500);
                await Navigation.PopAsync();
                await Navigation.PopAsync();
                await DisplayAlert("Уведомление", "Данные успешно сохранены в БД", "Ок");
            }
            catch
            {
                await DisplayAlert("Ошибка", "Возникла непридвиденная ошибка\nпри загрузке файла", "Ок");
            }
        }
        else
        {
            await DisplayAlert("Ошибка входных данных", "Проверьте значения полей", "Ок");
        }
    }

    private void EntryOrEditor_Focused(object sender, FocusEventArgs e)
    {
        InputView inputView = (InputView)sender;  
        if (inputView.TextColor == TextColors.ErrorColor)
        {
            inputView.TextColor = TextColors.NormalColorText;
            inputView.PlaceholderColor = TextColors.NormalColorPlaceholder;
        }
    }

    private ParticipantItem GetParticipantItem(string[] fullNameSplitted)
    {
        foreach (ParticipantItem participantItem2 in Participants)
        {
            if (participantItem2.FirstName == fullNameSplitted[1]
                && participantItem2.SecondName == fullNameSplitted[0]
                && participantItem2.LastName == fullNameSplitted[2])
            {
                return participantItem2;
            }
        }
        return new ParticipantItem();
    }

    private void ContentPage_Appearing(object sender, EventArgs e)
    {
        DatePicker.Date = DateTime.Now;
        AccidentTimePicker.Time = DateTime.Now.TimeOfDay;
    }
}