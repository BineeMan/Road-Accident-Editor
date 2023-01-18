using System;
using System.Diagnostics;
using System.Windows.Input;
using Microsoft.Maui.Controls.Shapes;
using static RoadsApp2.Utils.Enums;
using static RoadsApp2.Utils.Structs;
using static RoadsApp2.Utils.UserInterfaceUtils;
using static RoadsApp2.Utils.Utils;

namespace RoadsApp2;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private Node prevNode = new Node() { imageButtons = new List<ImageButton>() };

    private LineCoords lineCoords = new LineCoords();

    private List<Node> nodes = new List<Node>();

    private Point[] pointOrientations = new Point[]
    {
        new Point { X = 0.5, Y = 0 },
        new Point { X = 1, Y = 0.5 },
        new Point { X = 0.5, Y = 1},
        new Point { X = 0, Y = 0.5}
    };

    private bool AddNewNodeFlag = false;

    private readonly int RectWidth = 100;

    private readonly int RectHeight = 100;

    private ImageButton currentImageButton = new ImageButton();

    private Orientation currentOrientation = Orientation.Undefined;

    private void TapGestureRecognizerRectangle_Tapped(object sender, TappedEventArgs e)
    {
        ToggleImageButtons(prevNode.imageButtons);
        Rectangle rectangle = (Rectangle)sender;
        Node node = GetNodeFromRectangle(rectangle, nodes);
        ToggleImageButtons(node.imageButtons);
        prevNode = node;
    }

    private void AddFlagsAroundRectangle(Rectangle rectangle, Orientation orientationExcept = Orientation.Undefined, bool isVisible = true)
    {
        Node node = new Node()
        {
            rectangle = rectangle,
            imageButtons = new List<ImageButton>(),
            isActive = true
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
                Source = "plus.png",
                WidthRequest = 55,
                HeightRequest = 55,
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
                new Rect( x, y, imageButton.Width, imageButton.Height));
            imageButton.Clicked += imgButton_Clicked;
            node.imageButtons.Add(imageButton);
            rotation += 90;
        }
        nodes.Add(node);
        if (isVisible) 
        {
            prevNode = node;
        }
    }
    
    private void imgButton_Clicked(object sender, EventArgs e)
    {
        currentImageButton.Source = "plus.png";
        if (currentImageButton.Equals((ImageButton)sender))
        {
            AddNewNodeFlag = false;
            currentImageButton = new ImageButton();
            return;
        }
        ImageButton imageButton = (ImageButton)sender;
        imageButton.Source = "plusgreen.png";
        Rect rectButton = absoluteLayout.GetLayoutBounds(imageButton);
        Orientation orientation = Orientation.Up;
        Node nodeTarget = new Node();
        foreach (Node node in nodes)
        {
            if (node.imageButtons.IndexOf(imageButton) != -1)
            {
                orientation = (Orientation)imageButton.Rotation;
                currentOrientation = orientation;
                nodeTarget = node;
                break;
            }
        }
        Rect rectangleRect = absoluteLayout.GetLayoutBounds(nodeTarget.rectangle);
        lineCoords = GetLineCoordsForOrientation(orientation, rectangleRect);
        
        currentImageButton = imageButton;
        AddNewNodeFlag = true;
    }

    private void DrawRoad(LineCoords lineCoordsStart, LineCoords lineCoordsDest, Orientation orientation) 
    {
        PointCollection points = new[]
        {
            new Point(lineCoordsStart.coord1.X, lineCoordsStart.coord1.Y),
            new Point(lineCoordsDest.coord1.X, lineCoordsDest.coord1.Y),
            new Point(lineCoordsDest.coord2.X, lineCoordsDest.coord2.Y),
            new Point(lineCoordsStart.coord2.X, lineCoordsStart.coord2.Y)
        };

        Polygon polygon = new Polygon()
        {
            Points = points,
            Fill = Color.FromRgb(137, 137, 137),
            Stroke = Brush.Gray,
            StrokeThickness = 0,
            IsEnabled = false
        };
        absoluteLayout.Add(polygon);

        double offsetStartX = 0;
        double offsetStartY = 0;

        double offsetEndX = 0;
        double offsetEndY = 0;

        if (orientation == Orientation.Up || orientation == Orientation.Down)
        {
            offsetStartX = (lineCoordsStart.coord2.X - lineCoordsStart.coord1.X) / 2;
            offsetEndX = (lineCoordsDest.coord2.X - lineCoordsDest.coord1.X) / 2;
        }
        else if (orientation == Orientation.Left || orientation == Orientation.Right)
        {
            offsetStartY = (lineCoordsStart.coord2.Y - lineCoordsStart.coord1.Y) / 2;
            offsetEndY = (lineCoordsDest.coord2.Y - lineCoordsDest.coord1.Y) / 2;
        }

        Line line = new Line()
        {
            Fill = Brush.White,
            Stroke = Brush.White,
            StrokeThickness = 3,
            IsEnabled = false,
            ZIndex = 2,
            X1 = lineCoordsStart.coord1.X + offsetStartX,
            Y1 = lineCoordsStart.coord1.Y + offsetStartY,
            X2 = lineCoordsDest.coord1.X + offsetEndX,
            Y2 = lineCoordsDest.coord1.Y + offsetEndY
        };
        absoluteLayout.Add(line);

        line = new Line()
        {
            Fill = Brush.White,
            Stroke = Brush.White,
            StrokeThickness = 3,
            IsEnabled = false,
            ZIndex = 2,
            StrokeDashArray = { 2, 2 },
            StrokeDashOffset = 10,
            X1 = lineCoordsStart.coord1.X + offsetStartX / 2,
            Y1 = lineCoordsStart.coord1.Y + offsetStartY / 2,
            X2 = lineCoordsDest.coord1.X + offsetEndX / 2,
            Y2 = lineCoordsDest.coord1.Y + offsetEndY / 2
        };
        absoluteLayout.Add(line);

        line = new Line()
        {
            Fill = Brush.White,
            Stroke = Brush.White,
            StrokeThickness = 3,
            IsEnabled = false,
            ZIndex = 2,
            StrokeDashArray = { 2, 2 },
            StrokeDashOffset = 10,
            X1 = lineCoordsStart.coord1.X + offsetStartX + offsetStartX / 2,
            Y1 = lineCoordsStart.coord1.Y + offsetStartY + offsetStartY / 2,
            X2 = lineCoordsDest.coord1.X + offsetEndX + offsetEndX / 2,
            Y2 = lineCoordsDest.coord1.Y + offsetEndY + offsetEndY / 2
        };
        absoluteLayout.Add(line);
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        Point? point = e.GetPosition((View)sender);
        if (point == null) { return; }

        Rectangle rectangle;
        Rect rect = new Rect(point.Value.X - RectWidth / 2,
            point.Value.Y - RectHeight / 2, RectWidth, RectHeight);

        if (nodes.Capacity == 0)
        {
            rectangle = GetRectangle(rect, TapGestureRecognizerRectangle_Tapped);
            absoluteLayout.Add(rectangle);
            absoluteLayout.SetLayoutBounds(rectangle, rect);
            AddFlagsAroundRectangle(rectangle, GetReversedOrientation(currentOrientation), false);
        }
        if (AddNewNodeFlag) 
        {
            rectangle = GetRectangle(rect, TapGestureRecognizerRectangle_Tapped);
            absoluteLayout.Add(rectangle);
            absoluteLayout.SetLayoutBounds(rectangle, rect);

            Rect rect2 = absoluteLayout.GetLayoutBounds(rectangle);

            LineCoords destCoords = GetLineCoordsForOrientation(currentOrientation, rect, true);

            AddFlagsAroundRectangle(rectangle, GetReversedOrientation(currentOrientation), false);

            DrawRoad(lineCoords, destCoords, currentOrientation);
            absoluteLayout.Remove(currentImageButton);
            
            AddNewNodeFlag = false;

        }
    }
}

