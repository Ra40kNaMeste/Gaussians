using GaussiansModel.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GaussiansModel.Functions
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ApproximationFunctionAttribute:Attribute
    {

    }


    [ApproximationFunction]
    internal class SplineApproximation : NodeFunctionBase<SplineApproximation>
    {
        public SplineApproximation()
        {
            Inputs = new List<FunctionParameter>()
            {
                new FunctionParameter(Properties.Resources.InputSkeleton, typeof(PointGraph), new PointGraph()),
                new FunctionParameter(Properties.Resources.InputChartName, typeof(PointGraph), new PointGraph()),
                //new FunctionParameter(Properties.Resources.InputCountInflection, typeof(int), 2),
                new FunctionParameter(Properties.Resources.InputMaxSteps, typeof(int), 60),
                new FunctionParameter(Properties.Resources.InputMaxAngle, typeof(double), 1.0)
            };
            Outputs = new List<FunctionParameter>()
            {
                new FunctionParameter(Properties.Resources.OutputGraph, typeof(MultiGraph), new MultiGraph())
            };
        }
        public override string GetName()
        {
            return Properties.Resources.SplineApproximationName;
        }

        public override void Invoke()
        {
            PointGraph skeleton = (PointGraph)FindInputParameter(Properties.Resources.InputSkeleton).Value;
            PointGraph pointGraphs = (PointGraph)FindInputParameter(Properties.Resources.InputChartName).Value;
            int maxSteps = (int)FindInputParameter(Properties.Resources.InputMaxSteps).Value;
            double maxAngle = (double)FindInputParameter(Properties.Resources.InputMaxAngle).Value;
            int inflection = 3;

            int i = 0;
            List<(PointGraph, PointGraph)> beziers = new();
            List<Point> skeletPoint = new();
            IEnumerator<Point> enPoints = pointGraphs.GetEnumerator();
            PointGraph points = new();

            foreach (var item in skeleton)
            {
                if (i == 0)
                {
                    while (enPoints.Current.X < item.X && enPoints.MoveNext()) { }
                }
                skeletPoint.Add(item);
                i++;
                if (i >= inflection)
                {
                    while (enPoints.MoveNext() && enPoints.Current.X < item.X)
                    {
                        points.Add(enPoints.Current);
                    }


                    beziers.Add((new(skeletPoint), points));
                    points = new();
                    skeletPoint = new();
                    skeletPoint.Add(item);
                    i = 1;
                }

            }

            var splines = beziers.Select(i => new
            {
                Bezier = MinimizeBeziers(i.Item1, i.Item2, maxAngle, maxSteps),
                XMin = i.Item2.ElementAt(0),
                XMax = i.Item2.ElementAt(i.Item2.Count() - 1)
            });

            MultiGraph res = new();
            res.Graphs = splines.AsParallel().Select(i => new MultiGraphItem(i.XMin.X, i.XMax.X, new FuncGraph() { Func = i.Bezier.GetPoint })).ToList();
            SetOutputParameter(Properties.Resources.OutputGraph, res);
        }
        private static BeziersSpline MinimizeBeziers(PointGraph beziersPoints, PointGraph points, double maxAngle, int maxSteps)
        {
            if (beziersPoints.Count < 3)
                return new BeziersSpline(beziersPoints);
            Point start = beziersPoints.ElementAt(0), end = beziersPoints.ElementAt(beziersPoints.Count - 1);
            double firstStep = MathPoint.Distance(start, end) / 20;
            double stepChangedStep = 0.6;
            BeziersSpline beziers = new(beziersPoints);
            int size = beziersPoints.Count - 1;
            Vector startPoints = ConvertPointsToVector(beziersPoints);

            Vector res = NonlinearSolver.Minimize((v) =>
            {
                beziers.SetValuesByPoints(ConvertVectorToPoint(start, end, v));
                return FunctionCollection.GetErrorValue(beziers.GetPoint, points);
            }, startPoints, firstStep, stepChangedStep, maxAngle, maxSteps);

            beziers.SetValuesByPoints(ConvertVectorToPoint(start, end, res));
            return beziers;
        }
        private static Vector ConvertPointsToVector(IEnumerable<Point> points)
        {
            IEnumerator<Point> en = points.GetEnumerator();
            if (!en.MoveNext())
                return new Vector(0);
            if (!en.MoveNext())
                return new Vector(0);

            Vector vector = new((points.Count() - 2) * 2);
            int i = 0;

            Point old = en.Current;
            while (en.MoveNext())
            {
                vector.SetValue(2 * i, 0, old.X);
                vector.SetValue(2 * i + 1, 0, old.Y);
                i += 2;
                old = en.Current;
            }
            en.Dispose();
            return vector;
        }
        private static List<Point> ConvertVectorToPoint(Point start, Point end, Vector vector)
        {
            List<Point> res = new();
            res.Add(start);
            int count = vector.Rows / 2;
            for (int i = 0; i < count; i++)
            {
                res.Add(new(vector.GetValue(2 * i, 0), vector.GetValue(2 * i + 1, 0)));
            }
            res.Add(end);
            return res;
        }
    }

    //[ApproximationFunction]
    //internal class Test : NodeFunctionBase<Test>
    //{
    //    public override string GetName()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override void Invoke()
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
