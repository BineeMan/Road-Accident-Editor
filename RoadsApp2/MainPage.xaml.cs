using System.Diagnostics;
using System.Numerics;
using System.Runtime.Serialization;
using System.Xml;
using Microsoft.Maui.Controls.Shapes;
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
        
    }

    private async void ContentPage_Loaded(object sender, EventArgs e)
    {
        Debug.WriteLine(FileSystem.AppDataDirectory);
        await RoadAccidentDatabase.Init(); 
    }

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

    private bool AddNewNodeFlag { get; set; }

    private bool AddNewObjectFlag { get; set; }

    private bool AddNewTrajectoryFlag { get; set; }

    private int RectWidth { get; set; }

    private int RectHeight  { get; set; }

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
        RectWidth = 70;
        RectHeight = 70;
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
                    new Rect(x, y, imageButton.Width, imageButton.Height));;
                node.PlusButtons.Add(imageButton);
                rotation += 90;
            }
        }
        else
        {
            rect = absoluteLayout.GetLayoutBounds(rectangle);
            Image imageButton = new Image
            {
                Source = ButtonType.RoadPlusBlackButton,
                WidthRequest = 45,
                HeightRequest = 45,
                Rotation = (double)GetReversedOrientation(orientationExcept),
                ZIndex = 99,
                AnchorY = 1,
                IsVisible = isVisible,
                //Background = Brush.Transparent
            };
            double x = 0, y = 0;
            PanGestureRecognizer panGestureRecognizer = new PanGestureRecognizer();
            panGestureRecognizer.PanUpdated += PlusImagePuttonPanGesture_PanUpdated;
            imageButton.GestureRecognizers.Add(panGestureRecognizer);

            TapGestureRecognizer tapGestureRecogniser = new TapGestureRecognizer();
            tapGestureRecogniser.Tapped += ImgButtonPlus_Tapped;
            imageButton.GestureRecognizers.Add(tapGestureRecogniser);
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
            node.PlusButtons.Add(imageButton);
        }
        Nodes.Add(node);
        if (isVisible)
        {
            PreviousNode = node;
        }
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
            ZIndex = 5,
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
            ZIndex = 5,
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
        if (switchIsCrossroad.IsToggled)
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

        if (Nodes.Count == 0) //if there are no crossroads, then it just creates new crossroads without roads.
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
        Debug.WriteLine("RoadCollision_Tapped");
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

    private void ButtonClean_Clicked(object sender, EventArgs e)
    {
        ResetRoadElements();
        absoluteLayout.Clear();
        //RedrawRoad();
    }

    private void ModifyRoad(ref Link link, ref LineStepper lineStepper, bool isRemoveMode)
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

        int scale = 1;

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
                absoluteLayout.Remove(link.LinesSide1[link.LinesSide1.Count - 1]);
                link.LinesSide1.RemoveAt(link.LinesSide1.Count - 1);
            }
            else
            {
                Line line = DrawLine(lineStepper.Vector);
                link.LinesSide1.Add(line);
                absoluteLayout.Add(line);
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
                absoluteLayout.Remove(link.LinesSide2[link.LinesSide2.Count - 1]);
                link.LinesSide2.RemoveAt(link.LinesSide2.Count - 1);
            }
            else
            {
                Line line = DrawLine(lineStepper.Vector);
                link.LinesSide2.Add(line);
                absoluteLayout.Add(line);
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
            ModifyRoad(ref link, ref lineStepper, false);
        }
        else
        {
            ModifyRoad(ref link, ref lineStepper, true);
        }
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
            newImage.Scale = 0.7;
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
        }
        crossButton.IsEnabled = false;
    }

    private double x, y, rotation;
    private bool IsPanWorking = false;
    public void RoadObjectPanGesture_PanUpdated(object sender, PanUpdatedEventArgs e)
    {
        Image roadObject = (Image)sender;
        Debug.WriteLine("Rotation = ", roadObject.Rotation);
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

    private void ContentPage_Appearing(object sender, EventArgs e)
    {

    }

    private Polygon polygonPan;
    public void PlusImagePuttonPanGesture_PanUpdated(object sender, PanUpdatedEventArgs e)
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
                rect = new Rect(vectorEndPan.point1.X,
                    vectorEndPan.point1.Y, RectWidth, RectHeight);

                if ((Orientation)rotation == Orientation.Left)
                {
                    rect = new Rect(vectorEndPan.point1.X - RectWidth,
                        vectorEndPan.point1.Y, RectWidth, RectHeight);
                }
                else if ((Orientation)rotation == Orientation.Up)
                {
                    rect = new Rect(vectorEndPan.point1.X,
                        vectorEndPan.point1.Y - RectHeight, RectWidth, RectHeight);
                }

                if (switchIsCrossroad.IsToggled)
                {
                    if ((Orientation)rotation == Orientation.Left || (Orientation)rotation == Orientation.Right)
                    {
                        rect.Width = RectWidth / 10;
                        if ((Orientation)rotation == Orientation.Left)
                        {
                            rect.X += RectWidth - (RectWidth / 10);
                        }
                    }
                    else if ((Orientation)rotation == Orientation.Up || (Orientation)rotation == Orientation.Down)
                    {
                        rect.Height = RectHeight / 10;
                        if ((Orientation)rotation == Orientation.Up)
                        {
                            rect.Y += RectHeight - (RectHeight / 10);
                        }
                    }
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