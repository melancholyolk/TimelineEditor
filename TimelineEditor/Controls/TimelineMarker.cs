﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Timeline.Controls
{
    internal class TimelineMarker : ContentControl
    {
        public double CurrentPosition
        {
            get => (double)GetValue(CurrentPositionProperty);
            set => SetValue(CurrentPositionProperty, value);
        }
        public static readonly DependencyProperty CurrentPositionProperty =
            DependencyProperty.Register(nameof(CurrentPosition), typeof(double), typeof(TimelineMarker), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender, CurrentPositionPropertyChanged));

        public Brush LineColor
        {
            get => (Brush)GetValue(LineColorProperty);
            set => SetValue(LineColorProperty, value);
        }
        public static readonly DependencyProperty LineColorProperty =
            DependencyProperty.Register(nameof(LineColor), typeof(Brush), typeof(TimelineMarker), new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender, LineColorPropertyChanged));

        public Brush BackColor
        {
            get => (Brush)GetValue(BackColorProperty);
            set => SetValue(BackColorProperty, value);
        }
        public static readonly DependencyProperty BackColorProperty =
            DependencyProperty.Register(nameof(BackColor), typeof(Brush), typeof(TimelineMarker), new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromArgb(40,0,0,0)), FrameworkPropertyMetadataOptions.AffectsRender, BackColorPropertyChanged));

        public double BackWidth
        {
            get => (double)GetValue(BackWidthProperty);
            set => SetValue(BackWidthProperty, value);
        }
        public static readonly DependencyProperty BackWidthProperty =
            DependencyProperty.Register(nameof(BackWidth), typeof(double), typeof(TimelineMarker), new FrameworkPropertyMetadata(DefaultBackWidth, FrameworkPropertyMetadataOptions.AffectsRender));

        public bool Snap
        {
            get => (bool)GetValue(SnapProperty);
            set => SetValue(SnapProperty, value);
        }
        public static readonly DependencyProperty SnapProperty =
            DependencyProperty.Register(nameof(Snap), typeof(bool), typeof(TimelineMarker), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));
        
        public TimelineRuler TimelineRuler
        {
            get => (TimelineRuler)GetValue(TimelineRulerProperty);
            set => SetValue(TimelineRulerProperty, value);
        }
        public static readonly DependencyProperty TimelineRulerProperty =
            DependencyProperty.Register(nameof(TimelineRuler), typeof(TimelineRuler), typeof(TimelineMarker), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        public uint SnappedPosition { get;private set; }

        internal EventHandler<double>? MarkerPositionChanged;

        Pen? _LinePen = null;
        Brush? _BackColor = null;
        const double DefaultBackWidth = 17;


        static TimelineMarker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TimelineMarker), new FrameworkPropertyMetadata(typeof(TimelineMarker)));
        }

        public TimelineMarker()
        {

        }

        internal void UpdatePosition(double pos)
        {
            if (Snap)
            {
                pos = TimelineRuler.FindClosestSubHeaderPosition(pos);
                SnappedPosition = TimelineRuler.GetPositionFrame(pos);
            }
            CurrentPosition = Math.Max(0, pos);
        }
        internal void UpdatePosition(uint frameCount)
        {
            SnappedPosition = frameCount;
            CurrentPosition = TimelineRuler.FindClosestSubHeaderPosition(frameCount) - TimelineRuler.Offset.X;
        }
        internal void UpdatePositionFromValue(double value)
        {
            CurrentPosition = value;
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            
            UpdatePen();
            UpdateBackColor();

            // 親のGridの高さを取得
            var parentCanvas = (Canvas)Parent;
            var parentActualHeight = parentCanvas.ActualHeight;

            dc.DrawRectangle(_BackColor, null, new Rect(CurrentPosition - BackWidth * 0.5 + TimelineRuler.Offset.X, 0, BackWidth, parentActualHeight));

            dc.DrawLine(_LinePen, new Point(CurrentPosition + TimelineRuler.Offset.X, 0), new Point(CurrentPosition + TimelineRuler.Offset.X, parentActualHeight));
        }

        void UpdatePen(bool remake = false)
        {
            if (_LinePen == null || remake)
            {
                _LinePen = new Pen(LineColor, 1);
                _LinePen.Freeze();
            }
        }

        void UpdateBackColor(bool remake = false)
        {
            if (_BackColor == null || remake)
            {
                _BackColor = BackColor.Clone();
                _BackColor.Freeze();
            }
        }

        static void CurrentPositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = (TimelineMarker)d;
            editor.MarkerPositionChanged?.Invoke(editor, (double)e.NewValue);
        }

        static void LineColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TimelineMarker)d).UpdatePen(true);
        }

        static void BackColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TimelineMarker)d).UpdateBackColor(true);
        }
    }
}
