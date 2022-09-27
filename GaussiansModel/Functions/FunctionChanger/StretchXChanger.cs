using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GaussiansModel.Functions
{
    [AttributeUsage(AttributeTargets.Class)]
    public class FunctionChangedAttribute:Attribute
    {

    }
    [FunctionChanged]
    public class StretchXChanger : NodeFunctionBase<StretchXChanger>
    {
        public StretchXChanger()
        {
            Inputs.Add(new FunctionParameter(Properties.Resources.InputChartName, typeof(PointGraph), new PointGraph()));
            Inputs.Add(new FunctionParameter(Properties.Resources.InputMinValue, typeof(double), 0.0));
            Inputs.Add(new FunctionParameter(Properties.Resources.InputMaxValue, typeof(double), 1.0));

            Outputs.Add(new FunctionParameter(Properties.Resources.OutputGraph, typeof(PointGraph), new PointGraph()));
        }
        public override string GetName()
        {
            return Properties.Resources.StretchXChanger;
        }

        public override void Invoke(CancellationToken token)
        {
            PointGraph points = (PointGraph)FindInputParameter(Properties.Resources.InputChartName).Value;
            double min = (double)FindInputParameter(Properties.Resources.InputMinValue).Value;
            double max = (double)FindInputParameter(Properties.Resources.InputMaxValue).Value;

            double maxPointX = points.Max(i => i.X);
            double minPointX = points.Min(i => i.X);

            double multiply = (max - min) / (maxPointX - minPointX);
            double progressStep = 100 / points.Count;

            PointGraph res = new(points.Select(i =>
            {
                Progress += progressStep;
                return new Point((i.X - minPointX) * multiply + min, i.Y);
            }));
            SetOutputParameter(Properties.Resources.OutputGraph, res);
        }
    }
}
