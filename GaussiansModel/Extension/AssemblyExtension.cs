using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GaussiansModel.Extension
{
    /// <summary>
    /// Расширения для Assembly
    /// </summary>
    public static class AssemblyExtension
    {
        /// <summary>
        /// Возвращает все типы, в которых присутствуют аттрибуты и аттрибуты
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="attributeType">Тип аттрибута для поиска</param>
        /// <returns>Найденные типы</returns>
        /// <exception cref="Exception">Ошибка. Быть не должно</exception>
        public static IEnumerable<(Type, Attribute)> FindTypesWithAttribute(this Assembly assembly, Type attributeType)
        {
            return assembly.GetTypes()
                .Select(i => new { attr = i.GetCustomAttribute(attributeType), type = i })
                .Where(i => i.attr != null)
                .Select(i => (i.type, i.attr ?? throw new Exception()));
        }
    }
}
