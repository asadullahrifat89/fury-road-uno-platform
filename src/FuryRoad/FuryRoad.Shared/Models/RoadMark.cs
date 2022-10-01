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
            Fill = new ImageBrush() { ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/road-dash1.png")) };
        }
    }
}

