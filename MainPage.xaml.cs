using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Win2DChartSample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private List<double> data = new List<double>();
        private Random rand = new Random();
        private double lastValue = 0.5;
        private CanvasSolidColorBrush brush;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void Canvas_OnDraw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            if (this.brush == null)
            {
                this.brush = new CanvasSolidColorBrush(
                    args.DrawingSession,
                    Colors.Black);
            }

            for (int i = 0; i < 5; i++)
            {
                var delta = rand.NextDouble() * .1 - .05;
                this.lastValue = Math.Max(0d, Math.Min(1d, lastValue + delta));
                data.Add(this.lastValue);
            }

            if (data.Count > (int)canvas.ActualWidth)
            {
                data.RemoveRange(0, data.Count - (int)canvas.ActualWidth);
            }

            args.DrawingSession.Clear(Colors.White);

            using (var cpb = new CanvasPathBuilder(args.DrawingSession))
            {
                cpb.BeginFigure(new Vector2(0, (float)(canvas.ActualHeight * (1 - data[0]))));

                for (int i = 1; i < data.Count; i++)
                {
                    cpb.AddLine(new Vector2(i, (float)(canvas.ActualHeight * (1 - data[i]))));
                }

                cpb.EndFigure(CanvasFigureLoop.Open);

                args.DrawingSession.DrawGeometry(CanvasGeometry.CreatePath(cpb), Colors.Black, 1);
            }

            canvas.Invalidate();
        }
    }
}
