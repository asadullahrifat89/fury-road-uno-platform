using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace FuryRoad.Components
{
    public class GameEnvironment : Canvas
    {
        public GameEnvironment()
        {
            RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);
        }
    }
}
