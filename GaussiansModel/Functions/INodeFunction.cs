using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using GaussiansModel.Extension;
using Newtonsoft.Json;

namespace GaussiansModel.Functions
{
    /// <summary>
    /// Менеджер функций
    /// </summary>
    public static class FunctionManagerOperations
    {
        /// <summary>
        /// Возвращает все функции некоторой группы
        /// </summary>
        /// <param name="attributeType">Аттрибут группы функций</param>
        /// <returns>Функции группы</returns>
        public static Dictionary<string, INodeFunction> FindFunctionRealizationsByAttribute(Type attributeType)
        {
            Dictionary<string, INodeFunction> res = new();

            Assembly assembly = Assembly.GetExecutingAssembly();
            var objs = assembly.FindTypesWithAttribute(attributeType);
            foreach (var obj in objs)
            {
                Type type = obj.Item1;
                var constructor = type.GetConstructor(new Type[] { });
                object? prototype = null;
                if (constructor != null)
                {
                    prototype = constructor.Invoke(new object[] { });
                }
                if (prototype != null && prototype is INodeFunction func)
                    res.Add(func.GetName(), func);
            }
            return res;
        }
    }
    /// <summary>
    /// Некоторая функция
    /// </summary>
    public interface INodeFunction : INotifyPropertyChanged, ICloneable
    {
        /// <summary>
        /// Имя функции
        /// </summary>
        public string Name
        {
            get; set;
        }
        /// <summary>
        /// Массив входных значений
        /// </summary>
        public IEnumerable<FunctionParameter>? Inputs { get; set; }
        /// <summary>
        /// Запуск функции
        /// </summary>
        public void Invoke();
        /// <summary>
        /// Название функции. Будет отображаться в меню
        /// </summary>
        /// <returns>Имя</returns>
        public string GetName();
        /// <summary>
        /// Массив выходных значений
        /// </summary>
        public IEnumerable<FunctionParameter>? Outputs { get; set; }
    }
    /// <summary>
    /// Реализация функции с некоторыми методами
    /// </summary>
    [JsonObject]
    public abstract class NodeFunctionBase<T> : INodeFunction where T : INodeFunction
    {
        public NodeFunctionBase()
        {
            Name = GetName();
        }
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public IEnumerable<FunctionParameter>? Inputs { get; set; }
        [JsonProperty]
        public IEnumerable<FunctionParameter>? Outputs { get; set; }
        public abstract string GetName();

        public abstract void Invoke();
        /// <summary>
        /// Ищет входной параметр по имени
        /// </summary>
        /// <param name="name">Имя параметра</param>
        /// <returns>Входной параметр</returns>
        protected FunctionParameter? FindInputParameter(string name) => Inputs.Where(i => i.Name == name).FirstOrDefault();
        /// <summary>
        /// Ищет выходной параметр по имени
        /// </summary>
        /// <param name="nameProperty">Имя параметра</param>
        /// <returns>Параметр</returns>
        protected void SetOutputParameter(string nameProperty, object value)
        {
            var output = Outputs.Where(i => i.Name == nameProperty).FirstOrDefault();
            if (output == null)
                throw new ArgumentException("nameProperty");
            output.Value = value;
        }

        public virtual object Clone()
        {
            Type type = typeof(T);
            var constr = type.GetConstructor(new Type[] { });
            INodeFunction res = (INodeFunction)constr.Invoke(new object[] { });
            res.Name = Name;
            return res;
        }

        protected void OnPropertyChanged([CallerMemberName] string? property = null) => PropertyChanged?.Invoke(this, new(property));
        public event PropertyChangedEventHandler? PropertyChanged;
    }

    /// <summary>
    /// Параметр функции
    /// </summary>
    [JsonObject]
    public class FunctionParameter
    {
        public FunctionParameter()
        {

        }
        public FunctionParameter(string name, Type valueType, object DefaultValue)
        {
            Name = name;
            ValueType = valueType;
            Value = DefaultValue;
        }
        /// <summary>
        /// Тип параметра. При присваивании параметра с другим типом будет ошибка!
        /// </summary>
        [JsonProperty]
        public Type ValueType { get; set; }
        [JsonProperty]
        public string Name { get; set; }
        private object value;
        [JsonProperty]
        public object Value
        {
            get { return value; }
            set
            {
                if (value == null)
                    return;
                Type type = value.GetType();
                if (ValueType.IsInstanceOfType(value))
                    this.value = value;
                else
                    throw new ArgumentException("value");
            }
        }
        public override string ToString()
        {
            return Name;
        }
    }
}
