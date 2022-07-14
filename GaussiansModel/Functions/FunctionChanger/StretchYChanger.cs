using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GaussiansModel.Functions.FunctionChanger
{
    [FunctionChanged]
    public class StretchYChanger : NodeFunctionBase<StretchYChanger>
    {
        public StretchYChanger()
        {
            Inputs = new List<FunctionParameter>()
            {
                new FunctionParameter(Properties.Resources.InputChartName, typeof(PointGraph), new PointGraph()),
                new FunctionParameter(Properties.Resources.InputMinValue, typeof(double), 0.0),
                new FunctionParameter(Properties.Resources.InputMaxValue, typeof(double), 1.0)
            };
            Outputs = new List<FunctionParameter>()
            {
                new FunctionParameter(Properties.Resources.OutputGraph, typeof(PointGraph), new PointGraph())
            };
        }
        public override string GetName()
        {
            return Properties.Resources.StretchYChanger;
        }

        public override void Invoke()
        {
            PointGraph points = (PointGraph)FindInputParameter(Properties.Resources.InputChartName).Value;
            double min = (double)FindInputParameter(Properties.Resources.InputMinValue).Value;
            double max = (double)FindInputParameter(Properties.Resources.InputMaxValue).Value;

            double maxPointY = points.Max(i => i.Y);
            double minPointY = points.Min(i => i.Y);

            double multiply = (max - min) / (maxPointY - minPointY);

            PointGraph res = new(points.Select(i => new Point(i.X, (i.Y - minPointY) * multiply + min)));

            SetOutputParameter(Properties.Resources.OutputGraph, res);
        }
    }
}
