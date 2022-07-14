using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace InteractiveDataDisplay.WPF
{
    public interface IIsPointCollection
    {
        public PointCollection Points { get; set; }
    }
    public class PointLine : FrameworkElement, IIsPointCollection
    {

        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register("Points", typeof(PointCollection), typeof(PointLine),
                new FrameworkPropertyMetadata(new PointCollection(), FrameworkPropertyMetadataOptions.AffectsRender));
        public PointCollection Points
        {
            get { return (PointCollection)GetValue(PointsProperty); }
            set { SetValue(PointsProperty, value); }
        }

        public static readonly DependencyProperty RadiusEllipseProperty = DependencyProperty.Register("RadiusEllipse", typeof(double), typeof(PointLine), new PropertyMetadata(4.0));
        public double RadiusEllipse
        {
            get { return (double)GetValue(RadiusEllipseProperty); }
            set { SetValue(RadiusEllipseProperty, value); }
        }

        public static readonly DependencyProperty BrushEllipseProperty = DependencyProperty.Register("BrushEllipse", typeof(Brush), typeof(PointLine),
            new FrameworkPropertyMetadata(new SolidColorBrush(Colors.Black), FrameworkPropertyMetadataOptions.AffectsRender));
        public Brush BrushEllipse
        {
            get { return (Brush)GetValue(BrushEllipseProperty); }
            set { SetValue(BrushEllipseProperty, value); }
        }

        public static readonly DependencyProperty SelectionPointsProperty =
            DependencyProperty.Register("SelectionPoints", typeof(IPointsSkript),
            typeof(PointLine), new FrameworkPropertyMetadata(new ViewAllPoints(), FrameworkPropertyMetadataOptions.AffectsRender));
        public IPointsSkript SelectionPoints
        {
            get { return (IPointsSkript)GetValue(SelectionPointsProperty); }
            set { SetValue(SelectionPointsProperty, value); }
        }


        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            Size size = RenderSize;
            List<Point> points = new List<Point>();
            foreach (var point in Points)
            {
                if (point.X < 0 || point.Y < 0 || point.X > size.Width || point.Y > size.Height)
                    continue;
                points.Add(point);
            }
            IEnumerable<Point> points2 = SelectionPoints.OnSkript(points);
            foreach (var point in points2)
            {
                drawingContext.DrawEllipse(BrushEllipse, new Pen(), point, RadiusEllipse, RadiusEllipse);
            }
        }
    }
}
