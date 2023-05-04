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
                    startPoint1.X = rect.X-1;
                    startPoint1.Y = rect.Y;
                    startPoint2.X = rect.X + rect.Width;
                    startPoint2.Y = rect.Y;
                    break;

                case Orientation.Down:
                    startPoint1.X = rect.X-1;
                    startPoint1.Y = rect.Y + rect.Height - 1;
                    startPoint2.X = rect.X + rect.Width;
                    startPoint2.Y = rect.Y + rect.Height - 1;
                    break;

                case Orientation.Right:
                    startPoint1.X = rect.X + rect.Width-1;
                    startPoint1.Y = rect.Y - 1;
                    startPoint2.X = rect.X + rect.Width-1;
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

    }
}
