﻿using Microsoft.Maui;
using Microsoft.Maui.Controls.Shapes;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using static RoadsApp2.Utils.Structs;

namespace RoadsApp2.XMLClasses
{
    internal class XMLConverter
    {
        private AbsoluteLayout FormAbsoluteLayout { get; set; }

        private XmlDocument XmlDoc { get; set; }

        private XmlElement XmlRoot { get; set; }

        private string MauiTapGestureRecogniserName { get; } = "Microsoft.Maui.Controls.TapGestureRecognizer";

        private string MauiPanGestureRecogniserName { get; } = "Microsoft.Maui.Controls.PanGestureRecognizer";


        private MainPage MainPage { get; set; }

        public XMLConverter(AbsoluteLayout absoluteLayout, MainPage mainPage)
        {
            this.FormAbsoluteLayout = absoluteLayout;
            this.MainPage = mainPage;

            XmlDoc = new XmlDocument();

            XmlDoc.AppendChild(XmlDoc.CreateElement("RoadElements"));
            XmlRoot = XmlDoc.DocumentElement;

            XmlDeclaration xmlDeclaration = XmlDoc.CreateXmlDeclaration("1.0", "utf-8", "yes");
            XmlDoc.InsertBefore(xmlDeclaration, XmlRoot);
        }

        public bool SaveDocumentOnDisk(string filePath)
        {
            try
            {
                XmlDoc.Save(filePath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void AddNewAttribute(string name, string value, XmlElement ElementAppendTo)
        {
            XmlAttribute xmlAttribute = XmlDoc.CreateAttribute(name);
            XmlText xmlAttributeText = XmlDoc.CreateTextNode(value);
            xmlAttribute.AppendChild(xmlAttributeText);
            ElementAppendTo.Attributes.Append(xmlAttribute);
        }

        public void ConvertNodesToXML(List<Node> Nodes)
        {
            XmlElement NodesElement = XmlDoc.CreateElement("Nodes");
            XmlRoot.AppendChild(NodesElement);

            #region Nodes
            foreach (Node node in Nodes)
            {
                XmlElement Node = XmlDoc.CreateElement("Node");
                NodesElement.AppendChild(Node);

                #region Rectangle
                XmlElement Rectangle = XmlDoc.CreateElement("Rectangle");
                Node.AppendChild(Rectangle);

                SolidColorBrush solidColorBrush = (SolidColorBrush)node.Rectangle.Fill;
                AddNewAttribute("Fill", solidColorBrush.Color.ToHex().ToString(), Rectangle);

                AddNewAttribute("HeightRequest", node.Rectangle.HeightRequest.ToString(), Rectangle);

                AddNewAttribute("WidthRequest", node.Rectangle.WidthRequest.ToString(), Rectangle);

                AddNewAttribute("HeightRequest", node.Rectangle.HeightRequest.ToString(), Rectangle);

                solidColorBrush = (SolidColorBrush)node.Rectangle.Stroke;
                AddNewAttribute("Stroke", solidColorBrush.Color.ToHex().ToString(), Rectangle);

                AddNewAttribute("StrokeThickness", node.Rectangle.StrokeThickness.ToString(), Rectangle);

                AddNewAttribute("ZIndex", node.Rectangle.ZIndex.ToString(), Rectangle);

                Rect rect1 = FormAbsoluteLayout.GetLayoutBounds((IView)node.Rectangle);
                AddNewAttribute("rect", rect1.ToString(), Rectangle);

                XmlElement GestureRecognizers = XmlDoc.CreateElement("GestureRecognizers");

                foreach (GestureRecognizer gestureRecognizer in node.Rectangle.GestureRecognizers)
                {
                    XmlElement GestureRecognizerName = XmlDoc.CreateElement(gestureRecognizer.ToString());
                    if (gestureRecognizer.ToString().Equals("Microsoft.Maui.Controls.TapGestureRecognizer"))
                    {
                        AddNewAttribute("Tapped", "Crossroads_Tapped", GestureRecognizerName);
                    }
                    else if (gestureRecognizer.ToString().Equals("Microsoft.Maui.Controls.PanGestureRecognizer"))
                    {
                        AddNewAttribute("PanUpdated", "PlusImagePuttonPanGesture_PanUpdated", GestureRecognizerName);
                    }
                    else
                    {
                        AddNewAttribute("UnknownGesture", "null", GestureRecognizerName);
                    }
                    GestureRecognizers.AppendChild(GestureRecognizerName);
                }
                Rectangle.AppendChild(GestureRecognizers);
                Node.AppendChild(Rectangle);
                #endregion

                #region PlusButtons
                XmlElement PlusButtons = XmlDoc.CreateElement("PlusButtons");
                Node.AppendChild(PlusButtons);

                foreach (Image plusButton in node.PlusButtons)
                {
                    XmlElement Image = XmlDoc.CreateElement("Image");
                    PlusButtons.AppendChild(Image);

                    AddNewAttribute("Source", plusButton.Source.ToString().Replace("File: ", ""), Image);

                    AddNewAttribute("WidthRequest", plusButton.WidthRequest.ToString(), Image);

                    AddNewAttribute("HeightRequest", plusButton.HeightRequest.ToString(), Image);

                    AddNewAttribute("Rotation", plusButton.Rotation.ToString(), Image);

                    AddNewAttribute("AnchorY", plusButton.AnchorY.ToString(), Image);

                    AddNewAttribute("IsVisible", plusButton.IsVisible.ToString(), Image);

                    AddNewAttribute("ZIndex", plusButton.ZIndex.ToString(), Image);

                    AddNewAttribute("Rotation", plusButton.Rotation.ToString(), Image);

                    Rect rect = FormAbsoluteLayout.GetLayoutBounds(plusButton);
                    AddNewAttribute("rect", rect.ToString(), Image);

                    XmlElement GestureRecognizers1 = XmlDoc.CreateElement("GestureRecognizers");
                    foreach (GestureRecognizer gestureRecognizer in plusButton.GestureRecognizers)
                    {
                        XmlElement GestureRecognizerName = XmlDoc.CreateElement(gestureRecognizer.ToString());
                        if (gestureRecognizer.ToString().Equals("Microsoft.Maui.Controls.TapGestureRecognizer"))
                        {
                            AddNewAttribute("Tapped", "ImgButtonPlus_Tapped", GestureRecognizerName);
                        }
                        else if (gestureRecognizer.ToString().Equals("Microsoft.Maui.Controls.PanGestureRecognizer"))
                        {
                            AddNewAttribute("PanUpdated", "PlusImagePuttonPanGesture_PanUpdated", GestureRecognizerName);
                        }
                        else
                        {
                            AddNewAttribute("UnknownGesture", "null", GestureRecognizerName);
                        }
                        GestureRecognizers1.AppendChild(GestureRecognizerName);
                    }
                    Image.AppendChild(GestureRecognizers1);
                }
                #endregion

                #region Links
                XmlElement Links = XmlDoc.CreateElement("Links");

                Node.AppendChild(Links);

                foreach (Link link in node.Roads)
                {
                    XmlElement Link = XmlDoc.CreateElement("Link");
                    Links.AppendChild(Link);

                    #region ROAD
                    XmlElement Road = XmlDoc.CreateElement("Road");
                    Link.AppendChild(Road);

                    solidColorBrush = (SolidColorBrush)link.Road.Fill;
                    AddNewAttribute("Fill", solidColorBrush.Color.ToHex().ToString(), Road);

                    solidColorBrush = (SolidColorBrush)link.Road.Stroke;
                    AddNewAttribute("Stroke", solidColorBrush.Color.ToHex().ToString(), Road);

                    AddNewAttribute("StrokeThickness", link.Road.StrokeThickness.ToString(), Road);

                    AddNewAttribute("IsEnabled", link.Road.IsEnabled.ToString(), Road);

                    AddNewAttribute("ZIndex", link.Road.ZIndex.ToString(), Road);

                    XmlElement PointsCollection = XmlDoc.CreateElement("PointsCollection");
                    Road.AppendChild(PointsCollection);
                    foreach (Point point in link.Road.Points)
                    {
                        XmlElement Point = XmlDoc.CreateElement("Point");
                        PointsCollection.AppendChild(Point);
                        AddNewAttribute("X", point.X.ToString(), Point);

                        AddNewAttribute("Y", point.Y.ToString(), Point);
                    }
                    #endregion

                    #region OriginalRoadPoints
                    XmlElement OriginalRoadPoints = XmlDoc.CreateElement("OriginalRoadPoints");
                    Link.AppendChild(OriginalRoadPoints);
                    foreach (Point point in link.OriginalRoadPoints)
                    {
                        XmlElement Point = XmlDoc.CreateElement("Point");
                        PointsCollection.AppendChild(Point);

                        AddNewAttribute("X", point.X.ToString(), Point);

                        AddNewAttribute("Y", point.Y.ToString(), Point);

                        OriginalRoadPoints.AppendChild(Point);
                    }
                    #endregion

                    #region LinesSide1
                    XmlElement LinesSide1 = XmlDoc.CreateElement("LinesSide1");
                    Link.AppendChild(LinesSide1);

                    foreach (Line line in link.LinesSide1)
                    {
                        XmlElement Line = XmlDoc.CreateElement("Line");
                        LinesSide1.AppendChild(Line);

                        solidColorBrush = (SolidColorBrush)line.Fill;
                        AddNewAttribute("Fill", solidColorBrush.Color.ToHex().ToString(), Line);

                        solidColorBrush = (SolidColorBrush)line.Stroke;
                        AddNewAttribute("Stroke", solidColorBrush.Color.ToHex().ToString(), Line);

                        AddNewAttribute("StrokeThickness", line.StrokeThickness.ToString(), Line);

                        AddNewAttribute("IsEnabled", line.IsEnabled.ToString(), Line);

                        AddNewAttribute("ZIndex", line.ZIndex.ToString(), Line);

                        string temp = "";
                        foreach (double num in line.StrokeDashArray)
                        {
                            temp += num.ToString() + " ";
                        }

                        AddNewAttribute("StrokeDashArray", temp, Line);

                        AddNewAttribute("StrokeDashOffset", line.StrokeDashOffset.ToString(), Line);

                        AddNewAttribute("X1", line.X1.ToString(), Line);

                        AddNewAttribute("Y1", line.Y1.ToString(), Line);

                        AddNewAttribute("X2", line.X2.ToString(), Line);

                        AddNewAttribute("Y2", line.Y2.ToString(), Line);
                    }
                    #endregion

                    #region LinesSide2

                    XmlElement LinesSide2 = XmlDoc.CreateElement("LinesSide2");
                    Link.AppendChild(LinesSide2);

                    foreach (Line line in link.LinesSide2)
                    {
                        XmlElement Line = XmlDoc.CreateElement("Line");
                        LinesSide2.AppendChild(Line);

                        solidColorBrush = (SolidColorBrush)line.Fill;
                        AddNewAttribute("Fill", solidColorBrush.Color.ToHex().ToString(), Line);

                        solidColorBrush = (SolidColorBrush)line.Stroke;
                        AddNewAttribute("Stroke", solidColorBrush.Color.ToHex().ToString(), Line);

                        AddNewAttribute("StrokeThickness", line.StrokeThickness.ToString(), Line);

                        AddNewAttribute("IsEnabled", line.IsEnabled.ToString(), Line);

                        AddNewAttribute("ZIndex", line.ZIndex.ToString(), Line);

                        string temp = "";
                        foreach (double num in line.StrokeDashArray)
                        {
                            temp += num.ToString() + " ";
                        }

                        AddNewAttribute("StrokeDashArray", temp, Line);

                        AddNewAttribute("StrokeDashOffset", line.StrokeDashOffset.ToString(), Line);

                        AddNewAttribute("X1", line.X1.ToString(), Line);

                        AddNewAttribute("Y1", line.Y1.ToString(), Line);

                        AddNewAttribute("X2", line.X2.ToString(), Line);

                        AddNewAttribute("Y2", line.Y2.ToString(), Line);
                    }
                    #endregion

                    #region MiddleLines

                    XmlElement MiddleLines = XmlDoc.CreateElement("MiddleLines");
                    Link.AppendChild(MiddleLines);

                    foreach (Line line in link.MiddleLines)
                    {
                        XmlElement Line = XmlDoc.CreateElement("Line");
                        MiddleLines.AppendChild(Line);

                        solidColorBrush = (SolidColorBrush)line.Fill;
                        AddNewAttribute("Fill", solidColorBrush.Color.ToHex().ToString(), Line);

                        solidColorBrush = (SolidColorBrush)line.Stroke;
                        AddNewAttribute("Stroke", solidColorBrush.Color.ToHex().ToString(), Line);

                        AddNewAttribute("StrokeThickness", line.StrokeThickness.ToString(), Line);

                        AddNewAttribute("IsEnabled", line.IsEnabled.ToString(), Line);

                        AddNewAttribute("ZIndex", line.ZIndex.ToString(), Line);

                        string temp = "";
                        foreach (double num in line.StrokeDashArray)
                        {
                            temp += num.ToString() + " ";
                        }

                        AddNewAttribute("StrokeDashArray", temp, Line);

                        AddNewAttribute("StrokeDashOffset", line.StrokeDashOffset.ToString(), Line);

                        AddNewAttribute("X1", line.X1.ToString(), Line);

                        AddNewAttribute("Y1", line.Y1.ToString(), Line);

                        AddNewAttribute("X2", line.X2.ToString(), Line);

                        AddNewAttribute("Y2", line.Y2.ToString(), Line);
                    }
                    #endregion

                    #region LineSteppers
                    XmlElement LineSteppers = XmlDoc.CreateElement("LineSteppers");
                    Link.AppendChild(LineSteppers);

                    foreach (LineStepper lineStepper in link.LineSteppers)
                    {
                        XmlElement lineStepperElement = XmlDoc.CreateElement("LineStepper");

                        XmlElement stepperElement = XmlDoc.CreateElement("Stepper");
                        LineSteppers.AppendChild(lineStepperElement);

                        lineStepperElement.AppendChild(stepperElement);

                        AddNewAttribute("Maximum", lineStepper.Stepper.Maximum.ToString(), stepperElement);

                        AddNewAttribute("Minimum", lineStepper.Stepper.Minimum.ToString(), stepperElement);

                        AddNewAttribute("HorizontalOptions", lineStepper.Stepper.HorizontalOptions.Alignment.ToString(), stepperElement);

                        AddNewAttribute("Scale", lineStepper.Stepper.Scale.ToString(), stepperElement);

                        AddNewAttribute("ZIndex", lineStepper.Stepper.ZIndex.ToString(), stepperElement);

                        AddNewAttribute("AnchorY", lineStepper.Stepper.AnchorY.ToString(), stepperElement);

                        AddNewAttribute("AnchorX", lineStepper.Stepper.AnchorX.ToString(), stepperElement);

                        AddNewAttribute("IsVisible", lineStepper.Stepper.IsVisible.ToString(), stepperElement);

                        AddNewAttribute("Value", lineStepper.Stepper.Value.ToString(), stepperElement);

                        string thickness = lineStepper.Stepper.Margin.HorizontalThickness.ToString() + " " + lineStepper.Stepper.Margin.VerticalThickness.ToString();
                        AddNewAttribute("Margin", thickness, stepperElement);

                        Rect rect2 = FormAbsoluteLayout.GetLayoutBounds(lineStepper.Stepper);
                        AddNewAttribute("rect", rect2.ToString(), stepperElement);

                        XmlElement Vector = XmlDoc.CreateElement("Vector");
                        lineStepperElement.AppendChild(Vector);

                        XmlElement point1 = XmlDoc.CreateElement("point1");
                        Vector.AppendChild(point1);
                        AddNewAttribute("X", lineStepper.Vector.point1.X.ToString(), point1);
                        AddNewAttribute("Y", lineStepper.Vector.point1.Y.ToString(), point1);

                        XmlElement point2 = XmlDoc.CreateElement("point2");
                        Vector.AppendChild(point2);
                        AddNewAttribute("X", lineStepper.Vector.point2.X.ToString(), point2);
                        AddNewAttribute("Y", lineStepper.Vector.point2.Y.ToString(), point2);

                    }
                    #endregion

                    #region RectangleCollision

                    XmlElement RectangleCollision = XmlDoc.CreateElement("RectangleCollision");
                    Link.AppendChild(RectangleCollision);
                    Rect rect = FormAbsoluteLayout.GetLayoutBounds(link.RectangleCollision);
                    AddNewAttribute("rect", rect.ToString(), RectangleCollision);

                    solidColorBrush = (SolidColorBrush)link.RectangleCollision.Background;
                    AddNewAttribute("Background", solidColorBrush.Color.ToHex().ToString(), RectangleCollision);

                    AddNewAttribute("ZIndex", link.RectangleCollision.ZIndex.ToString(), RectangleCollision);

                    GestureRecognizers = XmlDoc.CreateElement("GestureRecognizers");

                    foreach (GestureRecognizer gestureRecognizer in node.Rectangle.GestureRecognizers)
                    {
                        XmlElement GestureRecognizerName = XmlDoc.CreateElement(gestureRecognizer.ToString());
                        if (gestureRecognizer.ToString().Equals("Microsoft.Maui.Controls.TapGestureRecognizer"))
                        {
                            AddNewAttribute("Tapped", "RoadCollision_Tapped", GestureRecognizerName);
                        }
                        else
                        {
                            AddNewAttribute("UnknownGesture", "null", GestureRecognizerName);
                        }
                        GestureRecognizers.AppendChild(GestureRecognizerName);
                    }
                    RectangleCollision.AppendChild(GestureRecognizers);
                    #endregion
                }

                #endregion

            }
            #endregion

        }

        public void ConvertImagesToXML(List<Image> images)
        {
            XmlElement ImagesElement = XmlDoc.CreateElement("Images");
            XmlRoot.AppendChild(ImagesElement);
            foreach (Image image in images)
            {
                XmlElement Image = XmlDoc.CreateElement("Image");
                ImagesElement.AppendChild(Image);

                AddNewAttribute("Source", image.Source.ToString().Replace("File: ", ""), Image);

                AddNewAttribute("WidthRequest", image.WidthRequest.ToString(), Image);

                AddNewAttribute("HeightRequest", image.HeightRequest.ToString(), Image);

                AddNewAttribute("ZIndex", image.ZIndex.ToString(), Image);

                AddNewAttribute("Scale", image.Scale.ToString(), Image);

                AddNewAttribute("Rotation", image.Rotation.ToString(), Image);

                Rect rect = FormAbsoluteLayout.GetLayoutBounds(image);
                AddNewAttribute("rect", rect.ToString(), Image);

                XmlElement GestureRecognizers = XmlDoc.CreateElement("GestureRecognizers");

                foreach (GestureRecognizer gestureRecognizer in image.GestureRecognizers)
                {
                    XmlElement GestureRecognizerName = XmlDoc.CreateElement(gestureRecognizer.ToString());
                    if (gestureRecognizer.ToString().Equals("Microsoft.Maui.Controls.TapGestureRecognizer"))
                    {
                        AddNewAttribute("Tapped", "RoadObjectImage_Tapped", GestureRecognizerName);
                    }
                    else if (gestureRecognizer.ToString().Equals("Microsoft.Maui.Controls.PanGestureRecognizer"))
                    {
                        AddNewAttribute("PanUpdated", "RoadObjectPanGesture_PanUpdated", GestureRecognizerName);
                    }
                    else
                    {
                        AddNewAttribute("UnknownGesture", "null", GestureRecognizerName);
                    }
                    GestureRecognizers.AppendChild(GestureRecognizerName);
                }
            }
        }

        private Rect StringToRect(string rectString)
        {
            rectString = rectString.Replace(".", ",");
            var matches = Regex.Match(rectString, @"{X=(.*?) Y=(.*?) Width=(.*?) Height=(.*?)}");
            var rectNew = new Rect(Double.Parse(matches.Groups[1].Value),
                                Double.Parse(matches.Groups[2].Value),
                                Double.Parse(matches.Groups[3].Value),
                                Double.Parse(matches.Groups[4].Value));
            return rectNew;
        }

        private string GetValueFromAttribute(string attribute, XElement element) 
        {
            return element.Attribute(attribute).Value;
        }

        private Line GetLineFromElement(XElement element)
        {
            Line line = new()
            {
                Fill = Color.FromArgb(GetValueFromAttribute("Fill", element)),
                Stroke = Color.FromArgb(GetValueFromAttribute("Stroke", element)),
                StrokeThickness = Double.Parse(GetValueFromAttribute("StrokeThickness", element)),
                IsEnabled = Convert.ToBoolean(GetValueFromAttribute("IsEnabled", element)),
                ZIndex = int.Parse(GetValueFromAttribute("ZIndex", element)),
                StrokeDashOffset = Double.Parse(GetValueFromAttribute("StrokeDashOffset", element)),
                X1 = Double.Parse(GetValueFromAttribute("X1", element)),
                X2 = Double.Parse(GetValueFromAttribute("X2", element)),
                Y1 = Double.Parse(GetValueFromAttribute("Y1", element)),
                Y2 = Double.Parse(GetValueFromAttribute("Y2", element)),
            };
            if (element.Attribute("StrokeDashArray") != null)
            {
                string strokeDashArrayString = GetValueFromAttribute("StrokeDashArray", element);
                DoubleCollection strokeDashArray = new DoubleCollection();
                var matches = Regex.Match(strokeDashArrayString, @"(.*?) (.*?) ");
                strokeDashArray.Add(Double.Parse(matches.Groups[1].Value));
                strokeDashArray.Add(Double.Parse(matches.Groups[2].Value));
                line.StrokeDashArray = strokeDashArray;
            }
            return line;
        }

        private Vector GetVectorFromElement(XElement element)
        {
            XElement point1 = element.Element("point1");
            XElement point2 = element.Element("point2");
            Vector vector = new Vector() 
            {
                point1 = new Point(double.Parse(GetValueFromAttribute("X", point1)), double.Parse(GetValueFromAttribute("Y", point1))),
                point2 = new Point(double.Parse(GetValueFromAttribute("X", point2)), double.Parse(GetValueFromAttribute("Y", point2))),
            };
            return vector;
        }

        private Stepper GetStepperFromElement(XElement element)
        {
            Stepper stepper = new()
            {
                Maximum = double.Parse(GetValueFromAttribute("Maximum", element)),
                Minimum = double.Parse(GetValueFromAttribute("Minimum", element)),
                HorizontalOptions = LayoutOptions.Center,
                Scale = double.Parse(GetValueFromAttribute("Scale", element)),
                ZIndex = int.Parse(GetValueFromAttribute("ZIndex", element)),
                AnchorY = double.Parse(GetValueFromAttribute("AnchorY", element)),
                AnchorX = double.Parse(GetValueFromAttribute("AnchorX", element)),
                IsVisible = Convert.ToBoolean(GetValueFromAttribute("IsVisible", element)),
                Value = double.Parse(GetValueFromAttribute("Value", element)),

            };
            stepper.ValueChanged += MainPage.OnStepperValueChanged;
            FormAbsoluteLayout.Add(stepper);
            FormAbsoluteLayout.SetLayoutBounds(stepper, StringToRect(GetValueFromAttribute("rect", element)));
            return stepper;
        }

        private bool IsRoadUnique(Link linkCompare, List<Link> links, out Link foundLink)
        {
            int samePointsAmount;

            for (int i = 0; i < links.Count; i++)
            {
                samePointsAmount = 0;
                for (int j = 0; j < links[i].Road.Points.Count; j++)
                {
                    if (linkCompare.Road.Points[j].X == links[i].Road.Points[j].X && linkCompare.Road.Points[j].Y == links[i].Road.Points[j].Y)
                    {
                        samePointsAmount++;
                        //Debug.WriteLine($"{linkCompare.Road.Points[j].X}  {links[i].Road.Points[j].X}");
                        //Debug.WriteLine($"{linkCompare.Road.Points[j].Y}  {links[i].Road.Points[j].Y}");
                        //Debug.WriteLine($"{links[i].Road.Points.Count - 1}  {samePointsAmount}");
                    }
                }
                if (samePointsAmount == links[i].Road.Points.Count)
                {
                    foundLink = links[i];
                    return false;
                }
            }
            foundLink = new Link();
            return true;
        }

        public void ConvertXmlToViews(string XmlDocumentPath, out List<Node> nodes, out List<Image> images, out List<Link> links)
        {
            XDocument xDoc = XDocument.Load(XmlDocumentPath);

            nodes = new List<Node>();
            links = new List<Link>();
            images = new List<Image>();
            XElement RoadElements = xDoc.Element("RoadElements");
            
            #region Nodes
            XElement Nodes = RoadElements.Element("Nodes");
            foreach (XElement nodeElement in Nodes.Elements("Node"))
            {
                Node node = new Node();
                #region Rectangle
                XElement rectangleElement = nodeElement.Element("Rectangle");
                Rectangle rectangle = new Rectangle() 
                {
                    Fill = Color.FromArgb(GetValueFromAttribute("Fill", rectangleElement)),
                    WidthRequest = Double.Parse(GetValueFromAttribute("WidthRequest", rectangleElement)),
                    HeightRequest = Double.Parse(GetValueFromAttribute("HeightRequest", rectangleElement)),
                    Stroke = Color.FromArgb(GetValueFromAttribute("Stroke", rectangleElement)),
                    StrokeThickness = Double.Parse(GetValueFromAttribute("StrokeThickness", rectangleElement)),
                    ZIndex = int.Parse(GetValueFromAttribute("ZIndex", rectangleElement)),
                };

                Rect rect = StringToRect(GetValueFromAttribute("rect", rectangleElement));
                FormAbsoluteLayout.Add(rectangle);
                FormAbsoluteLayout.SetLayoutBounds(rectangle, rect);

                //XElement gestureRecognisersElement = rectangleElement.Element("GestureRecognizers");
                //XElement tapGestureRecognizerElement = gestureRecognisersElement.Element("Microsoft.Maui.Controls.TapGestureRecognizer");
                TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.Tapped += MainPage.Crossroads_Tapped;
                rectangle.GestureRecognizers.Add(tapGestureRecognizer);
                    
                node.Rectangle = rectangle;
                #endregion

                #region PlusButtons
                XElement PlusButtonsElement = nodeElement.Element("PlusButtons");
                node.PlusButtons = new List<Image>();
                foreach (XElement PlusButtonElement in PlusButtonsElement.Elements("Image"))
                {                    
                    Image imagePlusButton = new Image()
                    {
                        Source = GetValueFromAttribute("Source", PlusButtonElement),
                        WidthRequest = Double.Parse(GetValueFromAttribute("WidthRequest", PlusButtonElement)),
                        HeightRequest = Double.Parse(GetValueFromAttribute("HeightRequest", PlusButtonElement)),
                        ZIndex = Int32.Parse(GetValueFromAttribute("ZIndex", PlusButtonElement)),
                        Rotation = Double.Parse(GetValueFromAttribute("Rotation", PlusButtonElement)),
                        AnchorY = Double.Parse(GetValueFromAttribute("AnchorY", PlusButtonElement)),
                        IsVisible = Convert.ToBoolean(GetValueFromAttribute("IsVisible", PlusButtonElement)),
                    };

                    FormAbsoluteLayout.Add(imagePlusButton);
                    FormAbsoluteLayout.SetLayoutBounds(imagePlusButton, StringToRect(GetValueFromAttribute("rect", PlusButtonElement)));

                    tapGestureRecognizer = new TapGestureRecognizer();
                    tapGestureRecognizer.Tapped += MainPage.ImgButtonPlus_Tapped;
                    imagePlusButton.GestureRecognizers.Add(tapGestureRecognizer);

                    PanGestureRecognizer panGestureRecognizer = new PanGestureRecognizer();
                    panGestureRecognizer.PanUpdated += MainPage.PlusImagePuttonPanGesture_PanUpdated;
                    imagePlusButton.GestureRecognizers.Add(panGestureRecognizer);

                    node.PlusButtons.Add(imagePlusButton);
                }

                #endregion

                #region Links
                foreach (XElement LinksElement in nodeElement.Elements("Links"))
                {
                    Link link = new Link();
                    Debug.WriteLine(LinksElement.Name);
                    #region Road
                    XElement LinkElement = LinksElement.Element("Link");

                    XElement RoadElement = LinkElement.Element("Road");

                    XElement PointsElement = RoadElement.Element("PointsCollection");

                    PointCollection points = new();
                    foreach (XElement pointElement in PointsElement.Elements("Point"))
                    {

                        Point point = new Point()
                        {
                            X = double.Parse(GetValueFromAttribute("X", pointElement)),
                            Y = double.Parse(GetValueFromAttribute("Y", pointElement)),
                        };
                        points.Add(point);
                    }

                    Polygon polygonRoad = new()
                    {
                        Points = points,
                        Fill = Color.FromArgb(GetValueFromAttribute("Fill", RoadElement)),   
                        Stroke = Color.FromArgb(GetValueFromAttribute("Stroke", RoadElement)),
                        StrokeThickness = Double.Parse(GetValueFromAttribute("StrokeThickness", RoadElement)),
                        ZIndex = int.Parse(GetValueFromAttribute("ZIndex", RoadElement)),
                        IsEnabled = Convert.ToBoolean(GetValueFromAttribute("IsEnabled", RoadElement)),
                    };
                    link.Road = polygonRoad;

                    #endregion

                    // CheckIfRoadIsUnique
                    if (!IsRoadUnique(link, links, out Link foundLink))
                    {
                        //link NOT unique, existing link
                        node.Roads ??= new List<Link>();
                        node.Roads.Add(foundLink);

                        Debug.WriteLine("1");
                    }
                    else
                    {
                        Debug.WriteLine("2");
                        node.Roads ??= new List<Link>();

                        FormAbsoluteLayout.Add(link.Road);

                        #region OriginalRoadPoints
                        points = new PointCollection();
                        XElement OriginalRoadPointsElement = LinkElement.Element("OriginalRoadPoints");

                        foreach (XElement pointElement in OriginalRoadPointsElement.Elements("Point"))
                        {
                            Point point = new Point()
                            {
                                X = double.Parse(GetValueFromAttribute("X", pointElement)),
                                Y = double.Parse(GetValueFromAttribute("Y", pointElement)),
                            };
                            points.Add(point);
                            Debug.WriteLine(point.ToString());
                        }
                        link.OriginalRoadPoints = points;

                        #endregion

                        #region LinesSide1
                        link.LinesSide1 = new List<Line>();
                        XElement LinesSide1Element = LinkElement.Element("LinesSide1");
                        foreach (XElement lineElement in LinesSide1Element.Elements("Line"))
                        {
                            Line line = GetLineFromElement(lineElement);
                            link.LinesSide1.Add(line);
                            FormAbsoluteLayout.Add(line);
                        }
                        #endregion

                        #region LinesSide2
                        link.LinesSide2 = new List<Line>();
                        XElement LinesSide2Element = LinkElement.Element("LinesSide2");

                        foreach (XElement lineElement in LinesSide2Element.Elements("Line"))
                        {
                            Line line = GetLineFromElement(lineElement);
                            link.LinesSide2.Add(line);
                            FormAbsoluteLayout.Add(line);
                        }
                        #endregion

                        #region MiddleLines
                        link.MiddleLines = new List<Line>();
                        XElement MiddleLinesElement = LinkElement.Element("MiddleLines");
                        foreach (XElement lineElement in MiddleLinesElement.Elements("MiddleLines"))
                        {
                            Line line = GetLineFromElement(lineElement);
                            link.MiddleLines.Add(line);
                            FormAbsoluteLayout.Add(line);
                        }
                        #endregion

                        #region LineSteppers
                        link.LineSteppers = new List<LineStepper>();
                        XElement LineSteppersElement = LinkElement.Element("LineSteppers");

                        foreach (XElement LineStepperElement in LineSteppersElement.Elements("LineStepper"))
                        {
                            XElement stepperElement = LineStepperElement.Element("Stepper");
                            XElement vectorElement = LineStepperElement.Element("Vector");
                            XElement point1Element = vectorElement.Element("point1");
                            XElement point2Element = vectorElement.Element("point2");

                            LineStepper lineStepper = new LineStepper();
                            lineStepper.Vector = GetVectorFromElement(vectorElement);
                            lineStepper.Stepper = GetStepperFromElement(stepperElement);
                            link.LineSteppers.Add(lineStepper);
                            Debug.WriteLine("LineStepper");
                        }
                        #endregion

                        #region RectangleCollision
                        XElement rectangleCollisionElement = LinkElement.Element("RectangleCollision");
                        Rectangle rectangleCollision = new Rectangle()
                        {
                            Background = Brush.Transparent,
                            ZIndex = int.Parse(GetValueFromAttribute("ZIndex", rectangleCollisionElement))
                        };
                        tapGestureRecognizer = new TapGestureRecognizer();
                        tapGestureRecognizer.Tapped += MainPage.RoadCollision_Tapped;
                        rectangleCollision.GestureRecognizers.Add(tapGestureRecognizer);
                        link.RectangleCollision = rectangleCollision;
                        FormAbsoluteLayout.Add(rectangleCollision);
                        FormAbsoluteLayout.SetLayoutBounds(rectangleCollision,
                            StringToRect(GetValueFromAttribute("rect", rectangleCollisionElement)));
                        Debug.WriteLine(StringToRect(GetValueFromAttribute("rect", rectangleCollisionElement)));
                        #endregion


                        node.Roads.Add(link);
                        links.Add(link);
                    }
                }
                #endregion

                nodes.Add(node);
            }
            #endregion

            #region Images
            XElement Images = RoadElements.Element("Images");
            foreach (XElement imageElement in Images.Elements("Image"))
            {
                Image imageMain = new Image();
                XAttribute Source = imageElement.Attribute("Source");
                imageMain.Source = Source.Value.Replace("File: ", "");
                XAttribute WidthRequest = imageElement.Attribute("WidthRequest");
                imageMain.WidthRequest = Double.Parse(WidthRequest.Value);
                XAttribute HeightRequest = imageElement.Attribute("HeightRequest");
                imageMain.HeightRequest = Double.Parse(HeightRequest.Value);
                XAttribute ZIndex = imageElement.Attribute("ZIndex");
                imageMain.ZIndex = Int32.Parse(ZIndex.Value);
                XAttribute Rotation = imageElement.Attribute("Rotation");
                imageMain.Rotation = Double.Parse(Rotation.Value);
                XAttribute Scale = imageElement.Attribute("Scale");
                imageMain.Scale = Double.Parse(Scale.Value);
                XAttribute rect = imageElement.Attribute("rect");

                TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.Tapped += MainPage.RoadObjectImage_Tapped;
                PanGestureRecognizer panGestureRecognizer = new PanGestureRecognizer();
                panGestureRecognizer.PanUpdated += MainPage.RoadObjectPanGesture_PanUpdated;
                imageMain.GestureRecognizers.Add(tapGestureRecognizer);
                imageMain.GestureRecognizers.Add(panGestureRecognizer);

                FormAbsoluteLayout.Add(imageMain);
                FormAbsoluteLayout.SetLayoutBounds(imageMain, StringToRect(rect.Value));
            }
            #endregion
        }
    }
}