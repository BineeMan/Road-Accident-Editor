using Microsoft.Maui.Controls.Shapes;

namespace RoadsApp2.Utils
{
    internal class Structs
    {
        public struct Node
        {
            public Rectangle rectangle;
            public List<ImageButton> imageButtons;
            public List<Link> roads;
            public bool isActive;
        }

        public struct Vector
        {
            public Point point1;
            public Point point2;
            public bool Equals(Vector vector)
            {
                return vector.point1 == this.point1 || vector.point2 == this.point2;
            }
        }

        public struct Link
        {
            public Polygon road;
            public List<Line> LinesSide1;
            public List<Line> LinesSide2;
            public List<Line> MiddleLines;
            public List<LineStepper> LineSteppers;
            public bool IsTwoLaned;
        }

        public struct LineStepper
        {
            public Stepper Stepper;
            public Vector Vector;

        }
    }
}
