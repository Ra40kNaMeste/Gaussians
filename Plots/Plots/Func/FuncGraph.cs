using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace InteractiveDataDisplay.WPF
{
    /// <summary>
    /// A plot to draw simple line by function
    /// </summary>
    public class FuncGraph : LineGraph
    {
        public FuncGraph()
        {

        }

        public static readonly DependencyProperty TargetFunctionProperty = DependencyProperty.Register("TargetFunction", typeof(Func<double, double>),
            typeof(FuncGraph), new PropertyMetadata(new Func<double, double>(i=>i), UpdateGraphs));

        /// <summary>
        /// Gets or sets line graph points.
        /// </summary>
        public Func<double, double> TargetFunction
        {
            get { return (Func<double, double>)GetValue(TargetFunctionProperty); }
            set { SetValue(TargetFunctionProperty, value); }
        }


        public static readonly DependencyProperty MinProperty = DependencyProperty.Register("Min", typeof(double), typeof(FuncGraph), new FrameworkPropertyMetadata(-10.0, FrameworkPropertyMetadataOptions.AffectsRender, UpdateGraphs));
        public double Min 
        {
            get { return (double)GetValue(MinProperty); }
            set { SetValue(MinProperty, value); }
        }

        public static readonly DependencyProperty MaxProperty = DependencyProperty.Register("Max", typeof(double), typeof(FuncGraph), new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsRender, UpdateGraphs));
        public double Max
        {
            get { return (double)GetValue(MaxProperty); }
            set { SetValue(MaxProperty, value); }
        }

        public static readonly DependencyProperty VolumePointsProperty = DependencyProperty.Register("VolumePoints", typeof(int), typeof(FuncGraph), new FrameworkPropertyMetadata(1000, FrameworkPropertyMetadataOptions.AffectsRender, UpdateGraphs));
        public int VolumePoints 
        { 
            get { return (int)GetValue(VolumePointsProperty); }
            set { SetValue(VolumePointsProperty, value); }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="FuncGraph"/> class.
        /// </summary>

        public void Update()
        {
            UpdateGraphs(this, new());
        }
        private static void UpdateGraphs(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Plot(d, GetXPoints((FuncGraph)d));
        }

        private static IEnumerable<double> GetXPoints(FuncGraph func)
        {
            List<double> res = new();
            double min = func.Min, max = func.Max;
            int number = func.VolumePoints;
            double step = (max - min) / number;
            for (double x = min; x < max; x+=step)
                res.Add(x);
            return res;
        }
        /// <summary>
        /// Updates data in <see cref="Points"/> and causes a redrawing of line graph.
        /// </summary>
        /// <param name="x">A set of x coordinates of new points.</param>
        private static void Plot(DependencyObject sender, IEnumerable<double> x)
        {
            FuncGraph graph = (FuncGraph)sender;
            if(x == null)
                throw new ArgumentNullException("x");
            List<double> y = new List<double>();
            Func<double, double> targetFunc = graph.TargetFunction;
            PointCollection points = new();
            foreach (var curX in x)
                try
                {
                    points.Add(new(curX, targetFunc(curX)));
                }
                catch (Exception) { }

            graph.Points = points;
        }
    }

}
