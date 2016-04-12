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
        private int ColumnAvgDataRange => (int)ColumnDataRangeSlider.Value;
        private float ColumnWidth => (float)(ColumnWidthSlider.Value * ColumnAvgDataRange * 0.01);
        private int DataPointsPerFrame => (int)ValuesPerFrameSlider.Value;
        private int MovingAverageRange1 => (int)MovingAverageRange1Slider.Value;
        private int MovingAverageRange2 => (int)MovingAverageRange2Slider.Value;
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

            if (ShowColumnsCheckBox.IsChecked == true)
                this.RenderAveragesAsColumns(args);
            if (ShowDataCheckBox.IsChecked == true)
                this.RenderData(args, Colors.Black, DataStrokeThickness);
            if (ShowMovingAverage1CheckBox.IsChecked == true)
                this.RenderMovingAverage(args, Colors.DeepSkyBlue, MovingAverageStrokeThickness1, MovingAverageRange1);
            if (ShowMovingAverage2CheckBox.IsChecked == true)
                this.RenderMovingAverage(args, Colors.Red, MovingAverageStrokeThickness2, MovingAverageRange2);
            if (ShowAxesCheckBox.IsChecked == true)
                this.RenderAxes(args);

            canvas.Invalidate();
        }

        private void RenderAxes(CanvasDrawEventArgs args)
        {
            var width = (float)canvas.ActualWidth;
            var height = (float)(canvas.ActualHeight);
            var midWidth = (float)(width * .5);
            var midHeight = (float)(height * .5);

            using (var cpb = new CanvasPathBuilder(args.DrawingSession))
            {
                // Horizontal line
                cpb.BeginFigure(new Vector2(0, midHeight));
                cpb.AddLine(new Vector2(width, midHeight));
                cpb.EndFigure(CanvasFigureLoop.Open);

                // Horizontal line arrow
                cpb.BeginFigure(new Vector2(width - 10, midHeight - 3));
                cpb.AddLine(new Vector2(width, midHeight));
                cpb.AddLine(new Vector2(width - 10, midHeight + 3));
                cpb.EndFigure(CanvasFigureLoop.Open);

                args.DrawingSession.DrawGeometry(CanvasGeometry.CreatePath(cpb), Colors.Gray, 1);
            }

            args.DrawingSession.DrawText("0", 5, midHeight - 30, Colors.Gray);
            args.DrawingSession.DrawText(canvas.ActualWidth.ToString(), width - 50, midHeight - 30, Colors.Gray);

            using (var cpb = new CanvasPathBuilder(args.DrawingSession))
            {
                // Vertical line
                cpb.BeginFigure(new Vector2(midWidth, 0));
                cpb.AddLine(new Vector2(midWidth, height));
                cpb.EndFigure(CanvasFigureLoop.Open);

                // Vertical line arrow
                cpb.BeginFigure(new Vector2(midWidth - 3, 10));
                cpb.AddLine(new Vector2(midWidth, 0));
                cpb.AddLine(new Vector2(midWidth + 3, 10));
                cpb.EndFigure(CanvasFigureLoop.Open);

                args.DrawingSession.DrawGeometry(CanvasGeometry.CreatePath(cpb), Colors.Gray, 1);
            }

            args.DrawingSession.DrawText("0", midWidth + 5, height - 30, Colors.Gray);
            args.DrawingSession.DrawText("1", midWidth + 5, 5, Colors.Gray);
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

                if (ShowDataAsAreaCheckBox.IsChecked == true)
                {
                    cpb.AddLine(new Vector2(_data.Count, (float)canvas.ActualHeight));
                    cpb.AddLine(new Vector2(0, (float)canvas.ActualHeight));
                    cpb.EndFigure(CanvasFigureLoop.Closed);
                    args.DrawingSession.FillGeometry(CanvasGeometry.CreatePath(cpb), Colors.LightGreen);
                }
                else
                {
                    cpb.EndFigure(CanvasFigureLoop.Open);
                    args.DrawingSession.DrawGeometry(CanvasGeometry.CreatePath(cpb), color, thickness);
                }
            }
        }

        private void RenderAveragesAsColumns(CanvasDrawEventArgs args)
        {
            var padding = .5 * (ColumnAvgDataRange - ColumnWidth);
            for (int start = 0; start < _data.Count; start += ColumnAvgDataRange)
            {
                double total = 0;
                var range = Math.Min(ColumnAvgDataRange, _data.Count - start);

                for (int i = start; i < start + range; i++)
                {
                    total += _data[i];
                }

                args.DrawingSession.FillRectangle(start + (float)padding, (float)(canvas.ActualHeight * (1 - total / range)), ColumnWidth, (float)(canvas.ActualHeight * (total / range)), Colors.WhiteSmoke);
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
