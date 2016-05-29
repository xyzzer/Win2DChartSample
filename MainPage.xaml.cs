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
        private readonly ChartRenderer _chartRenderer;

        public MainPage()
        {
            this.InitializeComponent();
            _chartRenderer = new ChartRenderer();
        }

        private void Canvas_OnDraw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            for (int i = 0; i < DataPointsPerFrame; i++)
            {
                var delta = _rand.NextDouble() * .1 - .05;
                _lastValue = Math.Max(0d, Math.Min(1d, _lastValue + delta));
                _data.Add(_lastValue);
            }

            if (_data.Count > (int)canvas.ActualWidth)
            {
                _data.RemoveRange(0, _data.Count - (int)canvas.ActualWidth);
            }

            args.DrawingSession.Clear(Colors.White);

            if (RenderAsPieChartCheckBox.IsChecked != true)
            { 
            if (ShowColumnsCheckBox.IsChecked == true)
                _chartRenderer.RenderAveragesAsColumns(canvas, args, ColumnAvgDataRange, ColumnWidth, _data);
            if (ShowDataCheckBox.IsChecked == true)
                _chartRenderer.RenderData(canvas, args, Colors.Black, DataStrokeThickness, _data, renderArea: ShowDataAsAreaCheckBox.IsChecked == true);
            if (ShowMovingAverage1CheckBox.IsChecked == true)
                _chartRenderer.RenderMovingAverage(canvas, args, Colors.DeepSkyBlue, MovingAverageStrokeThickness1, MovingAverageRange1, _data);
            if (ShowMovingAverage2CheckBox.IsChecked == true)
                _chartRenderer.RenderMovingAverage(canvas, args, Colors.Red, MovingAverageStrokeThickness2, this.MovingAverageRange2, _data);
            if (ShowAxesCheckBox.IsChecked == true)
                _chartRenderer.RenderAxes(canvas, args);
            }
            else
            {
                var pieValues = new List<double>(_data.Count / ColumnAvgDataRange);

                for (int start = 0; start < _data.Count; start += ColumnAvgDataRange)
                {
                    double rangeTotal = 0;
                    var range = Math.Min(ColumnAvgDataRange, _data.Count - start);

                    for (int i = start; i < start + range; i++)
                    {
                        rangeTotal += _data[i];
                    }

                    pieValues.Add(rangeTotal);
                }


                _chartRenderer.RenderAveragesAsPieChart(canvas, args, pieValues, new List<Color>(new [] {Colors.DarkSalmon, Colors.Azure, Colors.LemonChiffon, Colors.Honeydew, Colors.Pink}));
            }

            canvas.Invalidate();
        }
    }
}
