using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RoadsApp2.Database;
using System.Diagnostics;

namespace RoadsApp2.ViewModels
{
    public partial class NewParticipantViewModel : ObservableObject
    {

        [ObservableProperty]
        string fullName;

        [ObservableProperty]
        string carModel;

        [ObservableProperty]
        string carNumber;

        [RelayCommand]
        async void Add()
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(carModel) || string.IsNullOrWhiteSpace(carNumber))
                return;
            string[] fullNameSplitted = fullName.Split(new char[] { ' ' });
            if (fullNameSplitted.Length < 3)
            {
                return;
            }

            ParticipantItem item = new ParticipantItem()
            {
                FirstName = fullNameSplitted[0],
                SecondName = fullNameSplitted[1],
                LastName = fullNameSplitted[2],
                CarName = carModel,
                CarNumber = carNumber,
            };

            var navigationParameter = new Dictionary<string, object>
            {
                { "Participant", item },
            };

            await Shell.Current.GoToAsync($"..", navigationParameter);
        }
    }
}
