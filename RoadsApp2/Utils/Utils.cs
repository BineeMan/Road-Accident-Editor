﻿using System;
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
        public static LineCoords GetLineCoordsForOrientation(Orientation orientation, Rect rect, bool isInverted = false)
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
                    startPoint1.X = rect.X;
                    startPoint1.Y = rect.Y;
                    startPoint2.X = rect.X + rect.Width;
                    startPoint2.Y = rect.Y;
                    break;

                case Orientation.Down:
                    startPoint1.X = rect.X;
                    startPoint1.Y = rect.Y + rect.Height;
                    startPoint2.X = rect.X + rect.Width;
                    startPoint2.Y = rect.Y + rect.Height;
                    break;

                case Orientation.Right:
                    startPoint1.X = rect.X + rect.Width;
                    startPoint1.Y = rect.Y;
                    startPoint2.X = rect.X + rect.Width;
                    startPoint2.Y = rect.Y + rect.Height;
                    break;

                case Orientation.Left:
                    startPoint1.X = rect.X;
                    startPoint1.Y = rect.Y;
                    startPoint2.X = rect.X;
                    startPoint2.Y = rect.Y + rect.Height;
                    break;

                default: break;
            }
            LineCoords lineCoords = new LineCoords();
            lineCoords.coord1 = startPoint1;
            lineCoords.coord2 = startPoint2;
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
