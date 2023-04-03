using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RoadsApp2.Database
{
    public class ParticipantItem
    {
        [PrimaryKey, AutoIncrement]
        public int ID_Participant { get; set; }

        public string FirstName { get; set; }

        public string SecondName { get; set; }

        public string LastName { get; set; }

        public string CarName { get; set; }

        public string CarNumber { get; set; }

        //public override string ToString()
        //{
        //    return $"{this.FirstName} {this.SecondName} {this.LastName}";
        //}
    }
}
