using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GaussiansModel.Functions
{
    [AttributeUsage(AttributeTargets.Class)]
    public class StringFunctionsAttribute:Attribute
    { }
    [StringFunctions]
    public class StringAddBuilder : NodeFunctionBase<StringAddBuilder>
    {
        public StringAddBuilder()
        {
            Inputs.Add(new(Properties.Resources.InputString, typeof(string), string.Empty));
            Inputs.Add(new(Properties.Resources.InputStringTwo, typeof(string), string.Empty));

            Outputs.Add(new(Properties.Resources.OutputString, typeof(string), string.Empty));
        }
        public override string GetName()
        {
            return Properties.Resources.StringAddBuilderName;
        }

        public override void Invoke(CancellationToken token)
        {
            string str1 = (string)FindInputParameter(Properties.Resources.InputString).Value;
            string str2 = (string)FindInputParameter(Properties.Resources.InputStringTwo).Value;

            SetOutputParameter(Properties.Resources.OutputString, str1 + str2);
        }
    }
}
