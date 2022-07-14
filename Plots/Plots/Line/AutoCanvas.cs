using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace InteractiveDataDisplay.WPF
{
    //internal class PolylineWithPointPanel : Panel
    //{
    //    public static readonly DependencyProperty PointsProperty =
    //        DependencyProperty.Register("Points", typeof(IEnumerable<Point>), typeof(PolylineWithPoints),
    //        new FrameworkPropertyMetadata(new List<Point>(), FrameworkPropertyMetadataOptions.AffectsMeasure));
    //    public IEnumerable<Point> Points
    //    {
    //        get { return (IEnumerable<Point>)GetValue(PointsProperty); }
    //        set { SetValue(PointsProperty, value); }
    //    }
    //}

    public class AutoCanvas : Panel
    {
        public static DependencyProperty PointProperty =
            DependencyProperty.RegisterAttached("Point", typeof(Point), typeof(AutoCanvas),
            new FrameworkPropertyMetadata(new Point(), FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static Point GetPoint(DependencyObject obj)
        {
            return (Point)(obj.GetValue(PointProperty));
        }
        public static void SetPoint(DependencyObject obj, Point val) => obj.SetValue(PointProperty, val);
        protected override Size MeasureOverride(Size availableSize)
        {
            Point maxPoint = new(), minPoint = new();
            foreach (UIElement item in Children)
            {
                item.Measure(new(double.PositiveInfinity, double.PositiveInfinity));
                Point point = GetPoint(item);
                maxPoint = MaxPoint(maxPoint, new(point.X + item.DesiredSize.Width / 2, point.Y + item.DesiredSize.Height / 2));
                minPoint = MinPoint(minPoint, new(point.X + item.DesiredSize.Width / 2, point.Y + item.DesiredSize.Height / 2));

            }
            return new(Math.Min(maxPoint.X - minPoint.X, availableSize.Width), Math.Min(maxPoint.Y - minPoint.Y, availableSize.Height));
        }
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            Point maxPoint = new(), minPoint = new();
            foreach (UIElement item in Children)
            {
                Point point = GetPoint(item);
                item.Arrange(GetRectWithArrageChild(item.DesiredSize, point));
                maxPoint = MaxPoint(maxPoint, new(point.X + item.RenderSize.Width / 2, point.Y + item.RenderSize.Height / 2));
                minPoint = MinPoint(minPoint, new(point.X + item.RenderSize.Width / 2, point.Y + item.RenderSize.Height / 2));

            }
            return new(Math.Min(maxPoint.X - minPoint.X, arrangeSize.Width), Math.Min(maxPoint.Y - minPoint.Y, arrangeSize.Height));
        }
        private static Rect GetRectWithArrageChild(Size designeSize, Point point)
        {
            return new Rect(new Point(point.X - designeSize.Width / 2, point.Y - designeSize.Width / 2),
                new Point(point.X + designeSize.Width / 2, point.Y + designeSize.Width / 2));
        }
        private static Point MaxPoint(Point point1, Point point2)
        {
            return new Point(Math.Max(point1.X, point2.X), Math.Max(point1.Y, point2.Y));
        }
        private static Point MinPoint(Point point1, Point point2)
        {
            return new Point(Math.Min(point1.X, point2.X), Math.Min(point1.Y, point2.Y));
        }
    }
}
