using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Text;

namespace FuryRoad
{
    public class GameObject : Border
    {
        private Image _content = new Image() { Stretch = Microsoft.UI.Xaml.Media.Stretch.Fill };

        public GameObject()
        {
            //TODO: remove these
            //BorderThickness = new Microsoft.UI.Xaml.Thickness(1);
            //BorderBrush = new SolidColorBrush(Colors.Black);

            //BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
            //BorderBrush = new SolidColorBrush(Colors.Transparent);
            Child = _content;
        }

        public double Speed { get; set; }

        public bool IsVehicle { get; set; }

        public bool IsRoadMark { get; set; }

        public void SetContent(Uri uri) 
        {
            _content.Source = new BitmapImage(uri);
        }
    }
}

