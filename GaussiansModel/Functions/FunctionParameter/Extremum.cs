using GaussiansModel.Extension;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    internal enum ExtremumId
    {
        Min = 1,
        Max = 2,
        None = 3
    }

    [FunctionParameter]
    public class ExtremumParameter : NodeFunctionBase<ExtremumParameter>
    {
        public ExtremumParameter()
        {
            Inputs.Add(new FunctionParameter(Properties.Resources.InputChartName, typeof(IGraph), new PointGraph()));
            Inputs.Add(new FunctionParameter(Properties.Resources.InputStep, typeof(double), 0.1));
            Inputs.Add(new FunctionParameter(Properties.Resources.InputStartX, typeof(double), 0.0));
            Inputs.Add(new FunctionParameter(Properties.Resources.InputEndX, typeof(double), 10.0));
            Inputs.Add(new FunctionParameter(Properties.Resources.InputMaxError, typeof(double), 0.01));
            Inputs.Add(new FunctionParameter(Properties.Resources.InputRangeYIgnore, typeof(double), 0.05));

            Outputs.Add(new FunctionParameter(Properties.Resources.OutputMinGraph, typeof(PointGraph), new PointGraph()));
            Outputs.Add(new FunctionParameter(Properties.Resources.OutputMaxGraph, typeof(PointGraph), new PointGraph()));
        }
        public override string GetName()
        {
            return Properties.Resources.ExtremumParameterName;
        }

        public override void Invoke(CancellationToken token)
        {
            IGraph points = (IGraph)FindInputParameter(Properties.Resources.InputChartName).Value;
            double step = (double)FindInputParameter(Properties.Resources.InputStep).Value;
            double maxError = (double)FindInputParameter(Properties.Resources.InputMaxError).Value;
            double range = (double)FindInputParameter(Properties.Resources.InputRangeYIgnore).Value;
            double start = (double)FindInputParameter(Properties.Resources.InputStartX).Value;
            double end = (double)FindInputParameter(Properties.Resources.InputEndX).Value;

            IEnumerable<Point>? mins, maxes;
            (maxes, mins) = GetExtremumsFinder(points).FindExtremums(points, start, end, step, maxError, token, i => Progress = i);
            if (token.IsCancellationRequested || mins == null || maxes == null)
                return;

            var extremums = maxes.Union(mins).OrderBy(i => i.X);

            var enExtremums = extremums.GetEnumerator();

            List<Point> newMines = new();
            List<Point> newMaxes = new();

            ExtremumId lastExtremumId = ExtremumId.None;
            List<Point> tempExtremums = new();
            Point oldExtremum;
            Point curExtremum;
            if (!enExtremums.MoveNext())
                return;
            curExtremum = enExtremums.Current;
            oldExtremum = curExtremum;
            while (enExtremums.MoveNext())
            {
                curExtremum = enExtremums.Current;
                tempExtremums.Add(oldExtremum);
                if (Math.Abs((curExtremum = enExtremums.Current).Y - oldExtremum.Y)<=range)
                {
                    tempExtremums.Add(curExtremum);
                    continue;
                }
                oldExtremum = curExtremum;
                if (maxes.Contains(oldExtremum) && (lastExtremumId&ExtremumId.Min)!=0)
                {
                    newMines.Add(MathPoint.CenterPoint(tempExtremums));
                    lastExtremumId = ExtremumId.Max;
                }
                else if (mins.Contains(oldExtremum) && (lastExtremumId & ExtremumId.Max) != 0)
                {
                    newMaxes.Add(MathPoint.CenterPoint(tempExtremums));
                    lastExtremumId = ExtremumId.Min;
                }
                tempExtremums = new();
                tempExtremums.Add(curExtremum);
            }
            Progress += 10;
            SetOutputParameter(Properties.Resources.OutputMaxGraph, new PointGraph(newMaxes));
            SetOutputParameter(Properties.Resources.OutputMinGraph, new PointGraph(newMines));
        }

        private static IExtremumsFinder GetExtremumsFinder(IGraph graph)
        {
            return ExtremumFinders.Where(i =>
            {
                Type keyType = i.Key;
                Type graphType = graph.GetType();
                return keyType.IsAssignableFrom(graphType) || keyType == graphType || graphType.IsSubclassOf(keyType);
            }).First().Value;

        }

        private static readonly Dictionary<Type, IExtremumsFinder> ExtremumFinders = new()
        {
            { typeof(PointGraph), new PointGraphExtremumsFinder() },
            { typeof(IGraph), new StandartGraphExtremumsFinder() }
        };
    }

    internal interface IExtremumsFinder
    {
        /// <summary>
        /// Возвращает найденные максимумы и минимумы
        /// </summary>
        /// <param name="graph">Исходный график</param>
        /// <param name="start">Начало поиска</param>
        /// <param name="end">Конец поиска</param>
        /// <param name="step">Шаг</param>
        /// <param name="maxError">Допустимая ошибка</param>
        /// <param name="token">Токен отмены</param>
        /// <param name="progress">Прогресс выполнения от 0 до 80</param>
        /// <returns>Максимумы и минимумы; null - операция отменена</returns>
        public (IEnumerable<Point>?, IEnumerable<Point>?) FindExtremums(IGraph graph,  double start, double end, double step, double maxError, CancellationToken token, Action<double> SetProgress);
    }
    internal class StandartGraphExtremumsFinder : IExtremumsFinder
    {
        public (IEnumerable<Point>?, IEnumerable<Point>?) FindExtremums(IGraph graph, double start, double end, double step, double maxError, CancellationToken token, Action<double> SetProgress)
        {
            Func<double, double> func = graph.GetValue;

            double der = NonlinearSolver.Derivative(func, start, maxError / 100);
            double nextDer;
            List<(double, double)> rangesMin = new();
            List<(double, double)> rangesMax = new();
            double startZero = start;

            while (der == 0)
            {
                start += step;
                der = NonlinearSolver.Derivative(func, start, maxError / 100);
            }
            double progress = 0;
            double progressStep = 50 / ((end - start) / step);
            for (double x = start; x < end; x += step)
            {
                nextDer = NonlinearSolver.Derivative(func, x, maxError / 100);
                if (nextDer == 0)
                    continue;
                if (nextDer * der < 0)
                {
                    if (nextDer > 0)
                        rangesMin.Add((startZero - step, x + step));
                    else
                        rangesMax.Add((startZero - step, x + step));
                    progress = (x - start) / step * progressStep;
                    SetProgress?.Invoke(progress);
                }
                startZero = x;
                der = nextDer;
                if (token.IsCancellationRequested)
                    return (null, null);
            }

            IEnumerable<Point> FindExtremum(IEnumerable<(double, double)> ranges) => ranges.Select(i =>
            {
                double x = NonlinearSolver.FindExtremum(func, i.Item1, i.Item2, maxError);
                return new Point(x, func(x));
            }).OrderBy(i => i.X).ToList();

            IEnumerable<Point> maxes = FindExtremum(rangesMax);
            if (token.IsCancellationRequested)
                return (null, null);
            IEnumerable<Point> mins = FindExtremum(rangesMin);
            progress = 80;
            SetProgress?.Invoke(progress);
            return (maxes, mins);
        }
    }

    internal class PointGraphExtremumsFinder : IExtremumsFinder
    {
        public (IEnumerable<Point>?, IEnumerable<Point>?) FindExtremums(IGraph graph, double start, double end, double step, double maxError, CancellationToken token, Action<double> SetProgress)
        {
            PointGraph points = (PointGraph)graph;
            List<Point> maxes = new();
            List<Point> mins = new();
            if (points.Count == 0)
                return (null, null);
            var en = points.GetEnumerator();
            while (en.MoveNext() && en.Current.X < start) { }

            double delta = 0;

            Point oldPoint = en.Current;

            if (!en.MoveNext())
                return (null, null);

            List<Point> SkipPoints()
            {
                List<Point> res = new();
                res.Add(oldPoint);
                delta = en.Current.Y - oldPoint.Y;
                while (delta == 0 && en.MoveNext())
                {
                    delta = en.Current.Y - oldPoint.Y;
                    res.Add(en.Current);
                }
                return res;
            }

            SkipPoints();
            oldPoint = en.Current;
            double oldDelta = delta;

            while (en.MoveNext() && en.Current.X < end)
            {
                List<Point> temp = SkipPoints();
                if (delta * oldDelta < 0)
                {
                    if (delta > 0)
                        mins.Add(MathPoint.CenterPoint(temp));
                    else
                        maxes.Add(MathPoint.CenterPoint(temp));

                }
                oldPoint = en.Current;
                oldDelta = delta;
                if (token.IsCancellationRequested)
                    return (null, null);
            }
            SetProgress(80);
            return (maxes, mins);
        }
    }
}
