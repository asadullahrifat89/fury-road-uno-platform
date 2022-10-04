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

            RenderTransform = new SkewTransform() { AngleY = 43 };
            SetContent(new Uri("ms-appx:///Assets/Images/road-mark2.png"));
        }
    }
}

