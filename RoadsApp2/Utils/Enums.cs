using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadsApp2.Utils
{
    internal class Enums
    {
        public enum Orientation
        {
            Up = 0, Down = 180, Left = 270, Right = 90, Undefined = -1
        }

        public enum ConnectingMode
        {
            ToNewNode = 0, ToExistingNode = 1, None = -1
        }

    }
}
