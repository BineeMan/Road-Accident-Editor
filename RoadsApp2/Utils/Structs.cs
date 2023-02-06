using Microsoft.Maui.Controls.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadsApp2.Utils
{
    internal class Structs
    {
        public struct Node
        {
            public Rectangle rectangle;
            public List<ImageButton> imageButtons;
            public List<Polygon> roads;
            public bool isActive;
        }

        public struct LineCoords
        {
            public Point coord1;
            public Point coord2;
        }

        public struct Link
        {
            public Polygon road;
            public List<Line> lines;
            public List<ImageButton> imageButtons;
        }
    }
}
