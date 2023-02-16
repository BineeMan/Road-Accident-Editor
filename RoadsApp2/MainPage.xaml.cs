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

    private Node PrevNode { get; set; }

    private Vector LineCoords { get; set; }

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

    private int RectWidth { get; set; }

    private int RectHeight  { get; set; }

    private List<Link> links;

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

    private void AdaptUIElementsWindows()
    {
        switchIsCrossroad.Margin = new Thickness(0, 0, -50, 0);
        switchIsTrajectoryMode.Margin = new Thickness(10, 0, -50, 0);

    }

    private void ResetRoadElements()
    {
        PrevNode = new Node() { imageButtons = new List<ImageButton>() };
        LineCoords = new Vector();
        nodes = new List<Node>();
        AddNewNodeFlag = false;
        links = new List<Link>();
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
        RectWidth = 200;
        RectHeight = 200;
#endif
#if ANDROID
        RectWidth = 100;
        RectHeight = 100;
#endif
        crossButton.IsEnabled = false;
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
            PrevNode = node;
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
            Background = Color.FromRgb(81, 43, 212),
            BackgroundColor = Color.FromRgb(81, 43, 212),
            Margin = thickness
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
            BackgroundColor = Color.FromRgb(81, 43, 212),
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

    private void RedrawRoad()
    {
        foreach (Node node in nodes)
        {
            absoluteLayout.Remove(node.rectangle);
            foreach (ImageButton imageButton in node.imageButtons)
            {
                absoluteLayout.Remove(imageButton);
            }
        }

        foreach (Link link in links)
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
        Node node = GetNodeFromRectangle(rectangle, nodes);
        if (rectangle.Equals(CurrentRectangle))
        {
            ToggleSteppersVisibility(node.roads , true);
        }
        else if (PrevNode.Equals(rectangle))
        {
            SetImageButtonsVisibility(node.imageButtons, false);
        }
        else
        {
            SetImageButtonsVisibility(PrevNode.imageButtons, false);
            SetImageButtonsVisibility(node.imageButtons, true);
            PrevNode = node;
        }
        CurrentRectangle = rectangle;

        //SetImageButtonsType(rectangle, ButtonType.DestinationBtn, nodes);

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
        else if (AddNewNodeFlag == true && !IsFlagBelongsToSameNode(imageButton, CurrentImageButton, nodes))
            //if it receives another image button, it links a new road between two crossroads
        {
            Node targetNode = GetNodeFromImageButton(imageButton, nodes);
            Rect rect = absoluteLayout.GetLayoutBounds(targetNode.rectangle);
            Vector destCoords = GetLineCoordsForOrientation((Orientation)imageButton.Rotation, rect);
            Polygon road = DrawRoad(LineCoords, destCoords, (Orientation)imageButton.Rotation, (Orientation)CurrentImageButton.Rotation);     
            Link link = new Link() { road = road }; 
            targetNode.roads.Add(link);
            absoluteLayout.Add(road);

            foreach (Node node in nodes)
            {
                SetImageButtonsVisibility(node.imageButtons, false);
            }
            SetImageButtonsType(ButtonType.RoadPlusBlackButton, nodes);

            absoluteLayout.Remove(CurrentImageButton);
            absoluteLayout.Remove(imageButton);
            AddNewNodeFlag = false;
        }
        else
            //if sending image button belongs to the same crossroad, new image button become starting point
        {
            imageButton.Source = ButtonType.RoadPlusGreenButton;
            Node nodeTarget = GetNodeFromImageButton(imageButton, nodes);
            Orientation orientation = (Orientation)imageButton.Rotation;
            CurrentOrientation = orientation;
            Rect rectangleRect = absoluteLayout.GetLayoutBounds(nodeTarget.rectangle);
            LineCoords = GetLineCoordsForOrientation(orientation, rectangleRect);

            foreach (Node node in nodes)
            {
                if (!node.Equals(nodeTarget))
                {
                    SetImageButtonsVisibility(node.imageButtons, true);
                }
            }
            
            SetImageButtonsType(ButtonType.DestinationButton, nodes, nodeTarget.rectangle);
            CurrentImageButton = imageButton;
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
        else if(AddNewObjectFlag)
        {
            double width = 100, height = 100;
            Rect rectRoadObject = new Rect(tappedPoint.Value.X - width / 2,
               tappedPoint.Value.Y - height / 2, width, height);

            Image image = new Image() { Source = ImageSource, ZIndex = 10 };

            TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += RoadObjectImage_Tapped;
            image.GestureRecognizers.Add(tapGestureRecognizer);

            PanGestureRecognizer panGestureRecognizer = new PanGestureRecognizer();
            panGestureRecognizer.PanUpdated += RoadObjectPanGesture_PanUpdated;
            image.GestureRecognizers.Add(panGestureRecognizer);

            absoluteLayout.Add(image);
            absoluteLayout.SetLayoutBounds(image, rectRoadObject);
            ImageSource = "plus.png";
            CurrentFrame.Background = Brush.Transparent;
            AddNewObjectFlag = false;
            foreach (Node node in nodes)
            {
                node.rectangle.IsEnabled = true;
            }
        }
        else if (nodes.Count == 0) //if there are no crossroads, then it just creates new crossroads without roads.
        {
            rectangle = GetRectangle(rect, Crossroads_Tapped);
            absoluteLayout.Add(rectangle);
            absoluteLayout.SetLayoutBounds(rectangle, rect);
            AddFlagsAroundRectangle(rectangle, GetReversedOrientation(CurrentOrientation), false);
        }
        else if (nodes.Count > 0 || !AddNewNodeFlag) // resetting  image buttons if user clicks on empty area
        {
            Node targetNode = GetNodeFromImageButton(CurrentImageButton, nodes);
            foreach (Node node in nodes)
            {
                SetImageButtonsVisibility(node.imageButtons, false);
            }
            absoluteLayout.Remove(CurrentRotationSlider);
            ToggleSteppersVisibility(links, false);
            CurrentRectangle = new Rectangle();        
        }

        if (AddNewNodeFlag) //if the program in AddNewNode mode, then it creates new crossroad and links a road to it.
        {
            rectangle = GetRectangle(rect, Crossroads_Tapped);
            absoluteLayout.Add(rectangle);
            absoluteLayout.SetLayoutBounds(rectangle, rect);
            Rect rect2 = absoluteLayout.GetLayoutBounds(rectangle);
            Vector destCoords = GetLineCoordsForOrientation(CurrentOrientation, rect, true);
            SetImageButtonsType(ButtonType.RoadPlusBlackButton, nodes);
            AddFlagsAroundRectangle(rectangle, GetReversedOrientation(CurrentOrientation), false);
            Polygon road = DrawRoad(LineCoords, destCoords, CurrentOrientation, CurrentOrientation);
            absoluteLayout.Add(road);
            Node targetNode = GetNodeFromImageButton(CurrentImageButton, nodes);

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
            absoluteLayout.Remove(CurrentImageButton);         
            AddNewNodeFlag = false;
        }
    }

    private void ButtonClean_Clicked(object sender, EventArgs e)
    {
        ResetRoadElements();
        absoluteLayout.Clear();
        //RedrawRoad();
    }

    private void OnStepperValueChanged(object sender, ValueChangedEventArgs e)
    {
        double value = e.NewValue;
        Stepper stepper = (Stepper)sender;
        LineStepper lineStepper = GetLineStepperFromLinks(stepper, links);
        
        Link link = GetLinkFromLineStepper(lineStepper, links);

        DrawLines2(ref link, lineStepper.Vector, (int)value);

    }

    private void RoadObjectFromMenu_Tapped(object sender, EventArgs e)
    {
        Image tappedImage = (Image)sender;
        Frame frame = (Frame)tappedImage.Parent;
        switchIsTrajectoryMode.IsToggled = false;
        crossButton.IsEnabled = false;
        if (ImageSource.Equals(tappedImage.Source))
        {
            frame.Background = Brush.Transparent;
            AddNewObjectFlag = false;
            ImageSource = "plus.png";
        }
        else
        {
            frame.Background = Color.FromHex("#606060");
            AddNewNodeFlag = false;
            AddNewObjectFlag = true;
            CurrentFrame = frame;
            ImageSource = tappedImage.Source;
        }

        foreach (Node node in nodes)
        {
            node.rectangle.IsEnabled = false;
        }

    }

    private void SwitchIsTrajectoryMode_Toggled(object sender, ToggledEventArgs e)
    {
        foreach (Node node in nodes)
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
            Value = 0,
            ZIndex = 13,
            WidthRequest = 150
        };
        slider.ValueChanged += OnRotationSliderValueChanged;
        absoluteLayout.Add(slider);
        Rect rect = absoluteLayout.GetLayoutBounds(roadObject);
        rect.Y -= 20;
        absoluteLayout.SetLayoutBounds(slider, rect);
        CurrentRotationSlider = slider;
        CurrentRoadObject = roadObject;
    }

    void OnRotationSliderValueChanged(object sender, ValueChangedEventArgs args)
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
            Node node = GetNodeFromRectangle(rectangle, nodes);
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
                links.Remove(link);
            }
            absoluteLayout.Remove(node.rectangle);
            nodes.Remove(node);
            CurrentView = null;
        }
        else
        {
            absoluteLayout.Remove(CurrentView);
        }
        crossButton.IsEnabled = false;
    }

    double x, y;
    private void RoadObjectPanGesture_PanUpdated(object sender, PanUpdatedEventArgs e)
    {
        Image roadObject = (Image)sender;
        switch (e.StatusType)
        {
            case GestureStatus.Running:
                x = e.TotalX;
                y = e.TotalY;
                roadObject.TranslationX = x;
                roadObject.TranslationY = y;
                break;

            case GestureStatus.Completed:
                Rect rect2 = absoluteLayout.GetLayoutBounds(roadObject);
                absoluteLayout.SetLayoutBounds(roadObject, new Rect(x + rect2.X,
                    y + rect2.Y, roadObject.Width, roadObject.Height));
                roadObject.TranslationX = 0;
                roadObject.TranslationY = 0;
                break;
        }
    }

}