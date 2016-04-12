using Microsoft.Graphics.Canvas.Geometry;
using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.UI;
using Windows.UI.Xaml.Controls;

namespace Win2DChartSample
{
    public sealed partial class MainPage : Page
    {
        private const float StrokeThickness = 2;
        private const int DataPointsPerFrame = 10;
        private readonly List<double> _data = new List<double>();
        private readonly Random _rand = new Random();
        private double _lastValue = 0.5;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void Canvas_OnDraw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            for (int i = 0; i < DataPointsPerFrame; i++)
            {
                var delta = _rand.NextDouble() * .1 - .05;
                this._lastValue = Math.Max(0d, Math.Min(1d, _lastValue + delta));
                _data.Add(this._lastValue);
            }

            if (_data.Count > (int)canvas.ActualWidth)
            {
                _data.RemoveRange(0, _data.Count - (int)canvas.ActualWidth);
            }

            args.DrawingSession.Clear(Colors.White);

            using (var cpb = new CanvasPathBuilder(args.DrawingSession))
            {
                cpb.BeginFigure(new Vector2(0, (float)(canvas.ActualHeight * (1 - _data[0]))));

                for (int i = 1; i < _data.Count; i++)
                {
                    cpb.AddLine(new Vector2(i, (float)(canvas.ActualHeight * (1 - _data[i]))));
                }

                cpb.EndFigure(CanvasFigureLoop.Open);

                args.DrawingSession.DrawGeometry(CanvasGeometry.CreatePath(cpb), Colors.Black, StrokeThickness);
            }

            canvas.Invalidate();
        }
    }
}
