using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace InteractiveDataDisplay.WPF
{
    public interface IPointsSkript
    {
        public IEnumerable<Point> OnSkript(IEnumerable<Point> points);
    }
    public class ViewAllPoints : IPointsSkript
    {
        public IEnumerable<Point> OnSkript(IEnumerable<Point> points) => points;
    }
    public class DeleteNoisePotins : IPointsSkript
    {
        public DeleteNoisePotins()
        {
            Variation = 0.5;
        }
        public DeleteNoisePotins(double variation)
        {
            Variation = variation;
        }

        public double Variation { get; set; }
        public IEnumerable<Point> OnSkript(IEnumerable<Point> points)
        {
            Point point = points.ElementAt(0);
            List<Point> res = new();
            foreach (var item in points)
            {
                if(GetDistance(point, item) > Variation)
                {
                    res.Add(item);
                    point = item;
                }
            }
            return res;
        }
        private static double GetDistance(Point point1, Point point2)
        {
            double x = point1.X - point2.X;
            double y = point1.Y - point2.Y;
            return Math.Sqrt(x*x + y*y);
        }
    }

    public class DotReductionToMaxCountPoints : IPointsSkript
    {
        public DotReductionToMaxCountPoints(int maxCount) => MaxCount = maxCount;
        public IEnumerable<Point> OnSkript(IEnumerable<Point> points)
        {
            int count = points.Count();
            if (count <= MaxCount)
                return points;
            double t = (double)MaxCount / (count - MaxCount);
            int skip = (int)Math.Ceiling(t);
            int delete = (int)Math.Ceiling(1 / t);
            bool isSkip = true;
            int step = 0;
            List<Point> res = new();
            foreach (var item in points) 
            {
                if (isSkip)
                {
                    if(step >= skip)
                    {
                        step = 0;
                        isSkip = false;
                        continue;
                    }
                    res.Add(item);
                }
                else if(step >= delete)
                {
                    step = 0;
                    isSkip = true;
                    continue;
                }
                step++;
            }
            return res;
        }
        private int MaxCount { get; set; }
    }

    public class DeleteAngleDeviationPoints : IPointsSkript
    {
        public DeleteAngleDeviationPoints()
        {
            Variation = 0.01;
        }
        public DeleteAngleDeviationPoints(double variation)
        {
            Variation = variation;
        }

        public double Variation { get; set; }
        public IEnumerable<Point> OnSkript(IEnumerable<Point> points)
        {
            List<Point> res = new();
            using (var en = points.GetEnumerator())
            {
                if (!en.MoveNext())
                    return points;
                Point point = en.Current;
                res.Add(point);
                if (!en.MoveNext())
                    return points;
                Point newPoint = en.Current;
                double tan = GetTan(point, newPoint);
                double newTan;
                while (en.MoveNext())
                {
                    point = newPoint;
                    newPoint = en.Current;
                    newTan = GetTan(point, newPoint);
                    if (Math.Abs(newTan - tan) > Variation)
                    {
                        if (!res.Contains(point))
                            res.Add(point);
                        res.Add(newPoint);
                        tan = newTan;
                    }
                }
                res.Add(newPoint);
            }
            return res;
        }
        private static double GetTan(Point point, Point point2)
        {
            return Math.Tan((point2.Y - point.Y) / (point2.X / point.X));
        }
    }

    public class MultiDeletePoints : IPointsSkript
    {
        public MultiDeletePoints()
        {
            Skripts = new List<IPointsSkript>();
        }
        public MultiDeletePoints(IEnumerable<IPointsSkript> skripts)
        {
            Skripts = skripts;
        }
        public MultiDeletePoints(params IPointsSkript[] values)
        {
            List<IPointsSkript> skripts = new();
            foreach (var item in values)
            {
                skripts.Add(item);
            }
            Skripts = skripts;
        }

        public IEnumerable<IPointsSkript> Skripts { get; set; }
        public virtual IEnumerable<Point> OnSkript(IEnumerable<Point> points)
        {
            foreach (var skript in Skripts)
            {
                points = skript.OnSkript(points);
            }
            return points;
        }
    }

    public class MultiDeletePointsWithShowAll:MultiDeletePoints
    {
        public MultiDeletePointsWithShowAll() : base() { MaxPointsForShowAll = 100; }
        public MultiDeletePointsWithShowAll(IEnumerable<IPointsSkript> skripts) : base(skripts) { MaxPointsForShowAll = 100; }
        public MultiDeletePointsWithShowAll(params IPointsSkript[] skripts) : base(skripts) { MaxPointsForShowAll = 100; }
        public int MaxPointsForShowAll { get; set; }
        public override IEnumerable<Point> OnSkript(IEnumerable<Point> points)
        {
            return points.Count() > MaxPointsForShowAll ? base.OnSkript(points) : points;
        }
    }
}
