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
        public static void ToggleImageButtons(List<Image> imgButtons)
        {
            foreach (Image imageButton in imgButtons)
            {
                if (imageButton.IsEnabled)
                    imageButton.IsVisible = !imageButton.IsVisible;
            }
        }

        public static Rectangle GetRectangle(Rect rect,
            EventHandler<TappedEventArgs> tappedEvent = null,
            EventHandler<PanUpdatedEventArgs> panEvent = null )
        {

            Rectangle rectangle = new()
            {
                Fill = Color.FromRgb(137, 137, 137),
                //InputTransparent = true,
                //Background = Brush.Red,
                //HeightRequest = rect.Height,
                //WidthRequest = rect.Width,
                //Stroke = Color.FromRgb(137, 137, 137),
                Stroke = Brush.Transparent,
                StrokeThickness = 0,
                Background = Brush.Transparent,
                BackgroundColor = Brush.Transparent.Color,
                ZIndex = 3
            };
            if (tappedEvent != null)
            {
                TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.Tapped += tappedEvent;
                rectangle.GestureRecognizers.Add(tapGestureRecognizer);
            }
            if (panEvent != null)
            {
                PanGestureRecognizer panGestureRecognizer = new PanGestureRecognizer();
                panGestureRecognizer.PanUpdated += panEvent;
                rectangle.GestureRecognizers.Add(panGestureRecognizer);
            }
            return rectangle;
        }

        public static Node GetNodeFromRectangle(Rectangle rectangle, List<Node> nodes)
        {
            foreach (Node node in nodes)
            {
                if (node.Rectangle.Equals(rectangle))
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
                if (node.Rectangle.Equals(rectangle))
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
                if (link.Road.Equals(targetRoad))
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
                if (!node.Rectangle.Equals(rectangleExcept))
                {
                    foreach (Image imageButton in node.PlusButtons)
                    {
                        imageButton.Source = source;
                    }
                }
            }
        }

        public static bool IsFlagBelongsToSameNode(Image imageButton1, Image imageButton2, List<Node> nodes)
        {
            Node node = GetNodeFromImageButton(imageButton1, nodes);
            return node.PlusButtons.Contains(imageButton2);
        }

        public static void SetImageButtonsVisibility(List<Image> imageButtons, bool isVisible)
        {
            foreach (Image imageButton in imageButtons)
            {
                if (imageButton.IsEnabled)
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

        public static Node GetNodeFromImageButton(Image imageButton, List<Node> nodes)
        {
            foreach (Node node in nodes)
            {
                if (node.PlusButtons.IndexOf(imageButton) != -1)
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

        public static Polygon DrawRoad(Vector vectorStart, Vector vectorDestination,
            Orientation orientationStart, Orientation orientationDest)
        {
            if ((double)orientationDest + (double)orientationStart == (double)Orientation.Right + (double)Orientation.Up || (double)orientationDest + (double)orientationStart == (double)Orientation.Left + (double)Orientation.Down)
            {
                Vector temp = vectorStart;
                vectorStart.point1 = temp.point2;
                vectorStart.point2 = temp.point1;
            }

            PointCollection points = new[]
            {
                new Point(vectorStart.point1.X, vectorStart.point1.Y),
                new Point(vectorDestination.point1.X, vectorDestination.point1.Y),
                new Point(vectorDestination.point2.X, vectorDestination.point2.Y),
                new Point(vectorStart.point2.X, vectorStart.point2.Y),
                //new Point(vectorStart.point1.X, vectorStart.point1.Y)
            };

            Polygon polygon = new Polygon()
            {
                Points = points,
                Fill = Color.FromRgb(137, 137, 137),
                Stroke = Brush.Gray,
                StrokeThickness = 0,
                IsEnabled = false,
                ZIndex = 1
            };

            return polygon;
        }



        public static Rect GetCollision(Polygon polygon)
        {
            PointCollection points = polygon.Points;
            Point closestPoint = new Point(Int32.MaxValue, Int32.MaxValue);
            Point farestPoint = new Point(0, 0);
            foreach (Point point in points)
            {
                if (point.X > farestPoint.X)
                    farestPoint.X = point.X;
                if (point.X < closestPoint.X)
                    closestPoint.X = point.X;

                if (point.Y > farestPoint.Y)
                    farestPoint.Y = point.Y;
                if (point.Y < closestPoint.Y)
                    closestPoint.Y = point.Y;
            }
            Rect collision = new Rect()
            {
                X = closestPoint.X,
                Y = closestPoint.Y,
                Width = Math.Abs(closestPoint.X - farestPoint.X),
                Height = Math.Abs(closestPoint.Y - farestPoint.Y),
            };
            return collision;
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
                        //if (lineStepper.CheckBoxTwoLaned != null)
                        //{
                        //    lineStepper.CheckBoxTwoLaned.IsVisible = visibility;
                        //}
                    }
                }
            }
        }

        public static void ToggleSteppersVisibility2(List<LineStepper> lineSteppers, bool visibility)
        {
            if (lineSteppers != null)
            {
                foreach (LineStepper lineStepper in lineSteppers)
                {
                    lineStepper.Stepper.IsVisible = visibility;
                    //if (lineStepper.CheckBoxTwoLaned != null)
                    //{
                    //    lineStepper.CheckBoxTwoLaned.IsVisible = visibility;
                    //}
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

        public static Link GetLinkByCollision(Rectangle collision, List<Link> links)
        {
            foreach (Link link in links)
            {
                if (link.RectangleCollision.Equals(collision))
                {
                    return link;
                }
            }
            return new Link();
        }

        public static Link GetLinkByCheckBox(CheckBox checkBox, List<Link> links)
        {
            for (int i = 0; i < links.Count; i++)
            {
                for (int j = 0; j < links[i].LineSteppers.Count; j++)
                {
                    //if (links[i].LineSteppers[j].CheckBoxTwoLaned != null)
                    //{
                    //    if (links[i].LineSteppers[j].CheckBoxTwoLaned.Equals(checkBox))
                    //    {
                    //        return links[i];
                    //    }
                    //}
                }
            }
            return new Link();
        }

        public static Line DrawLine(Vector vector)
        {
            Brush lineColor = Brush.White;
            Line line = new Line()
            {
                Fill = lineColor,
                Stroke = lineColor,
                StrokeThickness = 3,
                IsEnabled = false,
                ZIndex = 10,

                StrokeDashArray = { 2, 2 },
                StrokeDashOffset = 10,
                X1 = vector.point1.X,
                Y1 = vector.point1.Y,
                X2 = vector.point2.X,
                Y2 = vector.point2.Y,
            };
            return line;
        }


        public static void RedrawRoad(ref Polygon road, Vector newDestination)
        {
            road.Points[1] = newDestination.point1;
            road.Points[2] = newDestination.point2;
        }

        public static List<Node> GetAllNodesByLink(Link link, List<Node> nodes)
        {
            var foundNodes = new List<Node>();
            for (int i = 0; i < nodes.Count; i++)
            {
                for (int j = 0; j < nodes[i].Roads.Count; j++)
                {
                    if (nodes[i].Roads[j].Equals(link))
                        foundNodes.Add(nodes[i]);
                }
            }
            return foundNodes;
        }

    }
}
