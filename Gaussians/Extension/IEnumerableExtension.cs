using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gaussians.Extension
{
    internal static class IEnumerableExtension
    {
        public static int IndexOf<T>(this IEnumerable<T> values, T value)
        {
            int index = 0;
            foreach (T item in values)
            {
                if (item != null && item.Equals(value))
                    return index;
                index++;
            }
            return -1;
        }
    }
}
