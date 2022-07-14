using GaussiansModel.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GaussiansModel.Functions
{
    /// <summary>
    /// Аттрибут, который необходимо давать всем классам - реализаторам IFileReader
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class FunctionParameterAttribute : Attribute
    {
    }
    [FunctionParameter]
    public class ExtremumParameter : NodeFunctionBase<ExtremumParameter>
    {
        public ExtremumParameter()
        {
            Inputs = new List<FunctionParameter>()
            {
                new FunctionParameter(Properties.Resources.InputChartName, typeof(IGraph), new PointGraph()),
                new FunctionParameter(Properties.Resources.InputStep, typeof(double), 0.1),
                new FunctionParameter(Properties.Resources.InputStartX, typeof(double), 0.0),
                new FunctionParameter(Properties.Resources.InputEndX, typeof(double), 10.0),
                new FunctionParameter(Properties.Resources.InputMaxError, typeof(double), 0.01),
                new FunctionParameter(Properties.Resources.InputRangeYIgnore, typeof(double), 0.05)
            };
            Outputs = new List<FunctionParameter>()
            {
                new FunctionParameter(Properties.Resources.OutputMinGraph, typeof(PointGraph), new PointGraph()),
                new FunctionParameter(Properties.Resources.OutputMaxGraph, typeof(PointGraph), new PointGraph())
            };
        }
        public override string GetName()
        {
            return Properties.Resources.ExtremumParameterName;
        }

        public override void Invoke()
        {
            IGraph points = (IGraph)FindInputParameter(Properties.Resources.InputChartName).Value;
            double step = (double)FindInputParameter(Properties.Resources.InputStep).Value;
            double maxError = (double)FindInputParameter(Properties.Resources.InputMaxError).Value;
            double range = (double)FindInputParameter(Properties.Resources.InputRangeYIgnore).Value;
            double start = (double)FindInputParameter(Properties.Resources.InputStartX).Value;
            double end = (double)FindInputParameter(Properties.Resources.InputEndX).Value;

            Func<double, double> func = points.GetValue;

            double der = NonlinearSolver.Derivative(func, start, step / 100);
            double nextDer;
            List<(double, double)> rangesMin = new();
            List<(double, double)> rangesMax = new();
            for (double x = start + step; x < end; x += step)
            {
                nextDer = NonlinearSolver.Derivative(func, x, step / 100);
                if (nextDer * der <= 0)
                {
                    if (der > 0)
                        rangesMax.Add((x - step, x));
                    else
                        rangesMin.Add((x - step, x));
                }
                der = nextDer;
            }

            IEnumerable<Point> maxes = rangesMax.Select(i =>
            {
                double x = NonlinearSolver.FindExtremum(func, i.Item1, i.Item2, maxError);
                return new Point(x, func(x));
            }).OrderBy(i => i.X).ToList();
            IEnumerable<Point> mins = rangesMin.Select(i =>
            {
                double x = NonlinearSolver.FindExtremum(func, i.Item1, i.Item2, maxError);
                return new Point(x, func(x));
            }).OrderBy(i => i.X).ToList();
            var enMax = maxes.GetEnumerator();
            var enMin = mins.GetEnumerator();
            if (enMax.MoveNext() && enMin.MoveNext())
            {
                Point oldMin = enMin.Current, oldMax = enMax.Current;
                List<Point> removedMin = new();
                List<Point> removedMax = new();
                var currentEnum = oldMax.X > oldMin.X ? enMin : enMax;
                while (currentEnum.MoveNext())
                {
                    Point cur = currentEnum.Current;
                    if (oldMax.X > oldMin.X)
                    {
                        if (Math.Abs(oldMin.Y - oldMax.Y) <= range && Math.Abs(cur.Y - oldMax.Y) <= range)
                            removedMax.Add(oldMax);
                        oldMin = cur;
                        currentEnum = enMax;
                    }
                    else
                    {
                        if (Math.Abs(oldMax.Y - oldMin.Y) >= range && Math.Abs(cur.Y - oldMin.Y) >= range)
                            removedMin.Add(oldMin);
                        oldMax = cur;
                        currentEnum = enMin;
                    }
                }
                maxes = maxes.Except(removedMax);
                mins = mins.Except(removedMin);
                maxes.Count();
                mins.Count();
            }

            SetOutputParameter(Properties.Resources.OutputMaxGraph, new PointGraph(maxes));
            SetOutputParameter(Properties.Resources.OutputMinGraph, new PointGraph(mins));
        }
    }
}
