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
        //GridMain.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Star);
    }

    private Node PreviousNode { get; set; }

    private Vector LineCoords { get; set; }

    private List<Node> Nodes { get; set; }

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

    private int RectWidth { get; set; }

    private int RectHeight  { get; set; }

    private List<Link> Links { get; set; }

    private ImageButton CurrentImageButton { get; set; }

    private Orientation CurrentOrientation { get; set; }

    private Rectangle CurrentRectangle { get; set; }

    private Image CurrentImage { get; set; }

    private ImageSource ImageSource { get; set; }

    private Frame CurrentFrame { get; set; }

    private Point? CurrentPoint { get; set; }

    private Slider CurrentSizeSlider { get; set; }

    private Slider CurrentRotationSlider { get; set; }

    private Image CurrentRoadObject { get; set; }

    private View CurrentView { get; set; }

    private Rectangle CurrentCollision { get; set; }

    private void AdaptUIElementsWindows()
    {
        switchIsCrossroad.Margin = new Thickness(0, 0, -50, 0);
        switchIsTrajectoryMode.Margin = new Thickness(10, 0, -50, 0);

    }

    private void ResetRoadElements()
    {
        PreviousNode = new Node() { imageButtons = new List<ImageButton>() };
        LineCoords = new Vector();
        Nodes = new List<Node>();
        AddNewNodeFlag = false;
        Links = new List<Link>();
        CurrentImageButton = new ImageButton();
        CurrentOrientation = Orientation.Undefined;
        CurrentRectangle = new Rectangle();
        AddNewObjectFlag = false;
        CurrentImage = new Image() { Source = "plus.png" };
        CurrentFrame = new Frame();
        ImageSource = "plus.png";
        RectWidth = 200;
        RectHeight = 200;
#if WINDOWS
        RectWidth = 70;
        RectHeight = 70;
#endif
#if ANDROID
        RectWidth = 70;
        RectHeight = 70;
#endif
        crossButton.IsEnabled = false;
        CurrentCollision = new Rectangle();
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
        Nodes.Add(node);
        if (isVisible)
        {
            PreviousNode = node;
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
        Thickness thickness = new Thickness(-70, -10);
#if WINDOWS
        scale = 1;
        thickness = new Thickness(0,0);
#endif

#if ANDROID
        scale = 0.5;
        thickness = new Thickness(-70, -10);
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
            //Background = Color.FromRgb(81, 43, 212),
            //BackgroundColor = Color.FromRgb(81, 43, 212),
            Margin = thickness
        };
        stepper.ValueChanged += OnStepperValueChanged;
        absoluteLayout.Add(stepper);
        absoluteLayout.SetLayoutBounds(stepper,
                new Rect(xUpHalf, yUpHalf, stepper.Width, stepper.Height));

        double l = 6f / 3f;
        xUpHalf = ((vectorStart.point1.X + step) + (l * (vectorDestination.point1.X + step))) / (1 + l);
        yUpHalf = ((vectorStart.point1.Y + step) + (l * (vectorDestination.point1.Y + step))) / (1 + l);


        CheckBox checkBox = new CheckBox()
        {
            HorizontalOptions = LayoutOptions.Center,
            IsVisible = false,
            ZIndex = 5,
        };
        checkBox.CheckedChanged += CheckBoxTwoLaned_CheckedChanged;
        absoluteLayout.Add(checkBox);
        absoluteLayout.SetLayoutBounds(checkBox,
                new Rect(xUpHalf, yUpHalf, checkBox.Width, checkBox.Height));

        link.LineSteppers.Add(new LineStepper()
        {
            Stepper = stepper,
            Vector = new Vector() { point1 = vectorStart.point1, point2 = vectorDestination.point1 },
            CheckBoxTwoLaned = checkBox
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
            //Background = Color.FromRgb(81, 43, 212),
            //BackgroundColor = Color.FromRgb(81, 43, 212),
            Margin = thickness
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

        //Debug.WriteLine(Links.IndexOf(link));
        //Debug.WriteLine(Links[Links.IndexOf(link)].LineSteppers.Count);
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

        if (!link.SwitchIsTwoLaned.IsToggled)
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
            Debug.WriteLine("11");
            vectorStart.point2 = new Point((vectorStart.point1.X + vectorStart.point2.X) / 2,
                (vectorStart.point1.Y + vectorStart.point2.Y) / 2);

            vectorDestination.point2 = new Point((vectorDestination.point1.X + vectorDestination.point2.X) / 2,
               (vectorDestination.point1.Y + vectorDestination.point2.Y) / 2);

            stepX = (vectorStart.point2.X - vectorStart.point1.X) / newAmount;
            stepY = (vectorStart.point2.Y - vectorStart.point1.Y) / newAmount;

            foreach (Line line1 in link.LinesSide1)
            {
                absoluteLayout.Remove(line1);
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
            Debug.WriteLine("22");
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

    private void RedrawRoad()
    {
        foreach (Node node in Nodes)
        {
            absoluteLayout.Remove(node.rectangle);
            foreach (ImageButton imageButton in node.imageButtons)
            {
                absoluteLayout.Remove(imageButton);
            }
        }

        foreach (Link link in Links)
        {
            foreach (Line line1 in link.LinesSide1)
            {
                absoluteLayout.Remove(line1);
            }
            foreach (Line line1 in link.LinesSide2)
            {
                absoluteLayout.Remove(line1);
            }
            foreach (Line line1 in link.MiddleLines)
            {
                absoluteLayout.Remove(line1);
            }
            foreach (LineStepper lineStepper in link.LineSteppers)
            {
                absoluteLayout.Remove(lineStepper.Stepper);
            }
            absoluteLayout.Remove(link.road);
        }


    }

    ///<summary>
    ///This event is only used to toggle flags around a crossroad. Doesn't trigger drawing.
    ///</summary>
    private void Crossroads_Tapped(object sender, TappedEventArgs e)
    {
        if (AddNewTrajectoryFlag || AddNewObjectFlag)
        {
            return;
        }
        Rectangle rectangle = (Rectangle)sender;
        CurrentView = rectangle;
        crossButton.IsEnabled = true;
        Node node = GetNodeFromRectangle(rectangle, Nodes);
        if (CurrentRectangle.Equals(rectangle))
        {
            SetImageButtonsVisibility(node.imageButtons, false);
            PreviousNode = new Node() { imageButtons = new List<ImageButton>() };
        }
        else
        {
            SetImageButtonsVisibility(PreviousNode.imageButtons, false);
            SetImageButtonsVisibility(node.imageButtons, true);
            PreviousNode = node;
        }

        ToggleSteppersVisibility(Links, false);

        CurrentRectangle = rectangle;
    }

    void Debug2()
    {
        foreach (Link link in Links)
        {
            Debug.WriteLine(link.RectangleCollision.IsEnabled.ToString());
        }
    }

    ///<summary>
    ///Event for plus and destination image buttons.
    ///</summary>
    private void ImgButton_Clicked(object sender, EventArgs e)
    {
        CurrentImageButton.Source = ButtonType.RoadPlusBlackButton;
        ImageButton imageButton = (ImageButton)sender;
        if (CurrentImageButton.Equals(imageButton)) 
            //if in AddNewNode mode clicked object is the same as image button that toggled this mode, then it cancel AddNewNodeMode
        {
            AddNewNodeFlag = false;
            CurrentImageButton = new ImageButton();
            SetImageButtonsType(ButtonType.RoadPlusBlackButton, Nodes);
            Node targetNode = GetNodeFromImageButton(imageButton, Nodes);
            foreach (Node node in Nodes)
            {
                if (!node.Equals(targetNode))
                {
                    SetImageButtonsVisibility(node.imageButtons, false);
                }
            }
            foreach (Link link in Links)
            {
                link.RectangleCollision.IsEnabled = true;
            }
        }
        else if (AddNewNodeFlag == true && !IsFlagBelongsToSameNode(imageButton, CurrentImageButton, Nodes))
            //if it receives another image button, it links a new road between two crossroads
        {
            Node targetNode = GetNodeFromImageButton(imageButton, Nodes);
            Rect rect = absoluteLayout.GetLayoutBounds(targetNode.rectangle);
            Vector destCoords = GetLineCoordsForOrientation((Orientation)imageButton.Rotation, rect);
            Polygon road = DrawRoad(LineCoords, destCoords, (Orientation)imageButton.Rotation, (Orientation)CurrentImageButton.Rotation);

            Link link = new Link() { road = road };

            PointCollection points = new PointCollection();
            foreach (Point point in link.road.Points)
            {
                points.Add(point);
            }
            link.OriginalRoadPoints = points;

            targetNode.roads.Add(link);
            absoluteLayout.Add(road);

            foreach (Node node in Nodes)
            {
                SetImageButtonsVisibility(node.imageButtons, false);
            }
            SetImageButtonsType(ButtonType.RoadPlusBlackButton, Nodes);
            absoluteLayout.Remove(CurrentImageButton);
            absoluteLayout.Remove(imageButton);
            AddNewNodeFlag = false;
            foreach (Link link2 in Links)
            {
                link.RectangleCollision.IsEnabled = true;
            }
        }
        else
            //if sending image button belongs to the same crossroad, new image button become starting point
        {
            imageButton.Source = ButtonType.RoadPlusGreenButton;
            Node nodeTarget = GetNodeFromImageButton(imageButton, Nodes);
            Orientation orientation = (Orientation)imageButton.Rotation;
            CurrentOrientation = orientation;
            Rect rectangleRect = absoluteLayout.GetLayoutBounds(nodeTarget.rectangle);
            LineCoords = GetLineCoordsForOrientation(orientation, rectangleRect);

            foreach (Node node in Nodes)
            {
                if (!node.Equals(nodeTarget))
                {
                    SetImageButtonsVisibility(node.imageButtons, true);
                }
            }
            
            SetImageButtonsType(ButtonType.DestinationButton, Nodes, nodeTarget.rectangle);
            CurrentImageButton = imageButton;
            foreach (Link link in Links)
            {
                link.RectangleCollision.IsEnabled = false;
            }
            AddNewNodeFlag = true;
        }
    }

    ///<summary>
    ///Main absolute layout event
    ///</summary>
    private void AbsoluteLayout_Tapped(object sender, TappedEventArgs e)
    {
        Point? tappedPoint = e.GetPosition((View)sender);
        if (tappedPoint == null) { return; }
        absoluteLayout.Remove(CurrentRotationSlider);
        crossButton.IsEnabled = false;
        Rectangle rectangle;

        Rect rect = new Rect();
        rect = new Rect(tappedPoint.Value.X - RectWidth / 2,
                tappedPoint.Value.Y - RectHeight / 2, RectWidth, RectHeight);

        if (switchIsCrossroad.IsToggled)
        {
            if (CurrentOrientation == Orientation.Left || CurrentOrientation == Orientation.Right)
            {
                rect = new Rect(tappedPoint.Value.X - RectWidth / 10,
                    tappedPoint.Value.Y - RectHeight / 2, RectWidth / 10 , RectHeight);
            }
            else if (CurrentOrientation == Orientation.Up || CurrentOrientation == Orientation.Down)
            {
                rect = new Rect(tappedPoint.Value.X - RectWidth / 2,
                    tappedPoint.Value.Y - RectHeight / 10, RectWidth, RectHeight / 10);
            }
        }
        if (AddNewTrajectoryFlag)
        {
            if (AddNewObjectFlag)
            {
                CurrentFrame.Background = Brush.Transparent;
                AddNewObjectFlag = false;
            }
            AddNewNodeFlag = false;

            if (CurrentPoint == null)
            {
                CurrentPoint = tappedPoint;
            }
            else
            {
                Line line = DrawTrajectory((Point)CurrentPoint, (Point)tappedPoint);
                absoluteLayout.Add(line);
                CurrentPoint = tappedPoint;
            }
        }
        else if (Nodes.Count == 0) //if there are no crossroads, then it just creates new crossroads without roads.
        {
            rectangle = GetRectangle(rect, Crossroads_Tapped);
            absoluteLayout.Add(rectangle);
            absoluteLayout.SetLayoutBounds(rectangle, rect);
            AddFlagsAroundRectangle(rectangle, GetReversedOrientation(CurrentOrientation), false);
        }
        else if (Nodes.Count > 0 || !AddNewNodeFlag) // resetting  image buttons if user clicks on empty area
        {
            Node targetNode = GetNodeFromImageButton(CurrentImageButton, Nodes);
            foreach (Node node in Nodes)
            {
                SetImageButtonsVisibility(node.imageButtons, false);
            }
            absoluteLayout.Remove(CurrentRotationSlider);
            ToggleSteppersVisibility(Links, false);
            CurrentRectangle = new Rectangle();        
        }

        if (AddNewNodeFlag) //if the program in AddNewNode mode, then it creates new crossroad and links a road to it.
        {
            rectangle = GetRectangle(rect, Crossroads_Tapped);
            absoluteLayout.Add(rectangle);
            absoluteLayout.SetLayoutBounds(rectangle, rect);
            Rect rect2 = absoluteLayout.GetLayoutBounds(rectangle);
            Vector destCoords = GetLineCoordsForOrientation(CurrentOrientation, rect, true);
            SetImageButtonsType(ButtonType.RoadPlusBlackButton, Nodes);
            AddFlagsAroundRectangle(rectangle, GetReversedOrientation(CurrentOrientation), false);
            Polygon road = DrawRoad(LineCoords, destCoords, CurrentOrientation, CurrentOrientation);
            absoluteLayout.Add(road);

            Rect collisionRect = GetCollision(road);
            Rectangle collisionBox = new Rectangle() { Background = Brush.Transparent };

            TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += RoadCollision_Tapped;
            collisionBox.GestureRecognizers.Add(tapGestureRecognizer);
            absoluteLayout.Add(collisionBox);
            absoluteLayout.SetLayoutBounds(collisionBox, collisionRect);

            Node targetNode = GetNodeFromImageButton(CurrentImageButton, Nodes);
            Node callerNode = GetNodeFromRectangle(rectangle, Nodes);
            foreach (Link link2 in Links)
            {
                link2.RectangleCollision.IsEnabled = true;
            }
            Link link = new Link()
            { 
                road = road,
                LinesSide1 = new List<Line>(),
                LinesSide2 = new List<Line>(),
                MiddleLines = new List<Line>(),
                RectangleCollision = collisionBox
            };
            
            PointCollection points = new PointCollection();
            foreach (Point point in link.road.Points)
            {
                points.Add(point);
            }
            link.OriginalRoadPoints = points;

            AddFlagAroundRoad(ref link);

            Links.Add(link);

            targetNode.roads.Add(link);
            callerNode.roads.Add(link);

            foreach (Node node in Nodes)
            {
                if (!node.Equals(targetNode))
                {
                    SetImageButtonsVisibility(node.imageButtons, false);
                }
            }
            absoluteLayout.Remove(CurrentImageButton);         
            AddNewNodeFlag = false;
        }
    }

    private void RoadCollision_Tapped(object sender, TappedEventArgs e)
    {
        Debug.WriteLine("RoadCollision_Tapped");
        Rectangle collision = (Rectangle)sender;
        Link link = GetLinkByCollision(collision, Links);
        if (!CurrentCollision.Equals(collision))
        {
            Link linkPrevious = GetLinkByCollision(CurrentCollision, Links);
            ToggleSteppersVisibility2(linkPrevious.LineSteppers, false);
        }

        SetImageButtonsVisibility(PreviousNode.imageButtons, false);
        PreviousNode = new Node() { imageButtons = new List<ImageButton>() };
        CurrentRectangle = new Rectangle();
        ToggleSteppersVisibility2(link.LineSteppers, true);
        CurrentCollision = collision;
    }

    private void ButtonClean_Clicked(object sender, EventArgs e)
    {
        ResetRoadElements();
        absoluteLayout.Clear();
        //RedrawRoad();
    }

    private Line DrawLine(Vector vector)
    {
        Brush lineColor = Brush.White;
        Line line = new Line()
        {
            Fill = lineColor,
            Stroke = lineColor,
            StrokeThickness = 3,
            IsEnabled = false,
            ZIndex = 2,
            StrokeDashArray = { 2, 2 },
            StrokeDashOffset = 10,
            X1 = vector.point1.X,
            Y1 = vector.point1.Y,
            X2 = vector.point2.X,
            Y2 = vector.point2.Y,
        };
        return line;
    }

    private void ExtendRoad(ref Link link, ref LineStepper lineStepper)
    {
        PointCollection points = link.road.Points;
        Vector vectorStart = new Vector();
        Vector vectorDestination = new Vector();
        vectorStart.point1 = points[0];
        vectorDestination.point1 = points[1];
        vectorDestination.point2 = points[2];
        vectorStart.point2 = points[3];
        vectorStart.point1 = points[4];

        Vector vectorStartOriginal = new Vector();
        vectorStartOriginal.point1 = link.OriginalRoadPoints[0];
        vectorStartOriginal.point2 = link.OriginalRoadPoints[3];
        Debug.WriteLine(link.OriginalRoadPoints[0].Y);
        //vectorStartOriginal.point1 = points2[4];

        Vector vector = new Vector()
        {
            point1 = points[0],
            point2 = points[1]
        };

        int scale = 1;

        if (lineStepper.Vector.Equals(vector))
        {
            double stepX = (vectorStartOriginal.point2.X - vectorStartOriginal.point1.X) / scale;
            double stepY = (vectorStartOriginal.point2.Y - vectorStartOriginal.point1.Y) / scale;

            //Debug.WriteLine(vectorStartOriginal.point1.Y);

            Point newPointStart = new Point(vectorStart.point1.X - stepX,
                vectorStart.point1.Y - stepY);
            Point newPointDestination = new Point(vectorDestination.point1.X - stepX,
                vectorDestination.point1.Y - stepY);
            link.road.Points[0] = newPointStart;
            link.road.Points[4] = newPointStart;
            link.road.Points[1] = newPointDestination;
            Line line = DrawLine(lineStepper.Vector);
            link.LinesSide1.Add(line);
            absoluteLayout.Add(line);
            Vector vector1 = new Vector()
            {
                point1 = newPointStart,
                point2 = newPointDestination
            };
            lineStepper.Vector = vector1;
            link.LineSteppers[0] = lineStepper;
        }
        else
        {
            double stepX = (vectorStartOriginal.point2.X - vectorStartOriginal.point1.X) / scale;
            double stepY = (vectorStartOriginal.point2.Y - vectorStartOriginal.point1.Y) / scale;

            Point newPointStart = new Point(vectorStart.point2.X + stepX,
                vectorStart.point2.Y + stepY);

            Point newPointDestination = new Point(vectorDestination.point2.X + stepX,
                vectorDestination.point2.Y + stepY);

            link.road.Points[3] = newPointStart;
            link.road.Points[2] = newPointDestination;
            absoluteLayout.Remove(link.road);
            absoluteLayout.Add(link.road);

            Line line = DrawLine(lineStepper.Vector);
            link.LinesSide2.Add(line);
            absoluteLayout.Add(line);
            Vector vector1 = new Vector()
            {
                point1 = newPointStart,
                point2 = newPointDestination
            };
            lineStepper.Vector = vector1;
            link.LineSteppers[1] = lineStepper; 
        }

    }

    private void NarrowRoad(ref Link link, ref LineStepper lineStepper)
    {
        PointCollection points = link.road.Points;
        Vector vectorStart = new Vector();
        Vector vectorDestination = new Vector();
        vectorStart.point1 = points[0];
        vectorDestination.point1 = points[1];
        vectorDestination.point2 = points[2];
        vectorStart.point2 = points[3];
        vectorStart.point1 = points[4];

        Vector vectorStartOriginal = new Vector();
        vectorStartOriginal.point1 = link.OriginalRoadPoints[0];
        vectorStartOriginal.point2 = link.OriginalRoadPoints[3];
        Debug.WriteLine(link.OriginalRoadPoints[0].Y);
        //vectorStartOriginal.point1 = points2[4];

        Vector vector = new Vector()
        {
            point1 = points[0],
            point2 = points[1]
        };

        int scale = 1;

        if (lineStepper.Vector.Equals(vector))
        {
            double stepX = (vectorStartOriginal.point2.X - vectorStartOriginal.point1.X) / scale;
            double stepY = (vectorStartOriginal.point2.Y - vectorStartOriginal.point1.Y) / scale;

            //Debug.WriteLine(vectorStartOriginal.point1.Y);

            Point newPointStart = new Point(vectorStart.point1.X + stepX,
                vectorStart.point1.Y + stepY);
            Point newPointDestination = new Point(vectorDestination.point1.X + stepX,
                vectorDestination.point1.Y + stepY);
            link.road.Points[0] = newPointStart;
            link.road.Points[4] = newPointStart;
            link.road.Points[1] = newPointDestination;
            absoluteLayout.Remove(link.LinesSide1[link.LinesSide1.Count - 1]);
            link.LinesSide1.RemoveAt(link.LinesSide1.Count - 1);
            Vector vector1 = new Vector()
            {
                point1 = newPointStart,
                point2 = newPointDestination
            };
            lineStepper.Vector = vector1;
            link.LineSteppers[0] = lineStepper;
        }
        else
        {
            double stepX = (vectorStartOriginal.point2.X - vectorStartOriginal.point1.X) / scale;
            double stepY = (vectorStartOriginal.point2.Y - vectorStartOriginal.point1.Y) / scale;

            Point newPointStart = new Point(vectorStart.point2.X - stepX,
                vectorStart.point2.Y - stepY);

            Point newPointDestination = new Point(vectorDestination.point2.X - stepX,
                vectorDestination.point2.Y - stepY);

            link.road.Points[3] = newPointStart;
            link.road.Points[2] = newPointDestination;
            absoluteLayout.Remove(link.road);
            absoluteLayout.Add(link.road);

            absoluteLayout.Remove(link.LinesSide2[link.LinesSide2.Count - 1]);
            link.LinesSide2.RemoveAt(link.LinesSide2.Count - 1);
            Vector vector1 = new Vector()
            {
                point1 = newPointStart,
                point2 = newPointDestination
            };
            lineStepper.Vector = vector1;
            link.LineSteppers[1] = lineStepper;
        }

    }

    private void OnStepperValueChanged(object sender, ValueChangedEventArgs e)
    {
        Stepper stepper = (Stepper)sender;
        LineStepper lineStepper = GetLineStepperFromLinks(stepper, Links);
        Link link = GetLinkFromLineStepper(lineStepper, Links);
        if (e.NewValue > e.OldValue)
        {
            ExtendRoad(ref link, ref lineStepper);
        }
        else
        {
            NarrowRoad(ref link, ref lineStepper);
        }
        
        //DrawLines2(ref link, lineStepper.Vector, (int)value);
    }

    private async void RoadObjectFromMenu_Tapped(object sender, EventArgs e)
    {
        Image tappedImage = new Image();
        Image sender2 = (Image)sender;
        tappedImage.Source = sender2.Source;
        tappedImage.WidthRequest = sender2.WidthRequest;
        tappedImage.HeightRequest = sender2.HeightRequest;
        if (tappedImage != null) 
        {
            TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += RoadObjectImage_Tapped;
            tappedImage.GestureRecognizers.Add(tapGestureRecognizer);
            tappedImage.ZIndex = 12;
            tappedImage.Scale = 0.7;
            PanGestureRecognizer panGestureRecognizer = new PanGestureRecognizer();
            panGestureRecognizer.PanUpdated += RoadObjectPanGesture_PanUpdated;

            tappedImage.GestureRecognizers.Add(panGestureRecognizer);
            absoluteLayout.Add(tappedImage);
            absoluteLayout.SetLayoutBounds(tappedImage, new Rect(absoluteLayout.Width / 2,
                    absoluteLayout.Height / 2, tappedImage.Width, tappedImage.Height));

            await ObjectMenuBottomSheet.CloseBottomSheet();
        }
        //foreach (Node node in Nodes)
        //{
        //    node.rectangle.IsEnabled = false;
        //}

    }

    private void SwitchIsTrajectoryMode_Toggled(object sender, ToggledEventArgs e)
    {
        foreach (Node node in Nodes)
        {
            node.rectangle.IsEnabled = !e.Value;
        }

        if (AddNewTrajectoryFlag == true || e.Value == false)
        {
            CurrentPoint = null;
        }
        AddNewTrajectoryFlag = e.Value;
    }

    private void RoadObjectImage_Tapped(object sender, TappedEventArgs e)
    {
        Image roadObject = (Image)sender;
        CurrentView = roadObject;
        absoluteLayout.Remove(CurrentRotationSlider);
        Slider slider = new Slider()
        {
            Minimum = -180,
            Maximum = 180,
            Value = roadObject.Rotation,
            ZIndex = 13,
            WidthRequest = 150
        };
        slider.ValueChanged += OnRotationSliderValueChanged;
        absoluteLayout.Add(slider);
        Rect rect = absoluteLayout.GetLayoutBounds(roadObject);
        rect.Y -= 50;
        absoluteLayout.SetLayoutBounds(slider, rect);
        CurrentRotationSlider = slider;
        CurrentRoadObject = roadObject;
    }

    private void OnRotationSliderValueChanged(object sender, ValueChangedEventArgs args)
    {
        CurrentRoadObject.Rotation = args.NewValue;
    }

    private void CrossButton_Clicked(object sender, EventArgs e)
    {
        if (CurrentView == null)
        {
            //crossButton.IsEnabled = false;
            return;
        }
        if (CurrentView is Rectangle rectangle)
        {
            Node node = GetNodeFromRectangle(rectangle, Nodes);
            if (node.imageButtons != null)
            {
                foreach (ImageButton imageButton in node.imageButtons)
                {
                    absoluteLayout.Remove(imageButton);
                }
            }
            foreach (Link link in node.roads)
            {
                foreach (Line line1 in link.LinesSide1)
                {
                    absoluteLayout.Remove(line1);
                }
                foreach (Line line1 in link.LinesSide2)
                {
                    absoluteLayout.Remove(line1);
                }
                foreach (Line line1 in link.MiddleLines)
                {
                    absoluteLayout.Remove(line1);
                }
                foreach (LineStepper lineStepper in link.LineSteppers)
                {
                    absoluteLayout.Remove(lineStepper.Stepper);
                }
                link.LineSteppers.Clear();
                absoluteLayout.Remove(link.road);
                Links.Remove(link);
            }
            absoluteLayout.Remove(node.rectangle);
            Nodes.Remove(node);
            CurrentView = null;
        }
        else
        {
            absoluteLayout.Remove(CurrentView);
        }
        crossButton.IsEnabled = false;
    }

    private double x, y, rotation;
    private bool IsPanWorking = false;
    private void RoadObjectPanGesture_PanUpdated(object sender, PanUpdatedEventArgs e)
    {
        Image roadObject = (Image)sender;
#if ANDROID
        if (!IsPanWorking)
            rotation = roadObject.Rotation;
        roadObject.Rotation = 0;
#endif

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                IsPanWorking = true;
                absoluteLayout.Remove(CurrentRotationSlider);
                break;
            case GestureStatus.Running:
#if ANDROID
                x = roadObject.TranslationX + e.TotalX;
                y = roadObject.TranslationY + e.TotalY;
#endif
#if WINDOWS
                x = e.TotalX;
                y = e.TotalY;
#endif
                roadObject.TranslationX = x;
                roadObject.TranslationY = y;
                break;

            case GestureStatus.Completed:
                Rect rect = absoluteLayout.GetLayoutBounds(roadObject);
                rect.X += x;
                rect.Y += y;
#if ANDROID
                IsPanWorking = false;
                roadObject.Rotation = rotation;
#endif
                roadObject.TranslationX = 0;
                roadObject.TranslationY = 0;
                Debug.WriteLine(roadObject.Rotation);
                absoluteLayout.SetLayoutBounds(roadObject, rect);
                break;
        }
    }

    private void PlusButtonNew_Clicked(object sender, EventArgs e)
    {

    }

    private async void PlusButtonSheet_Clicked(System.Object sender, System.EventArgs e)
    {
        await ObjectMenuBottomSheet.OpenBottomSheet();
    }

    private void ContentPage_Loaded(object sender, EventArgs e)
    {
        //
    }

    private void SwitchIsTwoLaned_Toggled(object sender, ToggledEventArgs e)
    {
        //label.Text = $"Значение {e.Value}";
    }

    private void CheckBoxTwoLaned_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        CheckBox checkBox = (CheckBox)sender;
        Link link = GetLinkByCheckBox(checkBox, Links);
        if (e.Value)
        {
            //
        }
    }

}