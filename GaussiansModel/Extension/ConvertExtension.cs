using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GaussiansModel.Extension
{
    internal static class ConvertExtension
    {
        public static double ToDoubleRich(string val)
        {
            val = val.Replace('.', ',');
            string[] strs = val.Split(new char[] { 'e', 'E' });
            double res = 0;
            if (strs.Length > 0)
                res = Convert.ToDouble(strs.ElementAt(0));
            if (strs.Length > 1)
                res *= Math.Pow(10, Convert.ToDouble(strs.ElementAt(1)));
            return res;
        }
    }
}
