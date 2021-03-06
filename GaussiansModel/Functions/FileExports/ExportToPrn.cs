using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GaussiansModel.Functions
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ExportGraphAttribute:Attribute
    {

    }

    [ExportGraph]
    public class ExportToPrn : NodeFunctionBase<ExportToPrn>
    {
        public ExportToPrn()
        {
            Inputs = new List<FunctionParameter>()
            {
                new FunctionParameter(Properties.Resources.InputChartName, typeof(IGraph), new PointGraph()),
                new FunctionParameter(Properties.Resources.InputMinValue, typeof(double), 0.0),
                new FunctionParameter(Properties.Resources.InputMaxValue, typeof(double), 1.0),
                new FunctionParameter(Properties.Resources.InputStep, typeof(double), 0.1),
                new FunctionParameter(Properties.Resources.InputSource, typeof(StreamData), new StreamData(null, FileMode.Create))
            };
        }
        public override string GetName()
        {
            return Properties.Resources.ExportToPrn;
        }

        public override void Invoke()
        {
            IGraph graph = (IGraph)FindInputParameter(Properties.Resources.InputChartName).Value;
            double max = (double)FindInputParameter(Properties.Resources.InputMaxValue).Value;
            double min = (double)FindInputParameter(Properties.Resources.InputMinValue).Value;
            double step = (double)FindInputParameter(Properties.Resources.InputStep).Value;
            StreamData file = (StreamData)FindInputParameter(Properties.Resources.InputSource).Value;

            var points = GetPoints(graph, min, max, step);
            using (StreamWriter sw = new(file.Stream))
            {
                foreach (var item in points)
                {
                    sw.WriteLine(item.X.ToString() + ' ' + item.Y.ToString());
                }
            }
        }
        private static IEnumerable<Point> GetPoints(IGraph graph, double  min, double max, double step)
        {
            if (graph is PointGraph points)
                return points.Where(i => i.X >= min && i.X <= max);
            List<Point> res = new();
            for (double i = min; i < max; i+=step)
                res.Add(new Point(i, graph.GetValue(i)));
            return res;
        }

    }
}
