using System;
using System.Collections.Generic;
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
    public class PointGraph : LineGraph
    {
        protected PointLine pointLine = new();
        public PointGraph() : base()
        {
            this.PointChanged += PointGraph_PointChanged;

            BindingOperations.SetBinding(pointLine, PointLine.RadiusEllipseProperty, new Binding("RadiusPoint") { Source = this });
            BindingOperations.SetBinding(pointLine, PointLine.BrushEllipseProperty, new Binding("Stroke") { Source = this });
            BindingOperations.SetBinding(pointLine, PointLine.SelectionPointsProperty, new Binding("SelectionPoints") { Source = this });

            Children.Add(pointLine);
        }

        public static readonly DependencyProperty RadiusPointProperty =
            DependencyProperty.Register("RadiusPoint",
            typeof(double),
            typeof(LineGraph),
            new PropertyMetadata(5.0));
        public double RadiusPoint
        {
            get
            {
                return (double)GetValue(RadiusPointProperty);
            }
            set
            {
                SetValue(RadiusPointProperty, value);
            }
        }

        public static readonly DependencyProperty SelectionPointsProperty =
            DependencyProperty.Register("SelectionPoints", typeof(IPointsSkript),
            typeof(PointGraph), new FrameworkPropertyMetadata(new ViewAllPoints()));
        public IPointsSkript SelectionPoints
        {
            get { return (IPointsSkript)GetValue(SelectionPointsProperty); }
            set { SetValue(SelectionPointsProperty, value); }
        }

        public static readonly DependencyProperty VisualModeProperty =
            DependencyProperty.Register("VisualMode", typeof(PointGraphState), typeof(PointGraph), new PropertyMetadata(PointGraphState.PointsAndLine, OnVisualModeChangedCallback));

        private static void OnVisualModeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PointGraph graph)
            {
                graph.Children.Clear();
                PointGraphState state = (PointGraphState)e.NewValue;
                if ((state & PointGraphState.OnlyPoints) != 0)
                    graph.Children.Add(graph.pointLine);
                if ((state & PointGraphState.OnlyLine) != 0)
                    graph.Children.Add(graph.polyline);

            }
        }

        public PointGraphState VisualMode
        {
            get { return (PointGraphState)GetValue(VisualModeProperty); }
            set { SetValue(VisualModeProperty, value); }
        }



        private void PointGraph_PointChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PointGraph linePlot = (PointGraph)d;
            if (linePlot != null)
            {
                SetPoints(linePlot.pointLine, (PointCollection)e.NewValue);
            }
        }
    }
    public enum PointGraphState
    {
        OnlyPoints = 1,
        OnlyLine = 2,
        PointsAndLine = 3,
    }
}
