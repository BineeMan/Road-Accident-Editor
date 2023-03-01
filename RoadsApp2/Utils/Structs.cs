using Microsoft.Maui.Animations;
using Microsoft.Maui.Controls.Shapes;
using System.Net;

namespace RoadsApp2.Utils
{
    internal class Structs
    {
        public struct Node
        {
            public Rectangle rectangle { get; set; }
            public List<Image> imageButtons { get; set; }
            public List<Link> roads { get; set; }
            public bool isActive { get; set; }
        }

        public struct Vector
        {
            public Point point1 { get; set; }
            public Point point2 { get; set; }
            public bool Equals(Vector vector)
            {
                return vector.point1 == this.point1 || vector.point2 == this.point2;
            }
        }

        public struct Link
        {
            public Polygon road { get; set; }
            public PointCollection OriginalRoadPoints { get; set; }
            public List<Line> LinesSide1 { get; set; }
            public List<Line> LinesSide2 { get; set; }
            public List<Line> MiddleLines { get; set; }
            public List<LineStepper> LineSteppers { get; set; }
            public Switch SwitchIsTwoLaned { get; set; }
            public Rectangle RectangleCollision { get; set; }
        }

        public struct LineStepper
        {
            public Stepper Stepper { get; set; }
            public Vector Vector { get; set; }

            public CheckBox CheckBoxTwoLaned { get; set; }

        }
    }
}
