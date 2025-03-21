﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Timeline.Controls
{
    public class TimelineKey : ContentControl
    {
        public double PlacementPosition
        {
            get => (double)GetValue(PlacementPositionProperty);
            set => SetValue(PlacementPositionProperty, value);
        }
        public static readonly DependencyProperty PlacementPositionProperty =
            DependencyProperty.Register(nameof(PlacementPosition), typeof(double), typeof(TimelineKey),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PlacementPositionPropertyChanged));

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(TimelineKey),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, IsSelectedPropertyChanged));
        public bool IsHover
        {
            get => (bool)GetValue(IsHoverProperty);
            set => SetValue(IsHoverProperty, value);
        }
        public static readonly DependencyProperty IsHoverProperty =
            DependencyProperty.Register(nameof(IsHover), typeof(bool), typeof(TimelineKey),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, IsHoverPropertyChanged));
        internal EventHandler<MouseButtonEventArgs>? PreMouseLeftButtonDown { get; set; }
        internal EventHandler<MouseButtonEventArgs>? PreMouseLeftButtonUp { get; set; }
        
        internal EventHandler<MouseEventArgs>? PreMouseEnter { get; set; }
        
        internal EventHandler<MouseEventArgs>? PreMouseLeave { get; set; }
        internal EventHandler<TimelineKey>? SelectionChangedEvent { get; set; }

        TranslateTransform Translate { get; }

        public TimelineKey(object content)
        {
            DataContext = content;

            Translate = new TranslateTransform();
            Translate.X = PlacementPosition;
            Translate.Y = 0;

            var group = new TransformGroup();
            group.Children.Add(Translate);

            RenderTransform = group;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            // Ctrl+Clickによる超シビアなタイミングでClickCount==2で処理が来る場合がある
            // なので、ClickCountを見て処理を限定する実装はしないように。
            PreMouseLeftButtonDown?.Invoke(this, e);

            e.Handled = true;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            PreMouseLeftButtonUp?.Invoke(this, e);

            // Lane側でHandleする必要がある処理が行われたので、ここでは処理しない
            if (e.Handled == false)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    IsSelected = !IsSelected;
                }
                else
                {
                    IsSelected = true;
                }

                e.Handled = true;
            }
        }
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            PreMouseEnter?.Invoke(this,e);
            if (!e.Handled)
            {
                IsHover = true;
            }
        }
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            PreMouseLeave?.Invoke(this,e);
            if (!e.Handled)
            {
                IsHover = false;
            }
        }
        static void PlacementPositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var key = (TimelineKey)d;
            key.Translate.X = Math.Max(0, (double)e.NewValue);
        }

        static void IsSelectedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var key = (TimelineKey)d;
            key.SelectionChangedEvent?.Invoke(key, key);
        }
        static void IsHoverPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var key = (TimelineKey)d;
        }
    }
}
