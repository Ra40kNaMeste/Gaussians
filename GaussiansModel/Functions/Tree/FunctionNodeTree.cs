using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GaussiansModel.Functions
{
    /// <summary>
    /// Дерево из нол
    /// </summary>
    [JsonObject]
    public class FunctionNodeTree : INotifyPropertyChanged
    {
        public FunctionNodeTree()
        {
            Functions = new ObservableCollection<FunctionNodeData>();
            OutputContext = new();
            InputContext = new();
        }
        /// <summary>
        /// Обновление контекста для всех функций
        /// </summary>
        private void UpdateContext()
        {
            FunctionNodeContext context = (FunctionNodeContext)InputContext.Clone();
            foreach (var function in Functions)
            {
                function.Context = (FunctionNodeContext)context.Clone();
                if (function.Function.Outputs == null)
                    continue;
                foreach (var output in function.Function.Outputs)
                {
                    context.Context.Add(function.Function.Name + ":" + output.Name, output);
                }

            }
        }

        private FunctionNodeContext outputContext;
        /// <summary>
        /// Все результаты выполнения дерева
        /// </summary>
        public FunctionNodeContext OutputContext
        {
            get { return outputContext; }
            private set { outputContext = value; OnPropertyChanged(); }
        }

        private FunctionNodeContext inputContext;
        /// <summary>
        /// Изначальные данные
        /// </summary>
        public FunctionNodeContext InputContext
        {
            get { return inputContext; }
            set
            {
                inputContext = value;
                UpdateContext();
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Добавить функцию в конец цепочки
        /// </summary>
        /// <param name="function">Функция</param>
        public void AddFunction(INodeFunction function)
        {
            var coll = (ICollection<FunctionNodeData>)Functions;
            coll.Add(new(function));
            function.PropertyChanged += OnFunctionPropertyChanged;
            UpdateContext();
        }

        /// <summary>
        /// Вставляет функцию по индексу
        /// </summary>
        /// <param name="function">Функция</param>
        /// <param name="index">Индекс</param>
        public void InsertFunction(INodeFunction function, int index)
        {
            var coll = Functions;
            coll.Insert(index, new(function));
            function.PropertyChanged += OnFunctionPropertyChanged;
            UpdateContext();
        }

        /// <summary>
        /// Удалить функцию из цепочки
        /// </summary>
        /// <param name="function">Функция</param>
        public void RemoveFunction(INodeFunction function)
        {
            var coll = (ICollection<FunctionNodeData>)Functions;
            coll.Remove(coll.Where(i => i.Function == function).FirstOrDefault());
            UpdateContext();
        }
        /// <summary>
        /// Запустить выполнение цепочки. В это случае обновиться OutputContext
        /// </summary>
        /// <param name="token">Токен для отмены</param>
        public void Invoke(CancellationToken token)
        {
            List<CycleFuncMetadata> cycleFunces = new();

            int countFunction = Functions.Count;
            int func = 0;
            do
            {
                if (cycleFunces.Count != 0)
                    func = cycleFunces[0].IndexFunction;
                while (func < countFunction)
                {
                    FindAndSetNodePropertyFunction(Functions[func]);
                    INodeFunction nodeFunction = Functions[func].Function;
                    SetInvokeFunction(nodeFunction);

                    if (nodeFunction is IForeachFunctionable cycleFunc)
                    {
                        CycleFuncMetadata metadata = cycleFunces.Where(i => i.Target == cycleFunc).FirstOrDefault();
                        if (metadata == null)
                        {
                            metadata = new(cycleFunc, func);
                            cycleFunces.Insert(0, metadata);
                        }
                        cycleFunc.InvokeByIndex(metadata.Index++, token);
                    }
                    else
                        nodeFunction.Invoke(token);

                    ClearInvokeFunction(nodeFunction);
                    AddOutputPropertyInContext(Functions[func]);
                    if (token.IsCancellationRequested)
                        return;
                    func++;
                }
                while (cycleFunces.Count != 0 && cycleFunces[0].IsClosed())
                    cycleFunces.RemoveAt(0);
            } while (cycleFunces.Count != 0);

        }

        /// <summary>
        /// Устанавливает входные значения для функции
        /// </summary>
        /// <param name="function">Все данные функции</param>
        private void FindAndSetNodePropertyFunction(FunctionNodeData function)
        {
            if (function.Function.Inputs == null)
                return;
            function.OnBindings();
        }
        /// <summary>
        /// Добавляет результат выполнения функции в общий контекст
        /// </summary>
        /// <param name="function">Все данные функции</param>
        private void AddOutputPropertyInContext(FunctionNodeData function)
        {
            if (function.Function.Outputs == null)
                return;
            foreach (var property in function.Function.Outputs)
                OutputContext.Context.Where(i => i.Key == CreateNameParameter(function.Function, property.Name));
        }

        /// <summary>
        /// Генератор имени выходного значения функции, как имя_функции.имя_выхода
        /// </summary>
        /// <param name="function">Функция</param>
        /// <param name="name">Имя выхода</param>
        /// <returns></returns>
        private static string CreateNameParameter(INodeFunction function, string name) => function.Name + "." + name;
        private void OnFunctionPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Name")
                UpdateContext();
        }

        private ObservableCollection<FunctionNodeData> functions;
        /// <summary>
        /// Цепочка функций
        /// </summary>
        [JsonProperty]
        public ObservableCollection<FunctionNodeData> Functions
        {
            get { return functions; }
            init
            {
                functions = value;
                OnPropertyChanged();
            }
        }

        private string name;
        /// <summary>
        /// Имя цепочки функций
        /// </summary>
        [JsonProperty]
        public string Name 
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? property = null) => PropertyChanged?.Invoke(this, new(property));
        public event PropertyChangedEventHandler? PropertyChanged;
        /// <summary>
        /// Добавить выполняющуюся функцию
        /// </summary>
        /// <param name="function">Функция</param>
        private void SetInvokeFunction(INodeFunction function)
        {
            function.PropertyChanged += InvokeFunctionPropertyChanged;
            FunctionProgressChanged?.Invoke(this, new(function.Name, 0));
        }
        /// <summary>
        /// Удалить выполняющую функцию
        /// </summary>
        /// <param name="function">Функция</param>
        private void ClearInvokeFunction(INodeFunction function)
        {
            FunctionProgressChanged?.Invoke(this, new(function.Name, 100));
            function.PropertyChanged -= InvokeFunctionPropertyChanged;
        }
        private void InvokeFunctionPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if(sender != null && e.PropertyName == "Progress")
            {
                INodeFunction function = (INodeFunction)sender;
                FunctionProgressChanged?.Invoke(this, new(function.Name, function.Progress));
            }
        }
        /// <summary>
        /// Прогресс выполнения цепочки
        /// </summary>
        public event FunctionProgressEventHandler? FunctionProgressChanged;
    }

    internal class CycleFuncMetadata
    {
        public CycleFuncMetadata(IForeachFunctionable func, int indexFunc)
        {
            count = func.Count();
            IndexFunction = indexFunc;
            Target = func;
            Index = 0;
        }
        private int count;
        public bool IsClosed() => Index >= count;
        public int Index { get; set; }
        public int IndexFunction { get; init; }
        public IForeachFunctionable Target { get; init; }
    }
    /// <summary>
    /// Данные функции: сама функция + контекст
    /// </summary>
    [JsonObject]
    public class FunctionNodeData : INotifyPropertyChanged
    {
        protected internal FunctionNodeData()
        {

        }
        protected internal FunctionNodeData(INodeFunction function)
        {
            Function = function;
            BindingDates = new();
        }
        protected internal FunctionNodeData(INodeFunction function, FunctionNodeContext context):this(function)
        {
            Context = context;
        }

        private INodeFunction function;
        /// <summary>
        /// Функция
        /// </summary>
        [JsonProperty]
        public INodeFunction Function
        {
            get { return function; }
            set
            {
                function = value;
                OnPropertyChanged();
            }
        }
        private FunctionNodeContext context;
        /// <summary>
        /// Контекст функции
        /// </summary>
        [JsonProperty]
        public FunctionNodeContext Context
        {
            get { return context; }
            set
            {
                context = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Добавление привязок
        /// </summary>
        protected internal void OnBindings()
        {
            foreach (var item in BindingDates)
            {
                item.OnBinding(this);
            }
        }
        private List<FunctionNodeBindingData> bindingDates;
        /// <summary>
        /// Возможные привязки
        /// </summary>
        [JsonProperty]
        public List<FunctionNodeBindingData> BindingDates
        {
            get { return bindingDates; }
            set
            {
                bindingDates = value;
                OnPropertyChanged();
            }
        }
        private void OnPropertyChanged([CallerMemberName] string? property = null) => PropertyChanged?.Invoke(this, new(property));
        public event PropertyChangedEventHandler? PropertyChanged;

        public override string ToString()
        {
            return Function.Name;
        }
    }
    /// <summary>
    /// Контекст функции
    /// </summary>
    public class FunctionNodeContext : ICloneable
    {
        public FunctionNodeContext()
        {
            Context = new();
        }
        public FunctionNodeContext(Dictionary<string, FunctionParameter> context)
        {
            Context = context;
        }

        public Dictionary<string, FunctionParameter> Context { get; init; }
        public object Clone()
        {
            return new FunctionNodeContext(Context.ToDictionary(i => i.Key, i => i.Value));
        }
    }
    public delegate void FunctionProgressEventHandler(object sender, FunctionProgressEventArgs e);
    public class FunctionProgressEventArgs
    {
        protected internal FunctionProgressEventArgs(string funcName, double progress)
        {
            FuncName = funcName;
            Progress = progress;
        }

        public string FuncName { get; init; }
        public double Progress { get; init; }
    }
    /// <summary>
    /// Привяка функции
    /// </summary>
    [JsonObject]
    public class FunctionNodeBindingData
    {
        /// <summary>
        /// Имя входа функции под привязкой
        /// </summary>
        [JsonProperty]
        public string NameTargetProperty { get; set; }
        /// <summary>
        /// Имя свойства в контексте
        /// </summary>
        [JsonProperty]
        public string NamePropertyOnContext { get; set; }
        /// <summary>
        /// Назначает на вход функции свойство из контекста
        /// </summary>
        /// <param name="data"></param>
        protected internal void OnBinding(FunctionNodeData data)
        {
            var targetParameter = data.Function.Inputs.Where(i => i.Name == NameTargetProperty).FirstOrDefault();
            var contextParameter = data.Context.Context.Where(i=>i.Key == NamePropertyOnContext).FirstOrDefault().Value;
            if (targetParameter == null || contextParameter == null)
                return;
            targetParameter.Value = contextParameter.Value;
        }

    }


}
