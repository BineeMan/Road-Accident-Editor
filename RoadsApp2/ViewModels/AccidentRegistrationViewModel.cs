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
    public partial class AccidentRegistrationViewModel : ObservableObject
    {
        [ObservableProperty]
        string name;

        [ObservableProperty]
        DateTime dateTime;

        [ObservableProperty]
        string address;

        [ObservableProperty]
        string description;

        [ObservableProperty]
        List<string> participants; 

        [RelayCommand]
        void Save()
        {
            RoadAccidentItem roadAccidentItem = new RoadAccidentItem();
            Debug.WriteLine(Name);
        }
    }
}
