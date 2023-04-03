using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadsApp2.Database
{
    public class RoadAccidentItem
    {
        [PrimaryKey, AutoIncrement]
        public int ID_RoadAccident { get; set; }

        public string Name { get; set; }

        public string DateTime { get; set; }

        public string Address { get; set; }

        public string Description { get; set; }

        public string SchemaXml { get; set; }

    }

}
