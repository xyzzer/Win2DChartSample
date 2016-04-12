using Microsoft.Graphics.Canvas.Geometry;
using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace Win2DChartSample
{
    public sealed partial class MainPage : Page
    {
        private const float DataStrokeThickness = 1;
        private const float MovingAverageStrokeThickness1 = 2;
        private const float MovingAverageStrokeThickness2 = 2;
        private const int DataPointsPerFrame = 10;
        private const int MovingAverageRange1 = 50;
        private const int MovingAverageRange2 = 150;
        private readonly List<double> _data = new List<double>();
        private readonly Random _rand = new Random();
        private double _lastValue = 0.5;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void Canvas_OnDraw(CanvasControl sender, CanvasDrawEventArgs args)
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
            this.RenderData(args, Colors.Black, DataStrokeThickness);
            this.RenderMovingAverage(args, Colors.DeepSkyBlue, MovingAverageStrokeThickness1, MovingAverageRange1);
            this.RenderMovingAverage(args, Colors.Red, MovingAverageStrokeThickness2, MovingAverageRange2);

            canvas.Invalidate();
        }

        private void RenderData(CanvasDrawEventArgs args, Color color, float thickness)
        {
            using (var cpb = new CanvasPathBuilder(args.DrawingSession))
            {
                cpb.BeginFigure(new Vector2(0, (float)(canvas.ActualHeight * (1 - _data[0]))));

                for (int i = 1; i < _data.Count; i++)
                {
                    cpb.AddLine(new Vector2(i, (float)(canvas.ActualHeight * (1 - _data[i]))));
                }

                cpb.EndFigure(CanvasFigureLoop.Open);

                args.DrawingSession.DrawGeometry(CanvasGeometry.CreatePath(cpb), color, thickness);
            }
        }

        private void RenderMovingAverage(CanvasDrawEventArgs args, Color color, float thickness, int movingAverageRange)
        {
            using (var cpb = new CanvasPathBuilder(args.DrawingSession))
            {
                cpb.BeginFigure(new Vector2(0, (float)(canvas.ActualHeight * (1 - _data[0]))));

                double total = _data[0];

                int previousRangeLeft = 0;
                int previousRangeRight = 0;

                for (int i = 1; i < _data.Count; i++)
                {
                    var range = Math.Max(0, Math.Min(movingAverageRange / 2, Math.Min(i, _data.Count - 1 - i)));
                    int rangeLeft = i - range;
                    int rangeRight = i + range;

                    for (int j = previousRangeLeft; j < rangeLeft; j++)
                    {
                        total -= _data[j];
                    }

                    for (int j = previousRangeRight + 1; j <= rangeRight; j++)
                    {
                        total += _data[j];
                    }

                    previousRangeLeft = rangeLeft;
                    previousRangeRight = rangeRight;

                    cpb.AddLine(new Vector2(i, (float)(canvas.ActualHeight * (1 - total / (range * 2 + 1)))));
                }

                cpb.EndFigure(CanvasFigureLoop.Open);

                args.DrawingSession.DrawGeometry(CanvasGeometry.CreatePath(cpb), color, thickness);
            }
        }
    }
}
