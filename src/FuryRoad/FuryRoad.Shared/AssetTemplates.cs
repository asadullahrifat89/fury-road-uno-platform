﻿using System;
using System.Collections.Generic;
using System.Text;

namespace FuryRoad
{
    public static class AssetTemplates
    {
        public static Uri[] CAR_TEMPLATES = new Uri[] 
        {
            new Uri("ms-appx:///Assets/Images/car1.png"),
            new Uri("ms-appx:///Assets/Images/car2.png"),
            new Uri("ms-appx:///Assets/Images/car3.png"),
            new Uri("ms-appx:///Assets/Images/car4.png"),
            new Uri("ms-appx:///Assets/Images/car5.png"),
            new Uri("ms-appx:///Assets/Images/car6.png")
        };

        public static Uri[] TRUCK_TEMPLATES = new Uri[]
        {
            new Uri("ms-appx:///Assets/Images/truck1.png"),
            new Uri("ms-appx:///Assets/Images/truck2.png"),
            new Uri("ms-appx:///Assets/Images/truck3.png"),
            new Uri("ms-appx:///Assets/Images/truck4.png"),
        };

        public static Uri[] ROADMARK_TEMPLATES = new Uri[]
        {
            new Uri("ms-appx:///Assets/Images/road-mark1.png"),
            new Uri("ms-appx:///Assets/Images/road-mark2.png"),
            new Uri("ms-appx:///Assets/Images/road-mark3.png"),
        };
    }
}
