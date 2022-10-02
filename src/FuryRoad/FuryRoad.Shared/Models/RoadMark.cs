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

