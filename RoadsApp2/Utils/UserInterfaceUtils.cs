using Microsoft.Maui.Controls.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RoadsApp2.Utils.Enums;
using static RoadsApp2.Utils.Structs;
using Node = RoadsApp2.Utils.Structs.Node;
using static RoadsApp2.Utils.Utils;

namespace RoadsApp2.Utils
{
    internal class UserInterfaceUtils
    {
        public static void ToggleImageButtons(List<ImageButton> imgButtons)
        {
            foreach (ImageButton imageButton in imgButtons)
            {
                imageButton.IsVisible = !imageButton.IsVisible;
            }
        }

        public static Rectangle GetRectangle(Rect rect, EventHandler<TappedEventArgs> targetEvent = null)
        {
            Rectangle rectangle = new Rectangle()
            {
                Fill = Color.FromRgb(137, 137, 137),
                HeightRequest = rect.Width,
                WidthRequest = rect.Height,
                Stroke = Brush.Red,
                StrokeThickness = 0,
                ZIndex = 3
            };
            if (targetEvent != null)
            {
                TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.Tapped += targetEvent;
                rectangle.GestureRecognizers.Add(tapGestureRecognizer);
            }
            return rectangle;
        }

        public static Node GetNodeFromRectangle(Rectangle rectangle, List<Node> nodes)
        {
            foreach (Node node in nodes)
            {
                if (node.rectangle.Equals(rectangle))
                {
                    return node;
                }
            }
            return new Node();
        }

        public static bool IsRectangleExistInNodes(Rectangle rectangle, List<Node> nodes)
        {
            foreach (Node node in nodes)
            {
                if (node.rectangle.Equals(rectangle))
                {
                    return true;
                }
            }
            return false;
        }

        public static Link GetLinkByRoad(Polygon targetRoad, List<Link> links)
        {
            foreach (Link link in links)
            {
                if (link.road.Equals(targetRoad))
                {
                    return link;
                }
            }
            return new Link();
        }

        public static void SetImageButtonsType(string source, List<Node> nodes, Rectangle rectangleExcept = null)
        {
            rectangleExcept ??= new Rectangle();
            foreach (Node node in nodes)
            {
                if (!node.rectangle.Equals(rectangleExcept))
                {
                    foreach (ImageButton imageButton in node.imageButtons)
                    {
                        imageButton.Source = source;
                    }
                }
            }
        }

        public static bool IsFlagBelongsToSameNode(ImageButton imageButton1, ImageButton imageButton2, List<Node> nodes)
        {
            Node node = GetNodeFromImageButton(imageButton1, nodes);
            return node.imageButtons.Contains(imageButton2);
        }

        public static Rectangle GetRectangleFromImageButton(ImageButton imageButton, List<Node> nodes)
        {
            foreach (Node node in nodes)
            {
                if (node.imageButtons.Contains(imageButton))
                {
                    return node.rectangle;
                }
            }
            return null;
        }

        public static void SetImageButtonsVisibility(List<ImageButton> imageButtons, bool isVisible)
        {
            foreach (ImageButton imageButton in imageButtons)
            {
                imageButton.IsVisible = isVisible;
            }
        }

        public static Node GetNodeFromImageButton(ImageButton imageButton, List<Node> nodes)
        {
            foreach (Node node in nodes)
            {
                if (node.imageButtons.IndexOf(imageButton) != -1)
                {
                    return node;
                }
            }
            return new Node();
        }

        public static bool IsOrientationsParallel(Orientation orientation1, Orientation orientation2)
        {
            return (double)orientation1 + (double)GetReversedOrientation(orientation1) == (double)orientation2 + (double)GetReversedOrientation(orientation2);
        }

        public static Polygon DrawRoad(LineCoords lineCoordsStart, LineCoords lineCoordsDest,
            Orientation orientationStart, Orientation orientationDest)
        {
            if ((double)orientationDest + (double)orientationStart == (double)Orientation.Right + (double)Orientation.Up || (double)orientationDest + (double)orientationStart == (double)Orientation.Left + (double)Orientation.Down)
            {
                LineCoords temp = lineCoordsStart;
                lineCoordsStart.coord1 = temp.coord2;
                lineCoordsStart.coord2 = temp.coord1;
            }

            PointCollection points = new[]
            {
            new Point(lineCoordsStart.coord1.X, lineCoordsStart.coord1.Y),
            new Point(lineCoordsDest.coord1.X, lineCoordsDest.coord1.Y),
            new Point(lineCoordsDest.coord2.X, lineCoordsDest.coord2.Y),
            new Point(lineCoordsStart.coord2.X, lineCoordsStart.coord2.Y),
            new Point(lineCoordsStart.coord1.X, lineCoordsStart.coord1.Y)
            };

            Polygon polygon = new Polygon()
            {
                Points = points,
                Fill = Color.FromRgb(137, 137, 137),
                Stroke = Brush.Gray,
                StrokeThickness = 0,
                IsEnabled = true
            };
            //links.Add(new Link() { road = polygon });

            //DrawLines(GetLinkByRoad(polygon, links), 3, 2);

            //Brush lineColor = Brush.White;
            //Line line = new Line()
            //{
            //    Fill = lineColor,
            //    Stroke = lineColor,
            //    StrokeThickness = 3,
            //    IsEnabled = false,
            //    ZIndex = 2,
            //    X1 = (lineCoordsStart.coord1.X + lineCoordsStart.coord2.X) / 2,
            //    Y1 = (lineCoordsStart.coord1.Y + lineCoordsStart.coord2.Y) / 2,
            //    X2 = (lineCoordsDest.coord1.X + lineCoordsDest.coord2.X) / 2,
            //    Y2 = (lineCoordsDest.coord1.Y + lineCoordsDest.coord2.Y) / 2,
            //};

            //absoluteLayout.Add(line);

            return polygon;
        }
    }
}
