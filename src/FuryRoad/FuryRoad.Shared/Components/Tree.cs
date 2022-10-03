using System;

namespace FuryRoad
{
    public class Tree : GameObject
    {
        public Tree()
        {
            Tag = Constants.TREE_TAG;

            SetContent(new Uri("ms-appx:///Assets/Images/tree2.png"));
        }
    }
}

