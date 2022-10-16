using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            return FindFunctionRealizationsByAttribute(attributeType, Assembly.GetExecutingAssembly());
        }

        public static Dictionary<string, INodeFunction> FindFunctionRealizationsByAttribute(Type attributeType, Assembly assembly)
        {
            Dictionary<string, INodeFunction> res = new();
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
    [JsonObject]
    public interface INodeFunction : INotifyPropertyChanged, ICloneable
    {
        /// <summary>
        /// Имя функции
        /// </summary>
        [JsonProperty]
        public string Name
        {
            get; set;
        }
        /// <summary>
        /// Массив входных значений
        /// </summary>
        [JsonProperty]
        public ICollection<FunctionParameter>? Inputs { get; }
        /// <summary>
        /// Запуск функции
        /// </summary>
        public void Invoke(CancellationToken token);
        /// <summary>
        /// Название функции. Будет отображаться в меню
        /// </summary>
        /// <returns>Имя</returns>
        public string GetName();
        /// <summary>
        /// Массив выходных значений
        /// </summary>
        [JsonProperty]
        public ICollection<FunctionParameter>? Outputs { get; }
        /// <summary>
        /// Прогресс выполнения от 0 до 100
        /// </summary>
        public double Progress { get; }
    }
    /// <summary>
    /// Реализация функции с некоторыми методами
    /// </summary>
    public abstract class NodeFunctionBase<T> : INodeFunction, INotifyPropertyChanged where T : INodeFunction
    {
        public NodeFunctionBase()
        {
            Name = GetName();
            Progress = 0.0;
            Inputs = CreateCollection();
            Outputs = CreateCollection();
        }
        private ObservableCollection<FunctionParameter> CreateCollection()
        {
            var res = new ObservableCollection<FunctionParameter>();
            res.CollectionChanged += OnCollectionChanged;
            return res;
        }

        private void OnCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null || sender == null)
                return;
            var res = (ObservableCollection<FunctionParameter>)sender;
            foreach (FunctionParameter item in e.NewItems)
                foreach (FunctionParameter oldItem in res)
                    if (item.Name == oldItem.Name && item != oldItem)
                    {
                        res.Remove(oldItem);
                        break;
                    }
        }

        private string name;
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        private ICollection<FunctionParameter>? inputs;
        public ICollection<FunctionParameter>? Inputs
        {
            get => inputs;
            private set
            {
                inputs = value;
                OnPropertyChanged();
            }
        }

        private ICollection<FunctionParameter>? outputs;
        public ICollection<FunctionParameter>? Outputs
        {
            get => outputs;
            private set
            {
                outputs = value;
                OnPropertyChanged();
            }
        }

        private double progress;
        /// <summary>
        /// Прогресс выполнения функции от 0 до 100
        /// </summary>
        public double Progress
        {
            get => progress;
            protected set
            {
                progress = value;
                OnPropertyChanged();
            }
        }

        public abstract string GetName();
        public abstract void Invoke(CancellationToken token);
        /// <summary>
        /// Ищет входной параметр по имени
        /// </summary>
        /// <param name="name">Имя параметра</param>
        /// <returns>Входной параметр</returns>
        protected FunctionParameter? FindInputParameter

            (string name) => Inputs.Where(i => i.Name == name).FirstOrDefault();
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
        public static bool CanValidation(Type type, object obj)
        {
            if (obj == null)
                return true;
            if (obj is FunctionParameter parameter)
            {
                return parameter.ValueType == typeof(long) && type == typeof(int) ||
                    type.IsInstanceOfType(parameter.Value) || parameter.ValueType.IsSubclassOf(type) || parameter.ValueType == type;
            }
            return false;
        }
        protected static object Validation(Type type, object obj)
        {
            if (obj == null)
                return null;
            Type typeObj = obj.GetType();
            if (typeObj == typeof(long) && type == typeof(int))
                return Convert.ToInt32(obj);
            if (type.IsInstanceOfType(obj) || typeObj.IsSubclassOf(type) || typeObj == type)
                return obj;
            return null;
        }
        [JsonProperty]
        public object Value
        {
            get { return value; }
            set
            {
                object obj = Validation(ValueType, value);
                this.value = obj;

            }
        }
        public override string ToString()
        {
            return Name;
        }
    }
}
