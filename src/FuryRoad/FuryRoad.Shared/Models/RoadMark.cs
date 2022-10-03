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

            SetContent(new Uri("ms-appx:///Assets/Images/road-dash2.png"));
        }
    }

    public class RoadSide : GameObject
    {
        public RoadSide()
        {
            Tag = Constants.ROADSIDE_TAG;
        }
    }
}

