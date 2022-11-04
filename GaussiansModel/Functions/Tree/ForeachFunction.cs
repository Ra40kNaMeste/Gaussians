using GaussiansModel.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GaussiansModel.Functions
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ForeachFunctionAttribute:Attribute
    {

    }
    public interface IForeachFunctionable
    {
        public int Count();
        public void InvokeByIndex(int index, CancellationToken token);
    }
    public class ForeachFunction<T> : NodeFunctionBase<ForeachFunction<T>>, IForeachFunctionable
        where T : new()
    {

        public ForeachFunction()
        {
            Inputs.Add(new(Properties.Resources.InputArray, typeof(IEnumerable<T>), new List<T>()));
            Outputs.Add(new(Properties.Resources.OutputValue, typeof(T), new T()));
            Outputs.Add(new(Properties.Resources.OutputValueIndex, typeof(string), string.Empty));
        }
        public int Count()
        {
            IEnumerable<T> res = (IEnumerable<T>)FindInputParameter(Properties.Resources.InputArray).Value;
            return res.Count();
        }
        public void InvokeByIndex(int index, CancellationToken token)
        {
            IEnumerable<T> res = (IEnumerable<T>)FindInputParameter(Properties.Resources.InputArray).Value;
            SetOutputParameter(Properties.Resources.OutputValue, res.ElementAt(index));
            SetOutputParameter(Properties.Resources.OutputValueIndex, index + 1);
        }
        public override void Invoke(CancellationToken token)
        {
            
        }

        public override string GetName()
        {
            return String.Empty;
        }
    }
    [ForeachFunction]
    public class PointGraphForeachFunction : ForeachFunction<PointGraph>
    {
        public override string GetName()
        {
            return Properties.Resources.PointGraphForeachFunctionName;
        }
    }
}
