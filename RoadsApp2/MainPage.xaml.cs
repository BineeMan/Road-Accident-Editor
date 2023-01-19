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

        //LineCoords lineCoords = new LineCoords()
        //{
        //    coord1 = new Point(100, 150),
        //    coord2 = new Point(150, 150),
        //};

        //double c = Math.Sqrt( Math.Pow( (lineCoords.coord2.X - lineCoords.coord1.X) , 2 ) + Math.Pow( (lineCoords.coord2.Y - lineCoords.coord1.Y) , 2) );


        //Line line = new Line()
        //{
        //    Fill = Brush.Black,
        //    Stroke = Brush.Black,
        //    StrokeThickness = 3,
        //    IsEnabled = false,
        //    ZIndex = 2,
        //    X1 = lineCoords.coord1.X,
        //    Y1 = lineCoords.coord1.Y,
        //    X2 = lineCoords.coord2.X,
        //    Y2 = lineCoords.coord2.Y
        //};
        //absoluteLayout.Add(line);
        //line = new Line()
        //{
        //    Fill = Brush.Black,
        //    Stroke = Brush.Black,
        //    StrokeThickness = 3,
        //    IsEnabled = false,
        //    ZIndex = 2,
        //    R = 0,
        //    X1 = lineCoords.coord1.X,
        //    Y1 = lineCoords.coord1.Y,
        //    X2 = lineCoords.coord2.X,
        //    Y2 = lineCoords.coord2.Y
        //};
        //absoluteLayout.Add(line);

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

    private readonly int RectWidth = 75;

    private readonly int RectHeight = 75;

    private ImageButton currentImageButton = new ImageButton();

    private Orientation currentOrientation = Orientation.Undefined;

    private void SetImageButtonsType(string source, List<Node> nodes, Rectangle rectangleExcept = null)
    {
        if (rectangleExcept == null)
        {
            rectangleExcept = new Rectangle();
        }
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

    private bool IsFlagBelongsToSameNode(ImageButton imageButton1, ImageButton imageButton2, List<Node> nodes)
    {
        Node node = GetNodeFromImageButton(imageButton1, nodes);
        return node.imageButtons.Contains(imageButton2);
    }

    private void SetImageButtonsVisibility(List<ImageButton> imageButtons, bool isVisible) 
    {
        foreach (ImageButton imageButton in imageButtons)
        {
            imageButton.IsVisible = isVisible;
        }
    }

    private Rectangle GetRectangleFromImageButton(ImageButton imageButton, List<Node> nodes)
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

    private Node GetNodeFromImageButton (ImageButton imageButton, List<Node> nodes)
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

    ///<summary>
    ///This event is only used to toggle flags around a crossroad. Doesn't trigger drawing.
    ///</summary>
    private void TapGestureRecognizerRectangle_Tapped(object sender, TappedEventArgs e)
    {
        Rectangle rectangle = (Rectangle)sender;
        Node node = GetNodeFromRectangle(rectangle, nodes);
        if (prevNode.Equals(rectangle))
        {
            SetImageButtonsVisibility(node.imageButtons, false);
        }
        else
        {
            SetImageButtonsVisibility(prevNode.imageButtons, false);
            SetImageButtonsVisibility(node.imageButtons, true);
            prevNode = node;
        }  
        //SetImageButtonsType(rectangle, ButtonType.DestinationBtn, nodes);

    }

    ///<summary>
    ///This events adds new flags around a crossroads with except orientation which will be avoided
    ///</summary>
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
                Source = ButtonType.PlusBtn,
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
        currentImageButton.Source = ButtonType.PlusBtn;

        ImageButton imageButton = (ImageButton)sender;

        if (currentImageButton.Equals(imageButton))
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
        {
            Node targetNode = GetNodeFromImageButton(imageButton, nodes);

            Rect rect = absoluteLayout.GetLayoutBounds(targetNode.rectangle);

            LineCoords destCoords = GetLineCoordsForOrientation((Orientation)imageButton.Rotation, rect);

            DrawRoad(lineCoords, destCoords, (Orientation)imageButton.Rotation, (Orientation)currentImageButton.Rotation);

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

    private bool IsOrientationsParallel(Orientation orientation1, Orientation orientation2)
    {
        return (double)orientation1 + (double)GetReversedOrientation(orientation1) == (double)orientation2 + (double)GetReversedOrientation(orientation2);
    }

    private void DrawRoad(LineCoords lineCoordsStart, LineCoords lineCoordsDest, Orientation orientationStart, Orientation orientationDest) 
    {
        if ( (double)orientationDest + (double)orientationStart == (double)Orientation.Right + (double)Orientation.Up || (double)orientationDest + (double)orientationStart == (double)Orientation.Left + (double)Orientation.Down)
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
            IsEnabled = false
        };
        absoluteLayout.Add(polygon);

        Brush lineColor = Brush.White;

        Line line = new Line()
        {
            Fill = lineColor,
            Stroke = lineColor,
            StrokeThickness = 3,
            IsEnabled = false,
            ZIndex = 2,
            X1 = (lineCoordsStart.coord1.X + lineCoordsStart.coord2.X) / 2,
            Y1 = (lineCoordsStart.coord1.Y + lineCoordsStart.coord2.Y) / 2,
            X2 = (lineCoordsDest.coord1.X + lineCoordsDest.coord2.X) / 2,
            Y2 = (lineCoordsDest.coord1.Y + lineCoordsDest.coord2.Y) / 2,
        };
        absoluteLayout.Add(line);

        line = new Line()
        {
            Fill = lineColor,
            Stroke = lineColor,
            StrokeThickness = 3,
            IsEnabled = false,
            ZIndex = 2,
            StrokeDashArray = { 2, 2 },
            StrokeDashOffset = 10,
            X1 = (lineCoordsStart.coord1.X + ((lineCoordsStart.coord1.X + lineCoordsStart.coord2.X) / 2)) / 2,
            Y1 = (lineCoordsStart.coord1.Y + (lineCoordsStart.coord1.Y + lineCoordsStart.coord2.Y) / 2) / 2,
            X2 = (lineCoordsDest.coord1.X + (lineCoordsDest.coord1.X + lineCoordsDest.coord2.X) / 2) / 2,
            Y2 = (lineCoordsDest.coord1.Y + (lineCoordsDest.coord1.Y + lineCoordsDest.coord2.Y) / 2) / 2,
        };
        absoluteLayout.Add(line);

        line = new Line()
        {
            Fill = lineColor,
            Stroke = lineColor,
            StrokeThickness = 3,
            IsEnabled = false,
            ZIndex = 2,
            StrokeDashArray = { 2, 2 },
            StrokeDashOffset = 10,
            X1 = (lineCoordsStart.coord2.X + ((lineCoordsStart.coord1.X + lineCoordsStart.coord2.X) / 2)) / 2,
            Y1 = (lineCoordsStart.coord2.Y + (lineCoordsStart.coord1.Y + lineCoordsStart.coord2.Y) / 2) / 2,
            X2 = (lineCoordsDest.coord2.X + (lineCoordsDest.coord1.X + lineCoordsDest.coord2.X) / 2) / 2,
            Y2 = (lineCoordsDest.coord2.Y + (lineCoordsDest.coord1.Y + lineCoordsDest.coord2.Y) / 2) / 2,
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

            SetImageButtonsType(ButtonType.PlusBtn, nodes);
            AddFlagsAroundRectangle(rectangle, GetReversedOrientation(currentOrientation), false);
            DrawRoad(lineCoords, destCoords, currentOrientation, currentOrientation);
            Node targetNode = GetNodeFromImageButton(currentImageButton, nodes);

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
        AddNewNodeFlag = false;
        currentImageButton = new ImageButton();
        currentOrientation = Orientation.Undefined;
        prevNode = new Node() { imageButtons = new List<ImageButton>() };
        lineCoords = new LineCoords();
        nodes = new List<Node>();


        Debug.Write(nodes.Capacity);
        absoluteLayout.Clear();
        Button button = new Button()
        {
            Text = "Очистить",
        };
        button.Clicked += ButtonClean_Clicked;
        absoluteLayout.Add(button);
        absoluteLayout.SetLayoutBounds(button, new Rect(10, 10, button.Width, button.Height));
    }

}

