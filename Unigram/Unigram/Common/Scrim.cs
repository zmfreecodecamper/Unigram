﻿using System;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Unigram.Common
{
    public static class Scrim
    {
        public static CubicBezierGradient GetGradient(DependencyObject obj)
        {
            return (CubicBezierGradient)obj.GetValue(GradientProperty);
        }

        public static void SetGradient(DependencyObject obj, CubicBezierGradient value)
        {
            obj.SetValue(GradientProperty, value);
        }

        public static readonly DependencyProperty GradientProperty =
            DependencyProperty.RegisterAttached("Gradient", typeof(CubicBezierGradient), typeof(LinearGradientBrush), new PropertyMetadata(null, OnGradientChanged));

        private static void OnGradientChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var linear = d as LinearGradientBrush;
            if (linear == null)
            {
                return;
            }

            linear.GradientStops.Clear();

            var gradient = e.NewValue as CubicBezierGradient;
            if (gradient == null)
            {
                return;
            }

            foreach (var stop in gradient.GetGradientStops())
            {
                linear.GradientStops.Add(stop);
            }
        }
    }

    public class CubicBezierGradient
    {
        public Color TopColor { get; set; }
        public Color BottomColor { get; set; }

        public Point ControlPoint1 { get; set; } = new Point(.42, 0);
        public Point ControlPoint2 { get; set; } = new Point(.58, 1);

        public GradientStop[] GetGradientStops()
        {
            return GetGradientStops(new[] { TopColor, BottomColor }, GetCoordinates(ControlPoint1, ControlPoint2));
        }

        private GradientStop[] GetGradientStops(Color[] colors, Point[] coordinates)
        {
            var colorStops = new GradientStop[coordinates.Length];

            for (int i = 0; i < coordinates.Length; i++)
            {
                colorStops[i] = new GradientStop
                {
                    Color = Mix(colors[0], colors[1], coordinates[i].Y),
                    Offset = coordinates[i].X
                };
            }

            return colorStops;
        }

        private Color Mix(Color x, Color y, double amount)
        {
            static double Mix(double x, double y, double f)
            {
                return Math.Sqrt(Math.Pow(x, 2) * (1 - f) + Math.Pow(y, 2) * f);
            }

            return Color.FromArgb(
                (byte)Mix(x.A, y.A, amount),
                (byte)Mix(x.R, y.R, amount),
                (byte)Mix(x.G, y.G, amount),
                (byte)Mix(x.B, y.B, amount));
        }

        private Point[] GetCoordinates(Point p1, Point p2, int polySteps = 15)
        {
            var increment = 1d / polySteps;
            var coordinates = new Point[polySteps + 1];
            var index = 0;

            for (double i = 0; i <= 1; i += increment)
            {
                coordinates[index++] = new Point(GetBezier(i, p1.X, p2.X), GetBezier(i, p1.Y, p2.Y));
            }

            return coordinates;
        }

        private double GetBezier(double t, double n1, double n2)
        {
            return Math.Round(
              (1 - t) * (1 - t) * (1 - t) * 0 +
              3 * ((1 - t) * (1 - t)) * t * n1 +
              3 * (1 - t) * (t * t) * n2 +
              t * t * t * 1, 10);
        }
    }
}
