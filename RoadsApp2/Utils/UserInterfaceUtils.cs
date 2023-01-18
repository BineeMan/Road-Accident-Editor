using Microsoft.Maui.Controls.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RoadsApp2.Utils.Structs;

namespace RoadsApp2.Utils
{
    internal class UserInterfaceUtils
    {
        public static void ToggleImageButtons(List<ImageButton> imgButtons)
        {
            foreach (ImageButton imageButton in imgButtons)
            {
                imageButton.IsVisible = !imageButton.IsVisible;
            }
        }

        public static Rectangle GetRectangle(Rect rect, EventHandler<TappedEventArgs> targetEvent = null)
        {
            Rectangle rectangle = new Rectangle()
            {
                Fill = Color.FromRgb(137, 137, 137),
                HeightRequest = rect.Width,
                WidthRequest = rect.Height,
                Stroke = Brush.Red,
                StrokeThickness = 0,
            };
            if (targetEvent != null)
            {
                TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.Tapped += targetEvent;
                rectangle.GestureRecognizers.Add(tapGestureRecognizer);
            }
            return rectangle;
        }

        public static Node GetNodeFromRectangle(Rectangle rectangle, List<Node> nodes)
        {
            foreach (Node node in nodes)
            {
                if (node.rectangle.Equals(rectangle))
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
                if (node.rectangle.Equals(rectangle))
                {
                    return true;
                }
            }
            return false;
        }

        private void AddPlusButton(int x, int y, double rotation)
        {
            ImageButton imageButton = new ImageButton
            {
                Source = "plus.png",
                WidthRequest = 50,
                Rotation = rotation,
                ZIndex = 99,
                AnchorY = 1
            };

            //absoluteLayout.Add(imageButton);
            //absoluteLayout.SetLayoutBounds(imageButton, new Rect(x, y, imageButton.Width, imageButton.Height));

            //imageButton.Clicked += imgButton_Clicked;
        }
    }
}
