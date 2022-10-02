using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Text;

namespace FuryRoad
{
    public class GameObject : Border
    {
        public GameObject()
        {
            //TODO: remove these
            BorderThickness = new Microsoft.UI.Xaml.Thickness(1);
            BorderBrush = new SolidColorBrush(Colors.Black);
        }

        public double Speed { get; set; }

        public bool IsVehicle { get; set; }

        public bool IsRoadMark { get; set; }
    }
}

