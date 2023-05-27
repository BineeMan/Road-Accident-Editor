using Microsoft.Maui.Controls.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RoadsApp2.Utils.Enums;
using static RoadsApp2.Utils.Structs;

namespace RoadsApp2.Utils
{
    internal class Utils
    {
        public static Vector GetVectorForOrientation(Orientation orientation, Rect rect, bool isInverted = false)
        {
            if (isInverted)
            {
                orientation = GetReversedOrientation(orientation);
            }
            Point startPoint1 = new Point();
            Point startPoint2 = new Point();

            switch (orientation)
            {
                case Orientation.Up:
                    startPoint1.X = rect.X - 1;
                    startPoint1.Y = rect.Y;
                    startPoint2.X = rect.X + rect.Width;
                    startPoint2.Y = rect.Y;
                    break;

                case Orientation.Down:
                    startPoint1.X = rect.X - 1;
                    startPoint1.Y = rect.Y + rect.Height - 1;
                    startPoint2.X = rect.X + rect.Width;
                    startPoint2.Y = rect.Y + rect.Height - 1;
                    break;

                case Orientation.Right:
                    startPoint1.X = rect.X + rect.Width - 1;
                    startPoint1.Y = rect.Y - 1;
                    startPoint2.X = rect.X + rect.Width - 1;
                    startPoint2.Y = rect.Y + rect.Height;
                    break;

                case Orientation.Left:
                    startPoint1.X = rect.X;
                    startPoint1.Y = rect.Y - 1;
                    startPoint2.X = rect.X;
                    startPoint2.Y = rect.Y + rect.Height;
                    break;

                default: break;
            }
            Vector lineCoords = new Vector();
            lineCoords.point1 = startPoint1;
            lineCoords.point2 = startPoint2;
            return lineCoords;
        }

        public static Orientation GetReversedOrientation(Orientation orientation)
        {
            return orientation switch
            {
                Orientation.Up => Orientation.Down,
                Orientation.Down => Orientation.Up,
                Orientation.Left => Orientation.Right,
                Orientation.Right => Orientation.Left,
                _ => Orientation.Undefined,
            };
        }

        public static double GetLength(Point point1, Point point2)
        {
            double vectorA = point1.X - point2.X;
            double vectorB = point1.Y - point2.Y;
            return Math.Sqrt(Math.Pow(vectorA, 2) + Math.Pow(vectorB, 2));
        }

        public static Point GetSecondPoint(Point firstPoint, Polygon polygonRoad)
        {
            if (polygonRoad.Points.Count > 5)
                return Point.Zero;

            if (firstPoint == polygonRoad.Points[0])
                return polygonRoad.Points[3];
            else if (firstPoint == polygonRoad.Points[3])
                return polygonRoad.Points[0];
            else if (firstPoint == polygonRoad.Points[1])
                return polygonRoad.Points[2];
            else if (firstPoint == polygonRoad.Points[2])
                return polygonRoad.Points[1];

            return Point.Zero;
        }

        public static Point GetRatioForOrientation(Orientation orientation)
        {
            return orientation switch
            {
                Orientation.Up => new Point { X = 0.5, Y = 0 },
                Orientation.Down => new Point { X = 0.5, Y = 1 },
                Orientation.Left => new Point { X = 0, Y = 0.5 },
                Orientation.Right => new Point { X = 1, Y = 0.5 },
                Orientation.Undefined => new Point { X = 0.5, Y = 0.5 },
                _ => new Point { X = 0.5, Y = 0.5 },
            };
        }
    }
}
