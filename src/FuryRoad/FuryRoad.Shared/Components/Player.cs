using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace FuryRoad.Models
{
    public class Player : GameObject
    {
        public Player()
        {
            Tag = Constants.PLAYER_TAG;

            SetContent(new Uri("ms-appx:///Assets/Images/player.png"));
        }
    }
}
