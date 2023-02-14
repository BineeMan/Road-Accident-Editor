using System.Diagnostics;
using System.Numerics;
using Microsoft.Maui.Controls.Shapes;
using RoadsApp2.DataClasses;
using static RoadsApp2.Utils.Enums;
using static RoadsApp2.Utils.Structs;
using static RoadsApp2.Utils.UserInterfaceUtils;
using static RoadsApp2.Utils.Utils;
using Node = RoadsApp2.Utils.Structs.Node;
using Vector = RoadsApp2.Utils.Structs.Vector;

namespace RoadsApp2;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        ResetRoadElements();
    }

    private Node prevNode { get; set; }

    private Vector lineCoords { get; set; }

    private List<Node> nodes;

    private readonly Point[] pointOrientations = new Point[]
    {
        new Point { X = 0.5, Y = 0 },
        new Point { X = 1, Y = 0.5 },
        new Point { X = 0.5, Y = 1},
        new Point { X = 0, Y = 0.5}
    };

    private bool AddNewNodeFlag { get; set; }

    private bool AddNewObjectFlag { get; set; }

    private bool AddNewTrajectoryFlag { get; set; }

    private readonly int RectWidth = 200;

    private readonly int RectHeight = 200;

    private List<Link> links;

    private ImageButton currentImageButton { get; set; }

    private Orientation currentOrientation { get; set; }

    private Rectangle currentRectangle { get; set; }

    private Image currentImage { get; set; }

    private ImageSource imageSource { get; set; }

    private Frame currentFrame { get; set; }

    private Point? currentPoint { get; set; }



    private void ResetRoadElements()
    {
        prevNode = new Node() { imageButtons = new List<ImageButton>() };
        lineCoords = new Vector();
        nodes = new List<Node>();
        AddNewNodeFlag = false;
        links = new List<Link>();
        currentImageButton = new ImageButton();
        currentOrientation = Orientation.Undefined;
        currentRectangle = new Rectangle();
        AddNewObjectFlag = false;
        currentImage = new Image() { Source = "plus.png" };
        currentFrame = new Frame();
        imageSource = "plus.png";
    }

    ///<summary>
    ///This function adds new flags around a crossroads with except orientation which will be avoided
    ///</summary>
    private void AddFlagsAroundRectangle(Rectangle rectangle, 
        Orientation orientationExcept = Orientation.Undefined, bool isVisible = true)
    {
        Node node = new Node()
        {
            rectangle = rectangle,
            imageButtons = new List<ImageButton>(),
            isActive = true,
            roads = new List<Link>()
        };
        Orientation rotation = Orientation.Up;
        Rect rect = absoluteLayout.GetLayoutBounds(rectangle);

        if (rect.Width == rect.Height)
        {
            foreach (Point point in pointOrientations)
            {
                if (rotation == orientationExcept)
                {
                    rotation += 90;
                    continue;
                }

                ImageButton imageButton = new ImageButton
                {
                    Source = ButtonType.RoadPlusBlackButton,
                    WidthRequest = 45,
                    HeightRequest = 45,
                    Rotation = (double)rotation,
                    ZIndex = 99,
                    AnchorY = 1,
                    IsVisible = isVisible,
                    Background = Brush.Transparent
                };
                absoluteLayout.Add(imageButton);

                rect = absoluteLayout.GetLayoutBounds(rectangle);

                double x = rect.X - imageButton.WidthRequest / 2 + rect.Width * point.X;
                double y = rect.Y - imageButton.HeightRequest + rect.Height * point.Y;

                absoluteLayout.SetLayoutBounds(imageButton,
                    new Rect(x, y, imageButton.Width, imageButton.Height));
                imageButton.Clicked += ImgButton_Clicked;
                node.imageButtons.Add(imageButton);
                rotation += 90;
            }
        }
        else
        {
            rect = absoluteLayout.GetLayoutBounds(rectangle);
            ImageButton imageButton = new ImageButton
            {
                Source = ButtonType.RoadPlusBlackButton,
                WidthRequest = 45,
                HeightRequest = 45,
                Rotation = (double)GetReversedOrientation(orientationExcept),
                ZIndex = 99,
                AnchorY = 1,
                IsVisible = isVisible,
                Background = Brush.Transparent
            };
            double x = 0, y = 0;

            if (rect.Width < rect.Height)
            {
                x = rect.X - imageButton.WidthRequest / 2 + rect.Width * 1;
                y = rect.Y - imageButton.HeightRequest + rect.Height * 0.5;
            }
            else
            {
                x = rect.X - imageButton.WidthRequest / 2 + rect.Width * 0.5;
                y = rect.Y - imageButton.HeightRequest + rect.Height * 0;
            }
            absoluteLayout.Add(imageButton);
            absoluteLayout.SetLayoutBounds(imageButton,
                new Rect(x, y, imageButton.Width, imageButton.Height));
            imageButton.Clicked += ImgButton_Clicked;
            node.imageButtons.Add(imageButton);
        }
        nodes.Add(node);
        if (isVisible)
        {
            prevNode = node;
        }
    }

    private void AddFlagAroundRoad(ref Link link)
    {
        PointCollection points = link.road.Points;
        Vector vectorStart = new Vector();
        Vector vectorDestination = new Vector();
        vectorStart.point1 = points[0];
        vectorDestination.point1 = points[1];
        vectorDestination.point2 = points[2];
        vectorStart.point2 = points[3];
        vectorStart.point1 = points[4];

        link.LineSteppers = new List<LineStepper>();

        double step = -35;
        double xUpHalf = ((vectorStart.point1.X + step) + (vectorDestination.point1.X + step)) / 2;
        double yUpHalf = ((vectorStart.point1.Y + step) + (vectorDestination.point1.Y + step)) / 2;

        Vector xO = new Vector
        {
            point1 = new Point(0, absoluteLayout.Height),
            point2 = new Point(absoluteLayout.Width, absoluteLayout.Height),
        };

        Vector2 a = new Vector2((float)(vectorDestination.point1.X - vectorStart.point1.X),
            (float)(vectorDestination.point1.Y - vectorStart.point1.Y));

        Vector2 b = new Vector2((float)(absoluteLayout.Width), 0);
        double scale = 1;
#if WINDOWS
        scale = 1;
#endif

#if ANDROID
        scale = 0.5;
#endif
        Color color = Color.FromRgb(165, 163, 165);
        Stepper stepper = new Stepper()
        {
            Maximum = 4,
            Minimum = 1,
            HorizontalOptions = LayoutOptions.Center,
            Scale = scale,
            ZIndex = 5,
            AnchorY = 0.5,
            AnchorX = 0.5,
            IsVisible = false,
            Value = 1,
            Background = Color.FromRgb(81, 43, 212),
            BackgroundColor = Color.FromRgb(81, 43, 212)
        };
        stepper.ValueChanged += OnStepperValueChanged;
        absoluteLayout.Add(stepper);
        absoluteLayout.SetLayoutBounds(stepper,
                new Rect(xUpHalf, yUpHalf, stepper.Width, stepper.Height));

        link.LineSteppers.Add(new LineStepper()
        {
            Stepper = stepper,
            Vector = new Vector() { point1 = vectorStart.point1, point2 = vectorDestination.point1 }
        });

        stepper = new Stepper()
        {
            Maximum = 4,
            Minimum = 1,
            HorizontalOptions = LayoutOptions.Center,
            Scale = scale,
            ZIndex = 5,
            IsVisible = false,
            Value = 1,
            Background = Color.FromRgb(81, 43, 212),
            BackgroundColor = Color.FromRgb(81, 43, 212)
        };
        stepper.ValueChanged += OnStepperValueChanged;
        step = 0;
        xUpHalf = ((vectorStart.point2.X + step) + (vectorDestination.point2.X + step)) / 2;
        yUpHalf = ((vectorStart.point2.Y + step) + (vectorDestination.point2.Y + step)) / 2;
        absoluteLayout.Add(stepper);
        absoluteLayout.SetLayoutBounds(stepper,
                new Rect(xUpHalf, yUpHalf, stepper.Width, stepper.Height));
        link.LineSteppers.Add(new LineStepper()
        {
            Stepper = stepper,
            Vector = new Vector() { point1 = vectorStart.point2, point2 = vectorDestination.point2 }
        });
    }

    private void DrawLines2(ref Link link, Vector vectorStepper, int newAmount)
    {
        PointCollection points = link.road.Points;
        Vector vectorStart = new Vector();
        Vector vectorDestination = new Vector();
        vectorStart.point1 = points[0];
        vectorDestination.point1 = points[1];
        vectorDestination.point2 = points[2];
        vectorStart.point2 = points[3];
        vectorStart.point1 = points[4];

        Brush lineColor = Brush.White;

        if (!link.IsTwoLaned)
        {
            if (link.MiddleLines != null) 
            {
                foreach (Line line1 in link.MiddleLines)
                {
                    absoluteLayout.Remove(line1);
                }
                link.MiddleLines.Clear();
            }
            else
            {
                link.MiddleLines = new List<Line>();
            }
            
            Line line = new Line()
            {
                Fill = lineColor,
                Stroke = lineColor,
                StrokeThickness = 3,
                IsEnabled = false,
                ZIndex = 2,
                X1 = (vectorStart.point1.X + vectorStart.point2.X) / 2,
                Y1 = (vectorStart.point1.Y + vectorStart.point2.Y) / 2,
                X2 = (vectorDestination.point1.X + vectorDestination.point2.X) / 2,
                Y2 = (vectorDestination.point1.Y + vectorDestination.point2.Y) / 2,
            };
            link.MiddleLines.Add(line);
            absoluteLayout.Add(line);
        }

        double offsetX = 0;
        double offsetY = 0;
        double stepX = 0;
        double stepY = 0;
        if (vectorStepper.Equals(vectorStart))
        {
            vectorStart.point2 = new Point((vectorStart.point1.X + vectorStart.point2.X) / 2,
                (vectorStart.point1.Y + vectorStart.point2.Y) / 2);

            vectorDestination.point2 = new Point((vectorDestination.point1.X + vectorDestination.point2.X) / 2,
               (vectorDestination.point1.Y + vectorDestination.point2.Y) / 2);

            stepX = (vectorStart.point2.X - vectorStart.point1.X) / newAmount;
            stepY = (vectorStart.point2.Y - vectorStart.point1.Y) / newAmount;

            Debug.WriteLine(link.LinesSide1.Count());
            foreach (Line line1 in link.LinesSide1)
            {
                Debug.WriteLine(absoluteLayout.Remove(line1));
            }

            link.LinesSide1.Clear();

            for (int i = 1; i < newAmount; i++)
            {
                offsetY += stepY;
                offsetX += stepX;
                Line line = new Line()
                {
                    Fill = lineColor,
                    Stroke = lineColor,
                    StrokeThickness = 3,
                    IsEnabled = false,
                    ZIndex = 2,
                    StrokeDashArray = { 2, 2 },
                    StrokeDashOffset = 10,
                    X1 = vectorStart.point1.X + offsetX,
                    Y1 = vectorStart.point1.Y + offsetY,
                    X2 = vectorDestination.point1.X + offsetX,
                    Y2 = vectorDestination.point1.Y + offsetY,
                };
                link.LinesSide1.Add(line);
                absoluteLayout.Add(line);
            }
        }
        else if (vectorStepper.Equals(vectorDestination))
        {
            vectorStart.point1 = new Point((vectorStart.point1.X + vectorStart.point2.X) / 2,
            (vectorStart.point1.Y + vectorStart.point2.Y) / 2);

            vectorDestination.point1 = new Point((vectorDestination.point1.X + vectorDestination.point2.X) / 2,
                (vectorDestination.point1.Y + vectorDestination.point2.Y) / 2);

            stepX = (vectorStart.point2.X - vectorStart.point1.X) / newAmount;
            stepY = (vectorStart.point2.Y - vectorStart.point1.Y) / newAmount;
            if (link.LinesSide2 != null)
            {
                foreach (Line line1 in link.LinesSide2)
                {
                    absoluteLayout.Remove(line1);
                }
            }
            else
            {
                link.LinesSide2 = new List<Line>();
            }
            link.LinesSide2.Clear();

            for (int i = 1; i < newAmount; i++)
            {
                offsetY += stepY;
                offsetX += stepX;
                Line line = new Line()
                {
                    Fill = lineColor,
                    Stroke = lineColor,
                    StrokeThickness = 3,
                    IsEnabled = false,
                    ZIndex = 2,
                    StrokeDashArray = { 2, 2 },
                    StrokeDashOffset = 10,
                    X1 = vectorStart.point1.X + offsetX,
                    Y1 = vectorStart.point1.Y + offsetY,
                    X2 = vectorDestination.point1.X + offsetX,
                    Y2 = vectorDestination.point1.Y + offsetY,
                };
                link.LinesSide2.Add(line);
                absoluteLayout.Add(line);
            }
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
            ToggleSteppersVisibility(node.roads , true);
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
        currentImageButton.Source = ButtonType.RoadPlusBlackButton;
        ImageButton imageButton = (ImageButton)sender;
        if (currentImageButton.Equals(imageButton)) 
            //if in AddNewNode mode clicked object is the same as image button that toggled this mode, then it cancel AddNewNodeMode
        {
            AddNewNodeFlag = false;
            currentImageButton = new ImageButton();
            SetImageButtonsType(ButtonType.RoadPlusBlackButton, nodes);
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
            Vector destCoords = GetLineCoordsForOrientation((Orientation)imageButton.Rotation, rect);
            Polygon road = DrawRoad(lineCoords, destCoords, (Orientation)imageButton.Rotation, (Orientation)currentImageButton.Rotation);     
            Link link = new Link() { road = road }; 
            targetNode.roads.Add(link);
            absoluteLayout.Add(road);

            foreach (Node node in nodes)
            {
                SetImageButtonsVisibility(node.imageButtons, false);
            }
            SetImageButtonsType(ButtonType.RoadPlusBlackButton, nodes);

            absoluteLayout.Remove(currentImageButton);
            absoluteLayout.Remove(imageButton);
            AddNewNodeFlag = false;
        }
        else
            //if sending image button belongs to the same crossroad, new image button become starting point
        {
            imageButton.Source = ButtonType.RoadPlusGreenButton;
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
            
            SetImageButtonsType(ButtonType.DestinationButton, nodes, nodeTarget.rectangle);
            currentImageButton = imageButton;
            AddNewNodeFlag = true;
        }
    }

    ///<summary>
    ///Main absolute layout event
    ///</summary>
    private void AbsoluteLayout_Tapped(object sender, TappedEventArgs e)
    {
        Debug.WriteLine("AbsoluteLayout_Tapped");
        Point? tappedPoint = e.GetPosition((View)sender);
        if (tappedPoint == null) { return; }

        Rectangle rectangle;

        Rect rect = new Rect();
        rect = new Rect(tappedPoint.Value.X - RectWidth / 2,
                tappedPoint.Value.Y - RectHeight / 2, RectWidth, RectHeight);

        if (switchIsCrossroad.IsToggled)
        {
            if (currentOrientation == Orientation.Left || currentOrientation == Orientation.Right)
            {
                rect = new Rect(tappedPoint.Value.X - RectWidth / 10,
                    tappedPoint.Value.Y - RectHeight / 2, RectWidth / 10 , RectHeight);
            }
            else if (currentOrientation == Orientation.Up || currentOrientation == Orientation.Down)
            {
                rect = new Rect(tappedPoint.Value.X - RectWidth / 2,
                    tappedPoint.Value.Y - RectHeight / 10, RectWidth, RectHeight / 10);
            }
        }
        if (AddNewTrajectoryFlag)
        {
            if (AddNewObjectFlag)
            {
                currentFrame.Background = Brush.Transparent;
                AddNewObjectFlag = false;
            }
            AddNewNodeFlag = false;

            if (currentPoint == null)
            {
                currentPoint = tappedPoint;
                Debug.WriteLine("urrentPoint == null");
            }
            else
            {
                Line line = DrawTrajectory((Point)currentPoint, (Point)tappedPoint);
                absoluteLayout.Add(line);
                currentPoint = tappedPoint;
                Debug.WriteLine("urrentPoint != null");
            }
        }
        else if(AddNewObjectFlag)
        {
            double width = 100, height = 100;
            Rect rectRoadObject = new Rect(tappedPoint.Value.X - width / 2,
               tappedPoint.Value.Y - height / 2, width, height);
           //currentImage = new Image();
            Image image = new Image() { Source = imageSource, ZIndex = 10 }; 
            absoluteLayout.Add(image);
            absoluteLayout.SetLayoutBounds(image, rectRoadObject);
            imageSource = "plus.png";
            Debug.WriteLine("AddNewObjectFlag");
            currentFrame.Background = Brush.Transparent;
            AddNewObjectFlag = false;
        }
        else if (nodes.Capacity == 0) //if there are no crossroads, then it just creates new crossroads without roads.
        {
            rectangle = GetRectangle(rect, Crossroads_Tapped);
            absoluteLayout.Add(rectangle);
            absoluteLayout.SetLayoutBounds(rectangle, rect);
            AddFlagsAroundRectangle(rectangle, GetReversedOrientation(currentOrientation), false);
        }
        else if (nodes.Capacity > 0 || !AddNewNodeFlag) // resetting  image buttons if user clicks on empty area
        {
            Node targetNode = GetNodeFromImageButton(currentImageButton, nodes);
            foreach (Node node in nodes)
            {
                SetImageButtonsVisibility(node.imageButtons, false);
            }

            ToggleSteppersVisibility(links, false);
            currentRectangle = new Rectangle();
        }

        if (AddNewNodeFlag) //if the program in AddNewNode mode, then it creates new crossroad and links a road to it.
        {
            rectangle = GetRectangle(rect, Crossroads_Tapped);
            absoluteLayout.Add(rectangle);
            absoluteLayout.SetLayoutBounds(rectangle, rect);
            Rect rect2 = absoluteLayout.GetLayoutBounds(rectangle);
            Vector destCoords = GetLineCoordsForOrientation(currentOrientation, rect, true);
            SetImageButtonsType(ButtonType.RoadPlusBlackButton, nodes);
            AddFlagsAroundRectangle(rectangle, GetReversedOrientation(currentOrientation), false);
            Polygon road = DrawRoad(lineCoords, destCoords, currentOrientation, currentOrientation);
            absoluteLayout.Add(road);
            Node targetNode = GetNodeFromImageButton(currentImageButton, nodes);

            Node callerNode = GetNodeFromRectangle(rectangle, nodes);

            Link link = new Link()
            { 
                road = road,
                LinesSide1 = new List<Line>(),
                LinesSide2 = new List<Line>(),
                MiddleLines = new List<Line>(),
            };

            AddFlagAroundRoad(ref link);
       
            links.Add(link);
            targetNode.roads.Add(link);
            callerNode.roads.Add(link);
            //DrawLines(ref link, 4, 2);

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

    void OnStepperValueChanged(object sender, ValueChangedEventArgs e)
    {
        double value = e.NewValue;
        Stepper stepper = (Stepper)sender;
        LineStepper lineStepper = GetLineStepperFromLinks(stepper, links);
        
        Link link = GetLinkFromLineStepper(lineStepper, links);

        DrawLines2(ref link, lineStepper.Vector, (int)value);

    }

    private void CarImage_Tapped(object sender, EventArgs e)
    {
        Image tappedImage = (Image)sender;
        Frame frame = (Frame)tappedImage.Parent;
        switchIsTrajectoryMode.IsToggled = false;

        if (imageSource.Equals(tappedImage.Source))
        {
            frame.Background = Brush.Transparent;
            AddNewObjectFlag = false;
            imageSource = "plus.png";
        }
        else
        {
            frame.Background = Color.FromHex("#606060");
            AddNewNodeFlag = false;
            AddNewObjectFlag = true;
            currentFrame = frame;
            imageSource = tappedImage.Source;
        }


    }

    private void switchIsTrajectoryMode_Toggled(object sender, ToggledEventArgs e)
    {
        if (AddNewTrajectoryFlag == true || e.Value == false)
        {
            currentPoint = null;
        }
        AddNewTrajectoryFlag = e.Value;
    }
}

