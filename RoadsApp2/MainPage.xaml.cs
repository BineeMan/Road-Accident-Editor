using System;
using System.Diagnostics;
using System.Windows.Input;
using Microsoft.Maui.Controls.Shapes;
using RoadsApp2.DataClasses;
using static RoadsApp2.Utils.Enums;
using static RoadsApp2.Utils.Structs;
using static RoadsApp2.Utils.UserInterfaceUtils;
using static RoadsApp2.Utils.Utils;
using Node = RoadsApp2.Utils.Structs.Node;

namespace RoadsApp2;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        ResetRoadElements();
    }

    private Node prevNode;

    private LineCoords lineCoords;

    private List<Node> nodes;

    private readonly Point[] pointOrientations = new Point[]
    {
        new Point { X = 0.5, Y = 0 },
        new Point { X = 1, Y = 0.5 },
        new Point { X = 0.5, Y = 1},
        new Point { X = 0, Y = 0.5}
    };

    private bool AddNewNodeFlag;

    private readonly int RectWidth = 100;

    private readonly int RectHeight = 100;

    private List<Link> links;

    private ImageButton currentImageButton;

    private Orientation currentOrientation;

    private Rectangle currentRectangle;

    private int tapAmounts;
    private void ResetRoadElements()
    {
        prevNode = new Node() { imageButtons = new List<ImageButton>() };
        lineCoords = new LineCoords();
        nodes = new List<Node>();
        AddNewNodeFlag = false;
        links = new List<Link>();
        currentImageButton = new ImageButton();
        currentOrientation = Orientation.Undefined;
        currentRectangle = new Rectangle();
        tapAmounts = 0;
    }

    ///<summary>
    ///This function adds new flags around a crossroads with except orientation which will be avoided
    ///</summary>
    private void AddFlagsAroundRectangle(Rectangle rectangle, Orientation orientationExcept = Orientation.Undefined, bool isVisible = true)
    {
        Node node = new Node()
        {
            rectangle = rectangle,
            imageButtons = new List<ImageButton>(),
            isActive = true,
            roads = new List<Polygon>()
        };
        Orientation rotation = Orientation.Up;
        foreach (Point point in pointOrientations)
        {
            if (rotation == orientationExcept)
            {
                rotation += 90;
                continue;
            }
            ImageButton imageButton = new ImageButton
            {
                Source = ButtonType.PlusBtn,
                WidthRequest = 45,
                HeightRequest = 45,
                Rotation = (double)rotation,
                ZIndex = 99,
                AnchorY = 1,
                IsVisible = isVisible
            };
            absoluteLayout.Add(imageButton);

            Rect rect = absoluteLayout.GetLayoutBounds(rectangle);

            double x = rect.X - imageButton.WidthRequest / 2 + rect.Width * point.X;
            double y = rect.Y - imageButton.HeightRequest + rect.Height * point.Y;

            absoluteLayout.SetLayoutBounds(imageButton,
                new Rect(x, y, imageButton.Width, imageButton.Height));
            imageButton.Clicked += ImgButton_Clicked;
            node.imageButtons.Add(imageButton);
            rotation += 90;
        }
        nodes.Add(node);
        if (isVisible)
        {
            prevNode = node;
        }
    }

    private void DrawLines(Link link, int leftAmount, int rightAmount)
    {
        if (link.lines != null)
        {
            foreach (Line line in link.lines)
            {
                absoluteLayout.Remove(line);
            }
        }
        PointCollection points = link.road.Points;
        LineCoords lineCoordsStart = new LineCoords();
        LineCoords lineCoordsDest = new LineCoords();
        lineCoordsStart.coord1 = points[0];
        lineCoordsDest.coord1 = points[1];
        lineCoordsDest.coord2 = points[2];
        lineCoordsStart.coord2 = points[3];
        lineCoordsStart.coord1 = points[4];

        lineCoordsStart.coord2 = new Point((lineCoordsStart.coord1.X + lineCoordsStart.coord2.X) / 2,
            (lineCoordsStart.coord1.Y + lineCoordsStart.coord2.Y) / 2);

        lineCoordsDest.coord2 = new Point((lineCoordsDest.coord1.X + lineCoordsDest.coord2.X) / 2,
           (lineCoordsDest.coord1.Y + lineCoordsDest.coord2.Y) / 2);

        Brush lineColor = Brush.White;

        int a = 1;
        int b = leftAmount;
        for (int i = a; i <= b; i++)
        {
            double l = (double)i / (double)b;
            double X = (lineCoordsStart.coord1.X + l * lineCoordsStart.coord2.X) / (1 + l);
            double Y = (lineCoordsStart.coord1.Y + l * lineCoordsStart.coord2.Y) / (1 + l);
            Debug.WriteLine($"l={l} x={X} y={Y}");
            Line line = new Line()
            {
                Fill = lineColor,
                Stroke = lineColor,
                StrokeThickness = 3,
                IsEnabled = false,
                ZIndex = 2,
                StrokeDashArray = { 2, 2 },
                StrokeDashOffset = 10,
                X1 = (lineCoordsStart.coord1.X + l * lineCoordsStart.coord2.X) / (1 + l),
                Y1 = (lineCoordsStart.coord1.Y + l * lineCoordsStart.coord2.Y) / (1 + l),
                X2 = (lineCoordsDest.coord1.X + l * lineCoordsDest.coord2.X) / (1 + l),
                Y2 = (lineCoordsDest.coord1.Y + l * lineCoordsDest.coord2.Y) / (1 + l),
            };
            absoluteLayout.Add(line);
        }

        lineCoordsStart.coord1 = points[0];
        lineCoordsDest.coord1 = points[1];
        lineCoordsDest.coord2 = points[2];
        lineCoordsStart.coord2 = points[3];
        lineCoordsStart.coord1 = points[4];

        lineCoordsStart.coord1 = new Point((lineCoordsStart.coord1.X + lineCoordsStart.coord2.X) / 2,
           (lineCoordsStart.coord1.Y + lineCoordsStart.coord2.Y) / 2);

        lineCoordsDest.coord1 = new Point((lineCoordsDest.coord1.X + lineCoordsDest.coord2.X) / 2,
           (lineCoordsDest.coord1.Y + lineCoordsDest.coord2.Y) / 2);

        b = rightAmount;
        for (int i = a; i <= b; i++)
        {
            double l = (double)i / (double)b;
            double X = (lineCoordsStart.coord1.X + l * lineCoordsStart.coord2.X) / (1 + l);
            double Y = (lineCoordsStart.coord1.Y + l * lineCoordsStart.coord2.Y) / (1 + l);
            Debug.WriteLine($"l={l} x={X} y={Y}");
            Line line = new Line()
            {
                Fill = lineColor,
                Stroke = lineColor,
                StrokeThickness = 3,
                IsEnabled = false,
                ZIndex = 2,
                StrokeDashArray = { 2, 2 },
                StrokeDashOffset = 10,
                X1 = (lineCoordsStart.coord1.X + l * lineCoordsStart.coord2.X) / (1 + l),
                Y1 = (lineCoordsStart.coord1.Y + l * lineCoordsStart.coord2.Y) / (1 + l),
                X2 = (lineCoordsDest.coord1.X + l * lineCoordsDest.coord2.X) / (1 + l),
                Y2 = (lineCoordsDest.coord1.Y + l * lineCoordsDest.coord2.Y) / (1 + l),
            };
            absoluteLayout.Add(line);
        }
    }

    ///<summary>
    ///This event is only used to toggle flags around a crossroad. Doesn't trigger drawing.
    ///</summary>
    private void Crossroads_Tapped(object sender, TappedEventArgs e)
    {
        Rectangle rectangle = (Rectangle)sender;
        Node node = GetNodeFromRectangle(rectangle, nodes);
        if (rectangle.Equals(currentRectangle))
        {
            Debug.WriteLine("same node");
        }
        else if (prevNode.Equals(rectangle))
        {
            SetImageButtonsVisibility(node.imageButtons, false);
        }
        else
        {
            SetImageButtonsVisibility(prevNode.imageButtons, false);
            SetImageButtonsVisibility(node.imageButtons, true);
            prevNode = node;
        }
        currentRectangle = rectangle;
        //SetImageButtonsType(rectangle, ButtonType.DestinationBtn, nodes);

    }

    ///<summary>
    ///Event for plus and destination image buttons.
    ///</summary>
    private void ImgButton_Clicked(object sender, EventArgs e)
    {
        currentImageButton.Source = ButtonType.PlusBtn;
        ImageButton imageButton = (ImageButton)sender;
        if (currentImageButton.Equals(imageButton)) 
            //if in AddNewNode mode clicked object is the same as image button that toggled this mode, then it cancel AddNewNodeMode
        {
            AddNewNodeFlag = false;
            currentImageButton = new ImageButton();
            SetImageButtonsType(ButtonType.PlusBtn, nodes);
            Node targetNode = GetNodeFromImageButton(imageButton, nodes);
            foreach (Node node in nodes)
            {
                if (!node.Equals(targetNode))
                {
                    SetImageButtonsVisibility(node.imageButtons, false);
                }
            }
        }
        else if (AddNewNodeFlag == true && !IsFlagBelongsToSameNode(imageButton, currentImageButton, nodes))
            //if it receives another image button, it links a new road between two crossroads
        {
            Node targetNode = GetNodeFromImageButton(imageButton, nodes);
            Rect rect = absoluteLayout.GetLayoutBounds(targetNode.rectangle);
            LineCoords destCoords = GetLineCoordsForOrientation((Orientation)imageButton.Rotation, rect);
            Polygon road = DrawRoad(lineCoords, destCoords, (Orientation)imageButton.Rotation, (Orientation)currentImageButton.Rotation);     
            targetNode.roads.Add(road);
            absoluteLayout.Add(road);
            foreach (Node node in nodes)
            {
                SetImageButtonsVisibility(node.imageButtons, false);
            }
            SetImageButtonsType(ButtonType.PlusBtn, nodes);

            absoluteLayout.Remove(currentImageButton);
            absoluteLayout.Remove(imageButton);
            AddNewNodeFlag = false;
        }
        else
            //if sending image button belongs to the same crossroad, new image button become starting point
        {
            imageButton.Source = ButtonType.PlusGreenBtn;
            Node nodeTarget = GetNodeFromImageButton(imageButton, nodes);
            Orientation orientation = (Orientation)imageButton.Rotation;
            currentOrientation = orientation;
            Rect rectangleRect = absoluteLayout.GetLayoutBounds(nodeTarget.rectangle);
            lineCoords = GetLineCoordsForOrientation(orientation, rectangleRect);

            foreach (Node node in nodes)
            {
                if (!node.Equals(nodeTarget))
                {
                    SetImageButtonsVisibility(node.imageButtons, true);
                }
            }
            
            SetImageButtonsType(ButtonType.DestinationBtn, nodes, nodeTarget.rectangle);
            currentImageButton = imageButton;
            AddNewNodeFlag = true;
        }
    }

    ///<summary>
    ///Main absolute layout event
    ///</summary>
    private void AbsoluteLayout_Tapped(object sender, TappedEventArgs e)
    {
        Point? point = e.GetPosition((View)sender);
        if (point == null) { return; }

        Rectangle rectangle;
        Rect rect = new Rect(point.Value.X - RectWidth / 2,
            point.Value.Y - RectHeight / 2, RectWidth, RectHeight);
        
        if (nodes.Capacity == 0) //if there are no crossroads, then it just creates new crossroads without roads.
        {
            rectangle = GetRectangle(rect, Crossroads_Tapped);
            absoluteLayout.Add(rectangle);
            absoluteLayout.SetLayoutBounds(rectangle, rect);
            AddFlagsAroundRectangle(rectangle, GetReversedOrientation(currentOrientation), false);
        }
        if (nodes.Capacity > 0 || !AddNewNodeFlag) // resetting  image buttons if user clicks on empty area
        {
            Node targetNode = GetNodeFromImageButton(currentImageButton, nodes);
            foreach (Node node in nodes)
            {
                SetImageButtonsVisibility(node.imageButtons, false);
            }
            currentRectangle = new Rectangle();
        }
        if (AddNewNodeFlag) //if the program in AddNewNode mode, then it creates new crossroad and links a road to it.
        {
            rectangle = GetRectangle(rect, Crossroads_Tapped);
            absoluteLayout.Add(rectangle);
            absoluteLayout.SetLayoutBounds(rectangle, rect);
            Rect rect2 = absoluteLayout.GetLayoutBounds(rectangle);
            LineCoords destCoords = GetLineCoordsForOrientation(currentOrientation, rect, true);
            SetImageButtonsType(ButtonType.PlusBtn, nodes);
            AddFlagsAroundRectangle(rectangle, GetReversedOrientation(currentOrientation), false);
            Polygon road = DrawRoad(lineCoords, destCoords, currentOrientation, currentOrientation);
            absoluteLayout.Add(road);
            Node targetNode = GetNodeFromImageButton(currentImageButton, nodes);

            targetNode.roads.Add(road);
            foreach (Node node in nodes)
            {
                if (!node.Equals(targetNode))
                {
                    SetImageButtonsVisibility(node.imageButtons, false);
                }
            }  
            absoluteLayout.Remove(currentImageButton);         
            AddNewNodeFlag = false;
        }
    }

    private void ButtonClean_Clicked(object sender, EventArgs e)
    {
        ResetRoadElements();
        absoluteLayout.Clear();
    }

    private void ButtonLines_Clicked(object sender, EventArgs e)
    {

    }
    
}

