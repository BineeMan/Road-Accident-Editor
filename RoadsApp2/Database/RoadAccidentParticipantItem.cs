using Microsoft.Maui.Controls;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadsApp2.Database
{
    public class RoadAccidentParticipantItem
    {

        [PrimaryKey, AutoIncrement]
        public int ID_RoadAccidentParticipant { get; set; }

        [SQLiteNetExtensions.Attributes.ForeignKey(typeof(RoadAccidentItem))]
        public int ID_RoadAccident { get; set; }

        [SQLiteNetExtensions.Attributes.ForeignKey(typeof(ParticipantItem))]
        public int ID_Participant { get; set; }
    }
}
