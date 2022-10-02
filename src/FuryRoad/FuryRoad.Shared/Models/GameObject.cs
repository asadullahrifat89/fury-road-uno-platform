﻿using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Shapes;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Text;

namespace FuryRoad
{
    public class GameObject : Border
    {
        public double Speed { get; set; }

        public bool IsVehicle { get; set; }

        public bool IsRoadMark { get; set; }
    }
}

