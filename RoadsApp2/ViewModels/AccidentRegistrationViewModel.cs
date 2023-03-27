using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RoadsApp2.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadsApp2.ViewModels
{
    [QueryProperty(nameof(Participant), "Participant")]
    public partial class AccidentRegistrationViewModel : ObservableObject
    {
        public AccidentRegistrationViewModel()
        {
            participants = new ObservableCollection<ParticipantItem>();
        }

        ParticipantItem participant;
        public ParticipantItem Participant
        {
            get => participant;
            set
            {
                participant = value;
                participants.Add(participant);
                OnPropertyChanged();
            }
        }

        [ObservableProperty]
        string name;

        [ObservableProperty]
        DateTime dateTime;

        [ObservableProperty]
        string address;

        [ObservableProperty]
        string description;

        [ObservableProperty]
        public ObservableCollection<ParticipantItem> participants;

        [RelayCommand]
        async Task OpenParticipantPage()
        {
            await Shell.Current.GoToAsync(nameof(NewParticipantPage));
        }

        [RelayCommand]
        void Save()
        {
            //RoadAccidentItem roadAccidentItem = new RoadAccidentItem();
            //Name = "123";
            Debug.WriteLine(participants.Count);
            foreach (var item in participants)
            {
                Debug.WriteLine(item.FirstName);
            }
        }

    }
}
