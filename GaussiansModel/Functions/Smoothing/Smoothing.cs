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
            Inputs.Add(new(Properties.Resources.InputChartName, typeof(PointGraph), new PointGraph()));
            Inputs.Add(new(Properties.Resources.InputValueName, typeof(int), 200));

            Outputs.Add(new(Properties.Resources.OutputGraph, typeof(PointGraph), new PointGraph()));
        }

        public override string GetName() => Properties.Resources.SmoothingByAverageFunctionName;

        public override void Invoke(CancellationToken token)
        {
            int length = (int)FindInputParameter(Properties.Resources.InputValueName).Value;
            PointGraph graph = (PointGraph)FindInputParameter(Properties.Resources.InputChartName).Value;
            IEnumerator<Point> en = graph.GetEnumerator();
            if (!en.MoveNext())
            {
                SetOutputParameter(Properties.Resources.OutputGraph, null);
                return;
            }
            Progress = 30;
            if (token.IsCancellationRequested)
            {
                en.Dispose();
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
            Progress += 30;
            if (token.IsCancellationRequested)
            {
                en.Dispose();
                return;
            }

            res.Add(GetAvegare(queue));
            while (en.MoveNext())
            {
                queue.Enqueue(en.Current);
                queue.Dequeue();
                res.Add(GetAvegare(queue));
            }
            Progress = 100;
            en.Dispose();
            if (token.IsCancellationRequested)
                return;
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
            Inputs.Add(new FunctionParameter(Properties.Resources.InputDistanceY, typeof(double), 0.5));
            Inputs.Add(new FunctionParameter(Properties.Resources.InputDistanceX, typeof(double), 0.5));
            Inputs.Add(new FunctionParameter(Properties.Resources.InputChartName, typeof(PointGraph), new PointGraph()));

            Outputs.Add(new FunctionParameter(Properties.Resources.OutputGraph, typeof(PointGraph), new PointGraph()));
            Outputs.Add(new FunctionParameter("ggwp", typeof(int), 2));
        }
        public override string GetName()
        {
            return Properties.Resources.SmoothingByDistanceFunctionName;
        }

        public override void Invoke(CancellationToken token)
        {
            PointGraph points = (PointGraph)FindInputParameter(Properties.Resources.InputChartName).Value;
            double disX = (double)FindInputParameter(Properties.Resources.InputDistanceX).Value;
            double disY = (double)FindInputParameter(Properties.Resources.InputDistanceY).Value;

            PointGraph res = new();

            var enumerator = points.GetEnumerator();
            Progress = 0;
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
            Progress = 100;
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
}
