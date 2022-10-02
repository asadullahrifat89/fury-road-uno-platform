using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;

namespace FuryRoad
{
    public class RoadMark : GameObject
    {
        public RoadMark()
        {
            Tag = Constants.ROADMARK_TAG;
            Child = new Image() { Source = new BitmapImage(new Uri("ms-appx:///Assets/road-dash1.png")), Stretch = Stretch.Fill };
        }
    }

    public class RoadSide : GameObject
    {
        public RoadSide()
        {
            Tag = Constants.ROADSIDE_TAG;
            Child = new Image() { Source = new BitmapImage(new Uri("ms-appx:///Assets/road-side.png")), Stretch = Stretch.Fill };
        }
    }
}

