using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.Foundation;

namespace FuryRoad
{
    public static class GameObjectExtensions
    {
        #region Methods      

        /// <summary>
        /// Checks if a two rects intersect.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool IntersectsWith(this Rect source, Rect target)
        {
            var targetX = target.X;
            var targetY = target.Y;
            var sourceX = source.X;
            var sourceY = source.Y;

            var sourceWidth = source.Width;
            var sourceHeight = source.Height;

            var targetWidth = target.Width;
            var targetHeight = target.Height;

            if (source.Width >= 0.0 && target.Width >= 0.0
                && targetX <= sourceX + sourceWidth && targetX + targetWidth >= sourceX
                && targetY <= sourceY + sourceHeight)
            {
                return targetY + targetHeight >= sourceY;
            }

            return false;
        }

        public static Rect GetHitBox(this GameObject gameObject, double scale)
        {
            return new Rect(
                x: gameObject.GetLeft() + 5* scale,
                y: gameObject.GetTop() + 10* scale,
                width: gameObject.Width - 5* scale,
                height: gameObject.Height - 10* scale);
        }

        public static Rect GetDistantHitBox(this GameObject gameObject, double scale)
        {
            return new Rect(
                x: gameObject.GetLeft() - 5* scale,
                y: gameObject.GetTop() - 50* scale,
                width: gameObject.Width + 5 * scale,
                height: gameObject.Height + 50 * scale);
        }

        #endregion
    }
}
