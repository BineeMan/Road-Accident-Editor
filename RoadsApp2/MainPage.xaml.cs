using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using RoadsApp2.Database;
using RoadsApp2.DataClasses;
using RoadsApp2.XMLClasses;
using static RoadsApp2.Utils.Enums;
using static RoadsApp2.Utils.Structs;
using static RoadsApp2.Utils.UserInterfaceUtils;
using static RoadsApp2.Utils.Utils;
using Node = RoadsApp2.Utils.Structs.Node;
using Vector = RoadsApp2.Utils.Structs.Vector;

namespace RoadsApp2;

public partial class MainPage : ContentPage
{
    public static RoadAccidentDatabase RoadAccidentDatabase { get; set; }

    public static XMLConverter XMLConverterMainPage;

    public MainPage()
    {
        InitializeComponent();
        ResetRoadElements();
        XMLConverterMainPage = new XMLConverter(absoluteLayout, this);
        RoadAccidentDatabase = new RoadAccidentDatabase();
        Debug.WriteLine(FileSystem.AppDataDirectory);

    }

    private async void ContentPage_Loaded(object sender, EventArgs e) =>
        await RoadAccidentDatabase.Init();

    private Node PreviousNode { get; set; }

    private Vector VectorStart { get; set; }

    public List<Node> Nodes { get; set; }

    private readonly Point[] pointOrientations = new Point[]
    {
        new Point { X = 0.5, Y = 0 },
        new Point { X = 1, Y = 0.5 },
        new Point { X = 0.5, Y = 1},
        new Point { X = 0, Y = 0.5}
    };

    public static double GlobalScale = 1;
    private bool AddNewNodeFlag { get; set; }
    private bool AddNewObjectFlag { get; set; }

    private bool AddNewTrajectoryFlag { get; set; }

    private double RectWidth { get; set; }

    private double RectHeight { get; set; }

    public List<Link> Links { get; set; }

    public List<Image> RoadObjects { get; set; }

    public List<Line> Trajectories { get; set; }

    private Image CurrentImageButton { get; set; }

    private Orientation CurrentOrientation { get; set; }

    private Rectangle CurrentRectangle { get; set; }

    private Frame CurrentFrame { get; set; }

    private Point? CurrentPoint { get; set; }

    private Slider CurrentRotationSlider { get; set; }

    private Image CurrentRoadObject { get; set; }

    private View CurrentView { get; set; }

    private Rectangle CurrentCollision { get; set; }

    private enum BottomSheetType { CarBottomSheet, SignBottomSheet, None }

    private void ResetRoadElements()
    {
        PreviousNode = new Node() { PlusButtons = new List<Image>() };
        VectorStart = new Vector();
        Nodes = new List<Node>();
        AddNewNodeFlag = false;
        Links = new List<Link>();
        CurrentImageButton = new Image();
        CurrentOrientation = Orientation.Undefined;
        CurrentRectangle = new Rectangle();
        AddNewObjectFlag = false;
        CurrentFrame = new Frame();
#if ANDROID
        GlobalScale = 1;
#endif
#if WINDOWS
        GlobalScale = 1.5d;
#endif
        RectWidth = 75 * GlobalScale;
        RectHeight = 75 * GlobalScale;
        crossButton.IsEnabled = false;
        CurrentCollision = new Rectangle();
        RoadObjects = new List<Image>();
        Trajectories = new List<Line>();
    }

    ///<summary>
    ///This function adds new flags around a crossroads with except orientation which will be avoided
    ///</summary>
    private void AddFlagsAroundRectangle(Rectangle rectangle,
        Orientation orientationExcept = Orientation.Undefined, bool isVisible = true)
    {
        Node node = new Node()
        {
            Rectangle = rectangle,
            PlusButtons = new List<Image>(),
            isActive = true,
            Roads = new List<Link>()
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

                Image imageButton = new Image
                {
                    Source = ButtonType.RoadPlusBlackButton,
                    WidthRequest = 45,
                    HeightRequest = 45,
                    Rotation = (double)rotation,
                    ZIndex = 99,
                    AnchorY = 1,
                    IsVisible = isVisible,
                };
                absoluteLayout.Add(imageButton);
                PanGestureRecognizer panGestureRecognizer = new PanGestureRecognizer();
                panGestureRecognizer.PanUpdated += PlusImagePuttonPanGesture_PanUpdated;
                imageButton.GestureRecognizers.Add(panGestureRecognizer);

                TapGestureRecognizer tapGestureRecogniser = new TapGestureRecognizer();
                tapGestureRecogniser.Tapped += ImgButtonPlus_Tapped;
                imageButton.GestureRecognizers.Add(tapGestureRecogniser);

                rect = absoluteLayout.GetLayoutBounds(rectangle);

                double x = rect.X - imageButton.WidthRequest / 2 + rect.Width * point.X;
                double y = rect.Y - imageButton.HeightRequest + rect.Height * point.Y;

                absoluteLayout.SetLayoutBounds(imageButton,
                    new Rect(x, y, imageButton.Width, imageButton.Height)); ;
                node.PlusButtons.Add(imageButton);
                rotation += 90;
            }
        }
        else
        {
            foreach (Point point in pointOrientations)
            {
                if (rotation == orientationExcept)
                {
                    rotation += 90;
                    continue;
                }

                rect = absoluteLayout.GetLayoutBounds(rectangle);
                if ((point == pointOrientations[0] || point == pointOrientations[2]) && rect.Width < rect.Height )
                {
                    rotation += 90;
                    continue;
                }
                else if ((point == pointOrientations[1] || point == pointOrientations[3]) && rect.Width > rect.Height)
                {
                    rotation += 90;
                    continue;
                }

                Image imageButton = new Image
                {
                    Source = ButtonType.RoadPlusBlackButton,
                    WidthRequest = 45,
                    HeightRequest = 45,
                    Rotation = (double)rotation,
                    ZIndex = 99,
                    AnchorY = 1,
                    IsVisible = isVisible,
                };
                absoluteLayout.Add(imageButton);
                PanGestureRecognizer panGestureRecognizer = new PanGestureRecognizer();
                panGestureRecognizer.PanUpdated += PlusImagePuttonPanGesture_PanUpdated;
                imageButton.GestureRecognizers.Add(panGestureRecognizer);

                TapGestureRecognizer tapGestureRecogniser = new TapGestureRecognizer();
                tapGestureRecogniser.Tapped += ImgButtonPlus_Tapped;
                imageButton.GestureRecognizers.Add(tapGestureRecogniser);


                double x = rect.X - imageButton.WidthRequest / 2 + rect.Width * point.X;
                double y = rect.Y - imageButton.HeightRequest + rect.Height * point.Y;

                absoluteLayout.SetLayoutBounds(imageButton,
                    new Rect(x, y, imageButton.Width, imageButton.Height)); ;
                node.PlusButtons.Add(imageButton);
                rotation += 90;

                node.PlusButtons.Add(imageButton);
                
            }   
            if (isVisible)
            {
                PreviousNode = node;
            }
        }
        Nodes.Add(node);
    }

    private void AddFlagAroundRoad(ref Link link)
    {
        PointCollection points = link.Road.Points;
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
            ZIndex = 99,
            AnchorY = 0.5,
            AnchorX = 0.5,
            IsVisible = false,
            Value = 1,
            Margin = thickness
        };
        stepper.ValueChanged += OnStepperValueChanged;
        absoluteLayout.Add(stepper);
        absoluteLayout.SetLayoutBounds(stepper,
                new Rect(xUpHalf, yUpHalf, stepper.Width, stepper.Height));

        double l = 6f / 3f;
        xUpHalf = ((vectorStart.point1.X + step) + (l * (vectorDestination.point1.X + step))) / (1 + l);
        yUpHalf = ((vectorStart.point1.Y + step) + (l * (vectorDestination.point1.Y + step))) / (1 + l);


        link.LineSteppers.Add(new LineStepper()
        {
            Stepper = stepper,
            Vector = new Vector() { point1 = vectorStart.point1, point2 = vectorDestination.point1 },
            //CheckBoxTwoLaned = checkBox
        });

        stepper = new Stepper()
        {
            Maximum = 4,
            Minimum = 1,
            HorizontalOptions = LayoutOptions.Center,
            Scale = scale,
            ZIndex = 99,
            IsVisible = false,
            Value = 1,
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
    }
    ///<summary>
    ///This event is only used to toggle flags around a crossroad. Doesn't trigger drawing.
    ///</summary>
    public void Crossroads_Tapped(object sender, TappedEventArgs e)
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
            SetImageButtonsVisibility(node.PlusButtons, false);
            PreviousNode = new Node() { PlusButtons = new List<Image>() };
        }
        else
        {
            SetImageButtonsVisibility(PreviousNode.PlusButtons, false);
            SetImageButtonsVisibility(node.PlusButtons, true);
            PreviousNode = node;
        }
        ToggleSteppersVisibility(Links, false);

        CurrentRectangle = rectangle;
    }

    ///<summary>
    ///Event for plus and destination image buttons.
    ///</summary>
    public void ImgButtonPlus_Tapped(object sender, TappedEventArgs e)
    {
        CurrentImageButton.Source = ButtonType.RoadPlusBlackButton;
        Image imageButton = (Image)sender;
        if (CurrentImageButton.Equals(imageButton))
        //if in AddNewNode mode clicked object is the same as image button that toggled this mode, then it cancel AddNewNodeMode
        {
            AddNewNodeFlag = false;
            CurrentImageButton = new Image();
            SetImageButtonsType(ButtonType.RoadPlusBlackButton, Nodes);
            Node targetNode = GetNodeFromImageButton(imageButton, Nodes);
            foreach (Node node in Nodes)
            {
                if (!node.Equals(targetNode))
                {
                    SetImageButtonsVisibility(node.PlusButtons, false);
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
            Rect rect = absoluteLayout.GetLayoutBounds(targetNode.Rectangle);
            Vector destCoords = GetVectorForOrientation((Orientation)imageButton.Rotation, rect);
            Polygon road = DrawRoad(VectorStart, destCoords, (Orientation)imageButton.Rotation, (Orientation)CurrentImageButton.Rotation);

            Rect collisionRect = GetCollision(road);
            Rectangle collisionBox = new Rectangle() { Background = Brush.Transparent };

            TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += RoadCollision_Tapped;
            collisionBox.GestureRecognizers.Add(tapGestureRecognizer);
            absoluteLayout.Add(collisionBox);
            absoluteLayout.SetLayoutBounds(collisionBox, collisionRect);

            Link link = new Link()
            {
                Road = road,
                LinesSide1 = new List<Line>(),
                LinesSide2 = new List<Line>(),
                MiddleLines = new List<Line>(),
                RectangleCollision = collisionBox
            };
            AddFlagAroundRoad(ref link);

            PointCollection points = new PointCollection();
            foreach (Point point in link.Road.Points)
            {
                points.Add(point);
            }
            link.OriginalRoadPoints = points;

            targetNode.Roads.Add(link);
            absoluteLayout.Add(road);

            foreach (Node node in Nodes)
            {
                SetImageButtonsVisibility(node.PlusButtons, false);
            }
            SetImageButtonsType(ButtonType.RoadPlusBlackButton, Nodes);
            absoluteLayout.Remove(CurrentImageButton);
            absoluteLayout.Remove(imageButton);
            AddNewNodeFlag = false;
            Links.Add(link);
            foreach (Link link2 in Links)
            {
                link2.RectangleCollision.IsEnabled = true;
            }
        }
        else
        //if sending image button belongs to the same crossroad, new image button become starting point
        {
            imageButton.Source = ButtonType.RoadPlusGreenButton;
            Node nodeTarget = GetNodeFromImageButton(imageButton, Nodes);
            Orientation orientation = (Orientation)imageButton.Rotation;
            CurrentOrientation = orientation;
            Rect rectangleRect = absoluteLayout.GetLayoutBounds(nodeTarget.Rectangle);
            VectorStart = GetVectorForOrientation(orientation, rectangleRect);

            foreach (Node node in Nodes)
            {
                if (!node.Equals(nodeTarget))
                {
                    SetImageButtonsVisibility(node.PlusButtons, true);
                }
            }

            SetImageButtonsType(ButtonType.DestinationButton, Nodes, nodeTarget.Rectangle);
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
    private async void AbsoluteLayout_Tapped(object sender, TappedEventArgs e)
    {
        Point? tappedPoint = e.GetPosition((View)sender);
        if (tappedPoint == null) { return; }
        absoluteLayout.Remove(CurrentRotationSlider);
        crossButton.IsEnabled = false;
        Rectangle rectangle;

        Rect rect = new Rect();
        rect = new Rect(tappedPoint.Value.X - RectWidth / 2,
                tappedPoint.Value.Y - RectHeight / 2, RectWidth, RectHeight);

        if (switchIsTrajectoryMode.IsToggled)
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
                Trajectories.Add(line);
                absoluteLayout.Add(line);
                CurrentPoint = tappedPoint;
            }
        }
        else if (!switchIsTrajectoryMode.IsToggled && Nodes.Count > 0 && AddNewNodeFlag)
        {
            string action = await DisplayActionSheet("Выберите новый \nобъект пристыковки",
                             "Отмена", null, "Новый перекресток", "Новый поворот", "Ничего");
            if (action != null)
            {
                if (action == "Ничего")
                {
                    if (CurrentOrientation == Orientation.Left || CurrentOrientation == Orientation.Right)
                    {
                        rect = new Rect(tappedPoint.Value.X - RectWidth / 10,
                            tappedPoint.Value.Y - RectHeight / 2, RectWidth / 10, RectHeight);
                    }
                    else if (CurrentOrientation == Orientation.Up || CurrentOrientation == Orientation.Down)
                    {
                        rect = new Rect(tappedPoint.Value.X - RectWidth / 2,
                            tappedPoint.Value.Y - RectHeight / 10, RectWidth, RectHeight / 10);
                    }

                }
                else if (action == "Отмена")
                {
                    return;
                }
            }
            else
            {
                return;
            }
        }
        if (Nodes.Count == 0) //if there are no crossroads, then it just creates new crossroads without roads.
        {
            string answer = await DisplayActionSheet("Выберите новый\n дорожный объект", "Отмена", null, "Перекресток", "Начало дороги");
            if (answer != null)
            {
                if (answer == "Отмена")
                {
                    return;
                }
                else if (answer == "Перекресток")
                {
                    rectangle = GetRectangle(rect, Crossroads_Tapped);
                    absoluteLayout.Add(rectangle);
                    absoluteLayout.SetLayoutBounds(rectangle, rect);
                    AddFlagsAroundRectangle(rectangle, GetReversedOrientation(CurrentOrientation), false);
                }
                else if (answer == "Начало дороги")
                {
                    string answer2 = await DisplayActionSheet("Выберите начальное положение дороги", "Отмена", null, "Горизотальное", "Вертикальное");
                    if (answer != null)
                    {
                        if (answer2 == "Горизотальное")
                        {
                            rect.Height = rect.Height / 20;
                            rectangle = GetRectangle(rect, Crossroads_Tapped);
                            absoluteLayout.Add(rectangle);
                            absoluteLayout.SetLayoutBounds(rectangle, rect);
                            AddFlagsAroundRectangle(rectangle, Orientation.Undefined, true);
                        }
                        else if (answer2 == "Вертикальное")
                        {
                            rect.Width = rect.Width / 20;
                            rectangle = GetRectangle(rect, Crossroads_Tapped);
                            absoluteLayout.Add(rectangle);
                            absoluteLayout.SetLayoutBounds(rectangle, rect);
                            AddFlagsAroundRectangle(rectangle, Orientation.Undefined, true);
                        }
                        else if (answer2 == "Отмена")
                        {
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
        }
        else if (Nodes.Count > 0 || !AddNewNodeFlag) // resetting  image buttons if user clicks on empty area
        {
            Node targetNode = GetNodeFromImageButton(CurrentImageButton, Nodes);
            foreach (Node node in Nodes)
            {
                SetImageButtonsVisibility(node.PlusButtons, false);
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
            Vector destCoords = GetVectorForOrientation(CurrentOrientation, rect, true);
            SetImageButtonsType(ButtonType.RoadPlusBlackButton, Nodes);
            AddFlagsAroundRectangle(rectangle, GetReversedOrientation(CurrentOrientation), false);
            Polygon road = DrawRoad(VectorStart, destCoords, CurrentOrientation, CurrentOrientation);
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
                Road = road,
                LinesSide1 = new List<Line>(),
                LinesSide2 = new List<Line>(),
                MiddleLines = new List<Line>(),
                RectangleCollision = collisionBox
            };

            PointCollection points = new PointCollection();
            foreach (Point point in link.Road.Points)
            {
                points.Add(point);
            }
            link.OriginalRoadPoints = points;

            AddFlagAroundRoad(ref link);
            
            Links.Add(link);

            targetNode.Roads.Add(link);
            callerNode.Roads.Add(link);

            foreach (Node node in Nodes)
            {
                if (!node.Equals(targetNode))
                {
                    SetImageButtonsVisibility(node.PlusButtons, false);
                }
            }
            absoluteLayout.Remove(CurrentImageButton);
            AddNewNodeFlag = false;
        }

    }

    public void RoadCollision_Tapped(object sender, TappedEventArgs e)
    {
        Rectangle collision = (Rectangle)sender;
        Link link = GetLinkByCollision(collision, Links);
        if (!CurrentCollision.Equals(collision))
        {
            Link linkPrevious = GetLinkByCollision(CurrentCollision, Links);
            ToggleSteppersVisibility2(linkPrevious.LineSteppers, false);
        }

        SetImageButtonsVisibility(PreviousNode.PlusButtons, false);
        PreviousNode = new Node() { PlusButtons = new List<Image>() };
        CurrentRectangle = new Rectangle();
        ToggleSteppersVisibility2(link.LineSteppers, true);
        CurrentCollision = collision;
    }

    private async void ButtonClean_Clicked(object sender, EventArgs e)
    {
        bool answer = await DisplayAlert("Очистка рабочей области",
            "Вы действительно хотите очистить рабочую область?", "Да", "Отмена");
        if (answer)
        {
            ResetRoadElements();
            absoluteLayout.Clear();
        }
    }

    private async Task<List<Line>> DrawLineByAnswer(Vector vector, double stepX = 0, double stepY = 0)
    {
        string action = await DisplayActionSheet("Выберите новую полосу \nразметки",
            "Отмена", null, "Сплошная", "Прерывистая", "Двойная сплошная", "Сплошная слева и прерывистая", "Сплошная справа и прерывистая", "Ничего");
        List<Line> lines = new List<Line>();
        if (action != null)
        {
            if (action == "Отмена")
            {
                Line line2 = new Line()
                {
                    Fill = Brush.Transparent,
                    Stroke = Brush.Transparent,
                    StrokeThickness = 3,
                    IsEnabled = false,
                    ZIndex = 2,
                    X1 = vector.point1.X,
                    Y1 = vector.point1.Y,
                    X2 = vector.point2.X,
                    Y2 = vector.point2.Y,
                };
                lines.Add(line2);
            }
            double scale = 10;
            
            Brush lineColor = Brush.White;
            if (action == "Прерывистая")
            {
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
                lines.Add(line);
            }
            else if (action == "Сплошная")
            {
                Line line = new Line()
                {
                    Fill = lineColor,
                    Stroke = lineColor,
                    StrokeThickness = 3,
                    IsEnabled = false,
                    ZIndex = 2,
                    X1 = vector.point1.X,
                    Y1 = vector.point1.Y,
                    X2 = vector.point2.X,
                    Y2 = vector.point2.Y,
                };
                lines.Add(line);
            }
            else if (action == "Двойная сплошная")
            {
                Line line = new Line()
                {
                    Fill = lineColor,
                    Stroke = lineColor,
                    StrokeThickness = 3,
                    IsEnabled = false,
                    ZIndex = 2,
                    X1 = vector.point1.X,
                    Y1 = vector.point1.Y,
                    X2 = vector.point2.X,
                    Y2 = vector.point2.Y,
                };
                Line line2 = new Line()
                {
                    Fill = lineColor,
                    Stroke = lineColor,
                    StrokeThickness = 3,
                    IsEnabled = false,
                    ZIndex = 2,
                    X1 = vector.point1.X + stepX / scale,
                    Y1 = vector.point1.Y + stepY / scale,
                    X2 = vector.point2.X + stepX / scale,
                    Y2 = vector.point2.Y + stepY / scale,
                };
                lines.Add(line);
                lines.Add(line2);
            }
            else if (action == "Сплошная слева и прерывистая")
            {
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
                Line line2 = new Line()
                {
                    Fill = lineColor,
                    Stroke = lineColor,
                    StrokeThickness = 3,
                    IsEnabled = false,
                    ZIndex = 2,
                    X1 = vector.point1.X + stepX / scale,
                    Y1 = vector.point1.Y + stepY / scale,
                    X2 = vector.point2.X + stepX / scale,
                    Y2 = vector.point2.Y + stepY / scale,
                };
                lines.Add(line);
                lines.Add(line2);
            }
            else if (action == "Сплошная справа и прерывистая")
            {
                Line line = new Line()
                {
                    Fill = lineColor,
                    Stroke = lineColor,
                    StrokeThickness = 3,
                    IsEnabled = false,
                    ZIndex = 2,
                    StrokeDashArray = { 2, 2 },
                    StrokeDashOffset = 10,
                    X1 = vector.point1.X + stepX / scale,
                    Y1 = vector.point1.Y + stepY / scale,
                    X2 = vector.point2.X + stepX / scale,
                    Y2 = vector.point2.Y + stepY / scale,
                };
                Line line2 = new Line()
                {
                    Fill = lineColor,
                    Stroke = lineColor,
                    StrokeThickness = 3,
                    IsEnabled = false,
                    ZIndex = 2,
                    X1 = vector.point1.X,
                    Y1 = vector.point1.Y,
                    X2 = vector.point2.X,
                    Y2 = vector.point2.Y,
                };
                lines.Add(line);
                lines.Add(line2);
            }
            else if (action == "Ничего")
            {
                Line line2 = new Line()
                {
                    Fill = Brush.Transparent,
                    Stroke = Brush.Transparent,
                    StrokeThickness = 3,
                    IsEnabled = false,
                    ZIndex = 2,
                    X1 = vector.point1.X,
                    Y1 = vector.point1.Y,
                    X2 = vector.point2.X,
                    Y2 = vector.point2.Y,
                };
                lines.Add(line2);
            }
            
        }
        else
        {
            Line line2 = new Line()
            {
                Fill = Brush.Transparent,
                Stroke = Brush.Transparent,
                StrokeThickness = 3,
                IsEnabled = false,
                ZIndex = 2,
                X1 = vector.point1.X,
                Y1 = vector.point1.Y,
                X2 = vector.point2.X,
                Y2 = vector.point2.Y,
            };
            lines.Add(line2);
        }
        return lines;
    }

    private void DrawLines2(ref Link link, Vector vectorStepper, int newAmount)
    {
        PointCollection points = link.Road.Points;
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


    private async void ModifyRoad(Link link, LineStepper lineStepper, bool isRemoveMode)
    {
        PointCollection points = link.Road.Points;
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

        Vector vector = new Vector()
        {
            point1 = points[0],
            point2 = points[1]
        };

        double scale = 1;

        if (lineStepper.Vector.Equals(vector))
        {
            double stepX = (vectorStartOriginal.point2.X - vectorStartOriginal.point1.X) / scale;
            double stepY = (vectorStartOriginal.point2.Y - vectorStartOriginal.point1.Y) / scale;
            if (isRemoveMode)
            {
                stepX = -stepX;
                stepY = -stepY;
            }

            Point newPointStart = new Point(vectorStart.point1.X - stepX,
                vectorStart.point1.Y - stepY);
            Point newPointDestination = new Point(vectorDestination.point1.X - stepX,
                vectorDestination.point1.Y - stepY);
            link.Road.Points[0] = newPointStart;
            link.Road.Points[4] = newPointStart;
            link.Road.Points[1] = newPointDestination;

            if (isRemoveMode)
            {
                if (link.LinesSide1.Count > 1)
                {
                    double vectorA = (link.LinesSide1[^1].X1 - link.LinesSide1[^2].X1);
                    double vectorB = (link.LinesSide1[^1].Y1 - link.LinesSide1[^2].Y1);
                    double vectorLength = Math.Sqrt(Math.Pow(vectorA, 2) + Math.Pow(vectorB, 2));
                    if (vectorLength < 20)
                    {
                        absoluteLayout.Remove(link.LinesSide1[^1]);
                        link.LinesSide1.RemoveAt(link.LinesSide1.Count - 1);
                    }
                }
                absoluteLayout.Remove(link.LinesSide1[^1]);
                link.LinesSide1.RemoveAt(link.LinesSide1.Count - 1);
            }
            else
            {
                List<Line> lines = await DrawLineByAnswer(lineStepper.Vector, stepX, stepY);
                if (lines == null)
                    return;
                foreach (Line line in lines)
                {
                    link.LinesSide1.Add(line);
                    absoluteLayout.Add(line);
                }
            }

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
            if (isRemoveMode)
            {
                stepX = -stepX;
                stepY = -stepY;
            }

            Point newPointStart = new Point(vectorStart.point2.X + stepX,
                vectorStart.point2.Y + stepY);

            Point newPointDestination = new Point(vectorDestination.point2.X + stepX,
                vectorDestination.point2.Y + stepY);

            link.Road.Points[3] = newPointStart;
            link.Road.Points[2] = newPointDestination;
            absoluteLayout.Remove(link.Road);
            absoluteLayout.Add(link.Road);
            if (isRemoveMode)
            {
                if (link.LinesSide2.Count > 1)
                {
                    double vectorA = (link.LinesSide2[^1].X1 - link.LinesSide2[^2].X1);
                    double vectorB = (link.LinesSide2[^1].Y1 - link.LinesSide2[^2].Y1);
                    double vectorLength = Math.Sqrt(Math.Pow(vectorA, 2) + Math.Pow(vectorB, 2));
                    if (vectorLength < 20)
                    {
                        absoluteLayout.Remove(link.LinesSide2[^1]);
                        link.LinesSide2.RemoveAt(link.LinesSide2.Count - 1);
                    }
                }
                absoluteLayout.Remove(link.LinesSide2[^1]);
                link.LinesSide2.RemoveAt(link.LinesSide2.Count - 1);
            }
            else
            {
                List<Line> lines = await DrawLineByAnswer(lineStepper.Vector, stepX, stepY);
                if (lines == null)
                    return;
                foreach (Line line in lines)
                {
                    link.LinesSide2.Add(line);
                    absoluteLayout.Add(line);
                }
            }

            Vector vector1 = new Vector()
            {
                point1 = newPointStart,
                point2 = newPointDestination
            };
            lineStepper.Vector = vector1;
            link.LineSteppers[1] = lineStepper;
        }

    }

    public void OnStepperValueChanged(object sender, ValueChangedEventArgs e)
    {
        Stepper stepper = (Stepper)sender;
        LineStepper lineStepper = GetLineStepperFromLinks(stepper, Links);
        Link link = GetLinkFromLineStepper(lineStepper, Links);
        if (e.NewValue > e.OldValue)
        {
            ModifyRoad(link, lineStepper, false);
        }
        else
        {
            ModifyRoad(link, lineStepper, true);
        }

        //double value = e.NewValue;
        //Stepper stepper = (Stepper)sender;
        //LineStepper lineStepper = GetLineStepperFromLinks(stepper, Links);

        //Link link = GetLinkFromLineStepper(lineStepper, Links);

        //DrawLines2(ref link, lineStepper.Vector, (int)value);
    }

    private async void RoadObjectFromMenu_Tapped(object sender, EventArgs e)
    {
        Image newImage = new Image();
        Image tappedImage = (Image)sender;
        newImage.Source = tappedImage.Source;
        newImage.WidthRequest = tappedImage.WidthRequest;
        newImage.HeightRequest = tappedImage.HeightRequest;
        if (newImage != null)
        {
            TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += RoadObjectImage_Tapped;
            newImage.GestureRecognizers.Add(tapGestureRecognizer);
            newImage.ZIndex = 12;
            newImage.Scale = 0.7 * GlobalScale;
            PanGestureRecognizer panGestureRecognizer = new PanGestureRecognizer();
            panGestureRecognizer.PanUpdated += RoadObjectPanGesture_PanUpdated;

            newImage.GestureRecognizers.Add(panGestureRecognizer);
            absoluteLayout.Add(newImage);
            absoluteLayout.SetLayoutBounds(newImage, new Rect(absoluteLayout.Width / 2,
                    absoluteLayout.Height / 2, newImage.Width, newImage.Height));

            RoadObjects.Add(newImage);
            switch (CurrentBottomSheetType)
            {
                case BottomSheetType.SignBottomSheet:
                    await SignMenuBottomSheet.CloseBottomSheet(); break;
                case BottomSheetType.CarBottomSheet:
                    await CarMenuBottomSheet.CloseBottomSheet(); break;
            }
        }
    }

    private void ToggleAllRoadObjects(bool isEnabled)
    {
        foreach (Image image in RoadObjects)
        {
            image.IsEnabled = isEnabled;
        }
        foreach (Node node in Nodes)
        {
            node.Rectangle.IsEnabled = !isEnabled;
            foreach (Link link in node.Roads)
            {
                link.Road.IsEnabled = isEnabled;
                link.RectangleCollision.InputTransparent = isEnabled;
                link.RectangleCollision.IsEnabled = !isEnabled;
            }
        }
    }

    private void SwitchIsTrajectoryMode_Toggled(object sender, ToggledEventArgs e)
    {
        ToggleAllRoadObjects(e.Value);

        if (AddNewTrajectoryFlag == true || e.Value == false)
        {
            CurrentPoint = null;
        }
        AddNewTrajectoryFlag = e.Value;
    }

    public void RoadObjectImage_Tapped(object sender, TappedEventArgs e)
    {
        Image roadObject = (Image)sender;
        CurrentView = roadObject;
        crossButton.IsEnabled = true;
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
            return;
        }
        if (CurrentView is Rectangle rectangle)
        {
            Node node = GetNodeFromRectangle(rectangle, Nodes);
            if (node.PlusButtons != null)
            {
                foreach (Image imageButton in node.PlusButtons)
                {
                    absoluteLayout.Remove(imageButton);
                }
            }
            foreach (Link link in node.Roads)
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
                absoluteLayout.Remove(link.Road);
                Links.Remove(link);
            }
            absoluteLayout.Remove(node.Rectangle);
            Nodes.Remove(node);
            CurrentView = null;
        }
        else
        {
            absoluteLayout.Remove(CurrentView);
            if (CurrentRotationSlider != null)
            {
                absoluteLayout.Remove(CurrentRotationSlider);
            }

        }
        crossButton.IsEnabled = false;
    }
    

    private double x, y, rotation;
    private bool IsPanWorking = false;
    public void RoadObjectPanGesture_PanUpdated(object sender, PanUpdatedEventArgs e)
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
                absoluteLayout.SetLayoutBounds(roadObject, rect);
                break;
        }
    }

    private BottomSheetType CurrentBottomSheetType = BottomSheetType.None;

    private bool IsFloatingActionOpened = false;
    private async Task AnimateFloatingAction()
    {
        if (IsFloatingActionOpened)
        {
            Task t1 = PlusButtonNew.RotateTo(90);
            Task t2 = CarIcon.TranslateTo(70, 0);
            Task t3 = SignIcon.TranslateTo(70, 0);
            await Task.WhenAll(t1, t2, t3);
            IsFloatingActionOpened = false;
        }
        else
        {
            await PlusButtonNew.RotateTo(-90);
            await CarIcon.TranslateTo(0, 0, 100);
            await SignIcon.TranslateTo(0, 0, 100);
            IsFloatingActionOpened = true;
        }
    }

    private async void PlusButtonSheet_Clicked(System.Object sender, System.EventArgs e) =>
        await AnimateFloatingAction();

    private Vector vectorStartPan, vectorEndPan, vectorTemp;

    private void TapGestureRecognizerCategoryObject_Tapped(object sender, TappedEventArgs e)
    {

    }

    private async void SignIcon_Clicked(object sender, EventArgs e)
    {
        CurrentBottomSheetType = BottomSheetType.SignBottomSheet;
        Task t1 = SignMenuBottomSheet.OpenBottomSheet();
        Task t2 = AnimateFloatingAction();
        await Task.WhenAll(t1, t2);
    } 

    private async void CarIcon_Clicked(object sender, EventArgs e)
    {
        CurrentBottomSheetType = BottomSheetType.CarBottomSheet;
        Task t1 = CarMenuBottomSheet.OpenBottomSheet();
        Task t2 = AnimateFloatingAction();
        await Task.WhenAll(t1, t2);
    }

    public async Task<ImageSource> TakeScreenshotAsync()
    {
        if (Screenshot.Default.IsCaptureSupported)
        {
            IScreenshotResult screen = await Screenshot.Default.CaptureAsync();

            Stream stream = await screen.OpenReadAsync();

            //MemoryStream memoryStream = new MemoryStream();
            //stream.CopyTo(memoryStream);
            //await System.IO.File.WriteAllBytesAsync("G:\\C#\\RoadsApp2\\RoadsApp2\\Saved\\test.png", memoryStream.ToArray());
            return ImageSource.FromStream(() => stream);
        }

        return null;
    }

    private async void ButtonScreenshot_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(MainPage));
        IScreenshotResult screen = await absoluteLayout.CaptureAsync();
        Stream stream = await screen.OpenReadAsync();
        MemoryStream memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        await File.WriteAllBytesAsync("G:\\C#\\RoadsApp2\\RoadsApp2\\Saved\\DebugImage2.png", memoryStream.ToArray());
    }

    private void ContentPage_Appearing(object sender, EventArgs e)
    {

    }

    private Polygon polygonPan;
    private Point imageButtonOrigin = new Point();
    public async void PlusImagePuttonPanGesture_PanUpdated(object sender, PanUpdatedEventArgs e)
    {
        Image imageButton = (Image)sender;
        if (!IsPanWorking)
        {
            rotation = imageButton.Rotation;
#if ANDROID
            imageButton.Rotation = 0;
#endif
        }
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                IsPanWorking = true;
                Node node = GetNodeFromImageButton(imageButton, Nodes);
                Rect rectCrossroad = absoluteLayout.GetLayoutBounds(node.Rectangle);
                Orientation orientation = (Orientation)rotation;
                vectorStartPan = GetVectorForOrientation(orientation, rectCrossroad);
                vectorEndPan = vectorStartPan;
                vectorTemp = vectorStartPan;
                polygonPan = DrawRoad(vectorStartPan, vectorEndPan, orientation, orientation);
                absoluteLayout.Add(polygonPan);
                absoluteLayout.SetLayoutBounds(polygonPan, new Rect(0, 0, absoluteLayout.Width, absoluteLayout.Height));
                imageButtonOrigin.X = imageButton.X;
                imageButtonOrigin.Y = imageButton.Y;
                break;
            case GestureStatus.Running:
#if ANDROID
                x = imageButton.TranslationX + e.TotalX;
                y = imageButton.TranslationY + e.TotalY;
#endif
#if WINDOWS
                x = e.TotalX;
                y = e.TotalY;
#endif
                imageButton.TranslationX = x;
                imageButton.TranslationY = y;
                vectorEndPan.point1 = new Point(vectorTemp.point1.X + x, vectorTemp.point1.Y + y);
                vectorEndPan.point2 = new Point(vectorTemp.point2.X + x, vectorTemp.point2.Y + y);
                RedrawRoad(ref polygonPan, vectorEndPan);
                break;

            case GestureStatus.Canceled:
                absoluteLayout.Remove(polygonPan);
                IsPanWorking= false;
                break;

            case GestureStatus.Completed:
                IsPanWorking = false;
                Rect rect = new Rect();
                rect = new Rect(vectorEndPan.point1.X-1,
                    vectorEndPan.point1.Y, RectWidth, RectHeight);

                switch ((Orientation)rotation)
                {
                    case Orientation.Up:
                        rect = new Rect(vectorEndPan.point1.X,
                        vectorEndPan.point1.Y - RectHeight + 1, RectWidth, RectHeight);
                        break;

                    case Orientation.Right:
                        rect = new Rect(vectorEndPan.point1.X - 1,
                            vectorEndPan.point1.Y + 0.5, RectWidth, RectHeight);
                        break;

                    case Orientation.Down:
                        rect = new Rect(vectorEndPan.point1.X + 0.5,
                            vectorEndPan.point1.Y-1, RectWidth, RectHeight);
                        break;

                    case Orientation.Left:
                        rect = new Rect(vectorEndPan.point1.X - RectWidth + 1,
                        vectorEndPan.point1.Y + 0.5, RectWidth, RectHeight);
                        break;
                }

                string action = await DisplayActionSheet("Выберите новый \nобъект пристыковки",
                    "Отмена", null, "Новый перекресток", "Новый поворот", "Ничего");
                if (action != null)
                {
                    if (action == "Ничего")
                    {
                        double scale = 20;
                        if ((Orientation)rotation == Orientation.Left || (Orientation)rotation == Orientation.Right)
                        {
                            rect.Width = RectWidth / scale;
                            if ((Orientation)rotation == Orientation.Left)
                            {
                                rect.X += RectWidth - (RectWidth / scale);
                            }
                        }
                        else if ((Orientation)rotation == Orientation.Up || (Orientation)rotation == Orientation.Down)
                        {
                            rect.Height = RectHeight / scale;
                            if ((Orientation)rotation == Orientation.Up)
                            {
                                rect.Y += RectHeight - (RectHeight / scale);
                            }
                        }
                    }
                    else if (action == "Отмена")
                    {
                        absoluteLayout.Remove(polygonPan);
                        imageButton.TranslationX = 0;
                        imageButton.TranslationY = 0;
                        return;
                    }
                }
                else
                {
                    absoluteLayout.Remove(polygonPan);
                    imageButton.TranslationX = 0;
                    imageButton.TranslationY = 0;
                    return;
                }

                Rectangle rectangle = GetRectangle(rect, Crossroads_Tapped);
                absoluteLayout.Add(rectangle);
                absoluteLayout.SetLayoutBounds(rectangle, rect);
                Rect rect2 = absoluteLayout.GetLayoutBounds(rectangle);
                AddFlagsAroundRectangle(rectangle, GetReversedOrientation((Orientation)rotation), false);

                Rect collisionRect = GetCollision(polygonPan);
                Rectangle collisionBox = new Rectangle() { Background = Brush.Transparent };

                TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.Tapped += RoadCollision_Tapped;
                collisionBox.GestureRecognizers.Add(tapGestureRecognizer);
                absoluteLayout.Add(collisionBox);
                absoluteLayout.SetLayoutBounds(collisionBox, collisionRect);

                Node targetNode = GetNodeFromImageButton(imageButton, Nodes);
                Node callerNode = GetNodeFromRectangle(rectangle, Nodes);
                foreach (Link link2 in Links)
                {
                    link2.RectangleCollision.IsEnabled = true;
                }
                Link link = new Link()
                {
                    Road = polygonPan,
                    LinesSide1 = new List<Line>(),
                    LinesSide2 = new List<Line>(),
                    MiddleLines = new List<Line>(),
                    RectangleCollision = collisionBox
                };

                PointCollection points = new PointCollection();
                foreach (Point point in link.Road.Points)
                {
                    points.Add(point);
                }
                link.OriginalRoadPoints = points;

                AddFlagAroundRoad(ref link);

                Links.Add(link);

                targetNode.Roads.Add(link);
                callerNode.Roads.Add(link);
                foreach (Node node1 in Nodes)
                {
                    if (!node1.Equals(targetNode))
                    {
                        SetImageButtonsVisibility(node1.PlusButtons, false);
                    }
                }
                absoluteLayout.Remove(imageButton);
                break;
        }
    }
}


