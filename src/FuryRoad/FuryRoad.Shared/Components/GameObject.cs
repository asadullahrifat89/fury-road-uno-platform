﻿using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Text;
using Windows.Foundation;

namespace FuryRoad
{
    public class GameObject : Border
    {
        #region Fields

        private Image _content = new Image() { Stretch = Stretch.Uniform };

        //private Border _hitBoxborder;

        #endregion

        #region Ctor

        public GameObject()
        {
            //TODO: remove these

            #region HitBox Debug
            //BorderThickness = new Microsoft.UI.Xaml.Thickness(1);
            //BorderBrush = new SolidColorBrush(Colors.Black);

            //_hitBoxborder = new Border()
            //{
            //    BorderThickness = new Microsoft.UI.Xaml.Thickness(1),
            //    BorderBrush = new SolidColorBrush(Colors.Black)
            //};


            //var grid = new Grid()
            //{
            //    HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
            //    VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center
            //};

            //grid.Children.Add(_hitBoxborder);
            //grid.Children.Add(_content);
            //Child = grid; 

            #endregion

            Child = _content;
            //BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
            //BorderBrush = new SolidColorBrush(Colors.Transparent);

            RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);
        }

        #endregion

        #region Properties

        public double Speed { get; set; }

        public bool IsVehicle { get; set; }

        public bool IsRoadMark { get; set; }

        #endregion

        #region Methods

        public void SetSize(double width, double height)
        {
            Width = width;
            Height = height;
        }

        public double GetTop()
        {
            return Canvas.GetTop(this);
        }

        public double GetLeft()
        {
            return Canvas.GetLeft(this);
        }

        public void SetTop(double top)
        {
            Canvas.SetTop(this, top);
        }

        public void SetLeft(double left)
        {
            Canvas.SetLeft(this, left);
        }

        public void SetPosition(double top, double left)
        {
            Canvas.SetTop(this, top);
            Canvas.SetLeft(this, left);
        }

        public void SetContent(Uri uri)
        {
            _content.Source = new BitmapImage(uri);
        }

        //public void SetHitBoxBorder(Rect rect)
        //{
        //    _hitBoxborder.Height = rect.Height;
        //    _hitBoxborder.Width = rect.Width;
        //}

        #endregion
    }
}

