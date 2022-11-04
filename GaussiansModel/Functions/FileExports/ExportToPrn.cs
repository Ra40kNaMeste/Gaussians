using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GaussiansModel.Functions
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ExportGraphAttribute : Attribute
    {

    }

    [ExportGraph]
    public class ExportToPrn : NodeFunctionBase<ExportToPrn>
    {
        public ExportToPrn()
        {
            Inputs.Add(new FunctionParameter(Properties.Resources.InputChartName, typeof(IGraph), new PointGraph()));
            Inputs.Add(new FunctionParameter(Properties.Resources.InputMinValue, typeof(double), 0.0));
            Inputs.Add(new FunctionParameter(Properties.Resources.InputMaxValue, typeof(double), 1.0));
            Inputs.Add(new FunctionParameter(Properties.Resources.InputStep, typeof(double), 0.1));
            Inputs.Add(new FunctionParameter(Properties.Resources.InputFolder, typeof(StreamData), new StreamData(null, FileMode.Create)));
            Inputs.Add(new FunctionParameter(Properties.Resources.InputPath, typeof(string), Properties.Resources.DefaultInputPrnPath + ".prn"));
        }
        public override string GetName()
        {
            return Properties.Resources.ExportToPrn;
        }

        public override void Invoke(CancellationToken token)
        {
            IGraph graph = (IGraph)FindInputParameter(Properties.Resources.InputChartName).Value;
            double max = (double)FindInputParameter(Properties.Resources.InputMaxValue).Value;
            double min = (double)FindInputParameter(Properties.Resources.InputMinValue).Value;
            double step = (double)FindInputParameter(Properties.Resources.InputStep).Value;
            string fileName = ((StreamData)FindInputParameter(Properties.Resources.InputFolder).Value).FileName + "\\"
                + FindInputParameter(Properties.Resources.InputPath).Value.ToString();
            var points = GetPoints(graph, min, max, step);
            double progressStep = 100 / points.Count();
            using (StreamWriter sw = new(fileName, false))
            {
                foreach (var item in points)
                {
                    string str = item.X.ToString() + ' ' + item.Y.ToString();
                    sw.WriteLine(str.Replace(',', '.'));
                    Progress += progressStep;
                }
            }

        }
        private static IEnumerable<Point> GetPoints(IGraph graph, double min, double max, double step)
        {
            if (graph is PointGraph points)
                return points.Where(i => i.X >= min && i.X <= max);
            List<Point> res = new();
            for (double i = min; i < max; i += step)
                res.Add(new Point(i, graph.GetValue(i)));
            return res;
        }

    }

    [ExportGraph]
    public class ExportString : NodeFunctionBase<ExportString>
    {
        public ExportString()
        {
            Inputs.Add(new(Properties.Resources.InputString, typeof(string), string.Empty));
            Inputs.Add(new(Properties.Resources.InputFolder, typeof(StreamData), new StreamData(null, FileMode.Create)));
            Inputs.Add(new(Properties.Resources.InputPath, typeof(string), Properties.Resources.DefaultInputStringPath));
        }
        public override string GetName()
        {
            return Properties.Resources.ExportStringName;
        }

        public override void Invoke(CancellationToken token)
        {
            string str = (string)FindInputParameter(Properties.Resources.InputString).Value;
            string fileName = ((StreamData)FindInputParameter(Properties.Resources.InputFolder).Value).FileName + "\\"
                + FindInputParameter(Properties.Resources.InputPath).Value.ToString();
            using (StreamWriter sw = new(fileName, false))
            {
                    sw.WriteLine(str);
            }
            Progress = 100;
        }

    }

}
