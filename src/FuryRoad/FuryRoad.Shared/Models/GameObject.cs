using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Text;

namespace FuryRoad
{
    public class GameObject : Rectangle
    {
        public double Speed { get; set; }

        public bool IsVehicle { get; set; }

        public bool IsRoadMark { get; set; }
    }

    public class Car : GameObject
    {
        public Car()
        {
            Tag = "car";
        }
    }

    public class Truck : GameObject
    {
        public Truck()
        {
            Tag = "truck";
        }
    }

    public class RoadMark : GameObject
    {
        public RoadMark()
        {
            Tag = "roadMarks";
            Fill = new ImageBrush() { ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/road-dash1.png")) };
        }
    }
}

