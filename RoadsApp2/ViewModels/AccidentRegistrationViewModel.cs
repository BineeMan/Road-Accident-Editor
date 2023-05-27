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
using System.Windows.Input;
using System.Xml;

namespace RoadsApp2.ViewModels
{
    [QueryProperty(nameof(Participant), "Participant")]
    public partial class AccidentRegistrationViewModel : ObservableObject
    {
        private RoadAccidentDatabase RoadAccidentDatabase = new();
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
                participantsCount = participants.Count;
                OnPropertyChanged();
            }
        }

        public static int participantsCount = 0; 

        [ObservableProperty]
        string name;

        [ObservableProperty]
        DateTime dateTime;

        [ObservableProperty]
        string address;

        [ObservableProperty]
        string description;
     
        [ObservableProperty]
        ObservableCollection<ParticipantItem> participants;

        [RelayCommand]
        async Task OpenParticipantPage()
        {
            await Shell.Current.GoToAsync(nameof(NewParticipantPage));
        }

        private bool ArePropertiesValid()
        {
            return !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(address) && !string.IsNullOrEmpty(Description) && participants.Count > 0;
        }

        [RelayCommand]
        async void Save()
        {
            //if (!ArePropertiesValid())
            //    return;
            //MainPage.XMLConverterMainPage.ConvertLinesToXML();
            //MainPage.XMLConverterMainPage.ConvertNodesToXML();
            //MainPage.XMLConverterMainPage.ConvertImagesToXML();
            //XmlDocument xmlSchema = MainPage.XMLConverterMainPage.XmlDocumentSchema;

            //RoadAccidentItem roadAccidentItem = new()
            //{
            //    Name = name,
            //    DateTime = dateTime,
            //    Address = address,
            //    Description = description,
            //    SchemaXml = xmlSchema.OuterXml
            //};
            //await RoadAccidentDatabase.SaveRoadAccidentItemAsync(roadAccidentItem);

            //foreach (var participant in participants)
            //{
            //    await RoadAccidentDatabase.SaveParticipantItemAsync(participant);
            //    RoadAccidentParticipantItem roadAccidentParticipantItem = new()
            //    {
            //        ID_Participant = participant.ID_Participant,
            //        ID_RoadAccident = roadAccidentItem.ID_RoadAccident
            //    };
            //    await RoadAccidentDatabase.SaveRoadAccidentParticipantItemAsync(roadAccidentParticipantItem);
            //};
            //await Shell.Current.GoToAsync("..");
        }


        [RelayCommand]
        void RemoveLast()
        {
            if (participants.Count > 0)
            {
                participantsCount--;
                participants.RemoveAt(participants.Count-1);
            }
        }
    }
}
