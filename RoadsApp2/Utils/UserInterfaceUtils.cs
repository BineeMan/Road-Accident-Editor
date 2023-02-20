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
using System.Diagnostics;
using Microsoft.Maui.Controls;

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
                //Fill = Brush.Red,
                HeightRequest = rect.Height,
                WidthRequest = rect.Width,
                Stroke = Brush.Red,
                StrokeThickness = 0,
                ZIndex = 3,
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

        public static void SetSteppersVisibility(List<Stepper> steppers, bool isVisible)
        {
            foreach (Stepper stepper in steppers)
            {
                stepper.IsVisible = isVisible;
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

        public static Polygon DrawRoad(Vector lineCoordsStart, Vector lineCoordsDest,
            Orientation orientationStart, Orientation orientationDest)
        {
            if ((double)orientationDest + (double)orientationStart == (double)Orientation.Right + (double)Orientation.Up || (double)orientationDest + (double)orientationStart == (double)Orientation.Left + (double)Orientation.Down)
            {
                Vector temp = lineCoordsStart;
                lineCoordsStart.point1 = temp.point2;
                lineCoordsStart.point2 = temp.point1;
            }

            PointCollection points = new[]
            {
                new Point(lineCoordsStart.point1.X, lineCoordsStart.point1.Y),
                new Point(lineCoordsDest.point1.X, lineCoordsDest.point1.Y),
                new Point(lineCoordsDest.point2.X, lineCoordsDest.point2.Y),
                new Point(lineCoordsStart.point2.X, lineCoordsStart.point2.Y),
                new Point(lineCoordsStart.point1.X, lineCoordsStart.point1.Y)
            };

            Polygon polygon = new Polygon()
            {
                Points = points,
                Fill = Color.FromRgb(137, 137, 137),
                Stroke = Brush.Gray,
                StrokeThickness = 0,
                IsEnabled = true,
            };

            return polygon;
        }
        public static void ToggleSteppersVisibility(List<Link> links, bool visibility)
        {
            if (links != null)
            {
                foreach (Link link in links)
                {
                    foreach (LineStepper lineStepper in link.LineSteppers)
                    {
                        lineStepper.Stepper.IsVisible = visibility;
                    }
                }
            }
        }

        public static LineStepper GetLineStepperFromLinks(Stepper stepper, List<Link> links)
        {
            for (int i = 0; i < links.Count; i++)
            {
                for (int j = 0; j < links[i].LineSteppers.Count; j++)
                {
                    if (links[i].LineSteppers[j].Stepper.Equals(stepper))
                    {
                        return links[i].LineSteppers[j];
                    }
                }
            }
            return new LineStepper();
        }

        public static Link GetLinkFromLineStepper2(LineStepper targetLineStepper, List<Link> links)
        {
            foreach (Link link in links)
            {
                foreach (LineStepper lineStapper1 in link.LineSteppers)
                {
                    if (lineStapper1.Stepper.Equals(targetLineStepper.Stepper))
                    {
                        return link;
                    }
                }
            }
            return new Link();
        }

        public static Link GetLinkFromLineStepper(LineStepper lineStepper, List<Link> links)
        {
            for (int i = 0; i < links.Count; i++)
            {
                for (int j = 0; j < links[i].LineSteppers.Count; j++)
                {
                    if (links[i].LineSteppers[j].Stepper.Equals(lineStepper.Stepper))
                    {
                        return links[i];
                    }
                }
            }
            Debug.WriteLine("new link");
            return new Link();
        }

        public static Line DrawTrajectory(Point point1, Point point2)
        {
            Brush lineColor = Brush.Red;
            Line line = new Line()
            {
                Fill = lineColor,
                Stroke = lineColor,
                StrokeThickness = 3,
                IsEnabled = false,
                ZIndex = 5,
                StrokeDashArray = { 2, 2 },
                StrokeDashOffset = 10,
                X1 = point1.X,
                Y1 = point1.Y,
                X2 = point2.X,
                Y2 = point2.Y,
            };
            return line;
        }
    }


}
