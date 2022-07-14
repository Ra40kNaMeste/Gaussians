using GaussiansModel.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GaussiansModel.Functions
{

    //Семейство функций сглаживания
    [AttributeUsage(AttributeTargets.Class)]
    public class SmoothingFinctionAttribute : Attribute
    {
    }
    //Функция сглаживания по среднему числу
    [SmoothingFinction]
    public class SmoothingByAverageValue : NodeFunctionBase<SmoothingByAverageValue>
    {
        public SmoothingByAverageValue()
        {
            Inputs = new List<FunctionParameter>()
            {
                new(Properties.Resources.InputChartName, typeof(PointGraph), new PointGraph()),
                new(Properties.Resources.InputValueName, typeof(int), 200)
            };
            Outputs = new List<FunctionParameter>()
            {
                new(Properties.Resources.OutputGraph, typeof(IGraph), new PointGraph())
            };
        }

        public override string GetName() => Properties.Resources.SmoothingByAverageFunctionName;

        public override void Invoke()
        {
            int length = (int)FindInputParameter(Properties.Resources.InputValueName).Value;
            PointGraph graph = (PointGraph)FindInputParameter(Properties.Resources.InputChartName).Value;
            IEnumerator<Point> en = graph.GetEnumerator();
            if (!en.MoveNext())
            {
                SetOutputParameter(Properties.Resources.OutputGraph, null);
                return;
            }
            PointGraph res = new();
            Queue<Point> queue = new();
            int i = 0;
            while (i < length)
            {
                queue.Enqueue(en.Current);
                if (!en.MoveNext())
                {
                    SetOutputParameter(Properties.Resources.OutputGraph, null);
                    return;
                }
                i++;
            }
            res.Add(GetAvegare(queue));
            while (en.MoveNext())
            {
                queue.Enqueue(en.Current);
                queue.Dequeue();
                res.Add(GetAvegare(queue));
            }
            en.Dispose();
            SetOutputParameter(Properties.Resources.OutputGraph, res);
            return;
        }
        private static Point GetAvegare(Queue<Point> points)
        {
            double x = (points.Peek().X + points.Last().X) / 2, y = 0;
            int count = points.Count;
            foreach (var item in points)
            {
                y += item.Y;
            }
            return new Point(x, y / count);
        }
    }

    [SmoothingFinction]
    public class SmoothingByDistanceValue : NodeFunctionBase<SmoothingByDistanceValue>
    {
        public SmoothingByDistanceValue()
        {
            Inputs = new List<FunctionParameter>()
            {
                new FunctionParameter(Properties.Resources.InputDistanceY, typeof(double), 0.5),
                new FunctionParameter(Properties.Resources.InputDistanceX, typeof(double), 0.5),
                new FunctionParameter(Properties.Resources.InputChartName, typeof(PointGraph), new PointGraph())
            };
            Outputs = new List<FunctionParameter>()
            {
               new FunctionParameter(Properties.Resources.OutputGraph, typeof(IGraph), new PointGraph())
            };
        }
        public override string GetName()
        {
            return Properties.Resources.SmoothingByDistanceFunctionName;
        }

        public override void Invoke()
        {
            PointGraph points = (PointGraph)FindInputParameter(Properties.Resources.InputChartName).Value;
            double disX = (double)FindInputParameter(Properties.Resources.InputDistanceX).Value;
            double disY = (double)FindInputParameter(Properties.Resources.InputDistanceY).Value;

            PointGraph res = new();

            var enumerator = points.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Point? point = FindNextPoint(enumerator, disX, disY);
                if (point == null)
                {
                    enumerator.Dispose();
                    SetOutputParameter(Properties.Resources.OutputGraph, res);
                    return;
                }
                res.Add((Point)point);
            }
            enumerator.Dispose();
            SetOutputParameter(Properties.Resources.OutputGraph, res);
            return;
        }
        private Point? FindNextPoint(IEnumerator<Point> enumerator, double distanceX, double distanceY)
        {
            Point point = enumerator.Current;
            Point firstPoint = point;
            List<Point> points = new();
            points.Add(point);
            while (MathPoint.Distance(new Point(firstPoint.X / distanceX, firstPoint.Y / distanceY), new Point(point.X / distanceX, point.Y / distanceY)) <= 1
                && enumerator.MoveNext())
            {
                point = enumerator.Current;
                points.Add(point);
            }
            return MathPoint.CenterPoint(points.ToArray());
        }
    }
    [SmoothingFinction]
    internal class SmoothingForSplineApproximation : NodeFunctionBase<SmoothingForSplineApproximation>
    {
        public SmoothingForSplineApproximation()
        {
            Inputs = new List<FunctionParameter>()
            {
                new FunctionParameter(Properties.Resources.InputChartName, typeof(PointGraph), new PointGraph()),
                new FunctionParameter(Properties.Resources.InputMaxtanValue, typeof(double), 0.01),
                new FunctionParameter(Properties.Resources.InputNoizePoint, typeof(int), 2)
            };
            Outputs = new List<FunctionParameter>()
            {
                new FunctionParameter(Properties.Resources.OutputGraph, typeof(PointGraph), new PointGraph())
            };
        }
        public override string GetName()
        {
            return Properties.Resources.SmoothingForSplineApprozimation;
        }

        public override void Invoke()
        {
            PointGraph points = (PointGraph)FindInputParameter(Properties.Resources.InputChartName).Value;
            int noize = (int)FindInputParameter(Properties.Resources.InputNoizePoint).Value;

            PointGraph res = new();

            double maxAngle = (double)FindInputParameter(Properties.Resources.InputMaxtanValue).Value;

            var en = points.GetEnumerator();
            if(!en.MoveNext())
                return;
            Point point = en.Current;
            res.Add(point);
            if (!en.MoveNext())
                return;
            Point nextPoint = en.Current;
            res.Add(nextPoint);
            double curAngle = Math.Atan(MathPoint.Tan(point, nextPoint));
            double deltaTan = 0;
            int steps = 0;

            while (en.MoveNext())
            {
                if(IsNoize(nextPoint, maxAngle, curAngle, noize, en, ref steps))
                {
                    for (int i = 0; i < steps; i++)
                        en.MoveNext();
                }
                double nextTan = MathPoint.Tan(nextPoint, en.Current);

                 double nextDeltaTan = nextTan - curAngle;
                if(nextDeltaTan*deltaTan<0)
                    res.Add(nextPoint);

                point = nextPoint;
                nextPoint = en.Current;
                curAngle = nextTan;
                deltaTan = nextDeltaTan;
            }
            SetOutputParameter(Properties.Resources.OutputGraph, res);
        }
        private bool IsNoize(Point startPoint, double maxAngle, double curAngle, int noizeCount, IEnumerator<Point> points, ref int steps)
        {
            Point point = points.Current;
            double angle = Math.Atan(MathPoint.Tan(startPoint, point));
            if(Math.Abs(angle-curAngle)>maxAngle)
            {
                steps = 0;
                return true;
            }
            for (int i = 0; i < noizeCount; i++)
            {
                if(!points.MoveNext())
                    return false;
                point = points.Current;
                angle = Math.Atan(MathPoint.Tan(startPoint, point));
                if (Math.Abs(angle - curAngle) > maxAngle)
                {
                    steps = i + 1;
                    return true;
                }
            }
            return false;
        }
    }

    [SmoothingFinction]
    internal class SplittingGraph : NodeFunctionBase<SplittingGraph>
    {
        public SplittingGraph()
        {
            Inputs = new List<FunctionParameter>()
            {
                new FunctionParameter(Properties.Resources.InputChartName, typeof(PointGraph), new PointGraph()),
                new FunctionParameter(Properties.Resources.InputMaxtanValue, typeof(double), 0.01),
                new FunctionParameter(Properties.Resources.InputUncorrectPointsValue, typeof(int), 1)
            };
            Outputs = new List<FunctionParameter>()
            {
                new FunctionParameter(Properties.Resources.OutputGraphs, typeof(PointGraph), new PointGraph())
            };
        }
        public override string GetName()
        {
            return Properties.Resources.SmoothingSplitingGraph;
        }

        public override void Invoke()
        {
            PointGraph points = (PointGraph)FindInputParameter(Properties.Resources.InputChartName).Value;
            double maxTan = (double)FindInputParameter(Properties.Resources.InputMaxtanValue).Value;
            int maxUncorrectPoints = (int)FindInputParameter(Properties.Resources.InputUncorrectPointsValue).Value + 1;

            int uncorrectPoints = 0;
            var en = points.GetEnumerator();
            if (!en.MoveNext())
                return;
            List<Point> res = new();
            Point startPoint = en.Current, cur, old;
            res.Add(startPoint);
            while (en.MoveNext())
            {
                cur = en.Current;
                if (Math.Abs(Math.Atan(MathPoint.Tan(startPoint, cur))) > maxTan)
                {
                    old = cur;
                    uncorrectPoints++;
                    if (uncorrectPoints > maxUncorrectPoints)
                    {
                        res.Add(old);
                        uncorrectPoints = 0;
                    }
                }
                else
                    uncorrectPoints = 0;

            }

            SetOutputParameter(Properties.Resources.OutputGraphs, new PointGraph(res));

        }
    }

}
