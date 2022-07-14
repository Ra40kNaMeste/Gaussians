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
    [JsonObject]
    public class FunctionNodeTree : INotifyPropertyChanged
    {
        public FunctionNodeTree()
        {
            Functions = new ObservableCollection<FunctionNodeData>();
            OutputContext = new();
            InputContext = new();
        }
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
        public FunctionNodeContext OutputContext
        {
            get { return outputContext; }
            private set { outputContext = value; OnPropertyChanged(); }
        }

        private FunctionNodeContext inputContext;
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

        public void AddFunction(INodeFunction function)
        {
            var coll = (ICollection<FunctionNodeData>)Functions;
            coll.Add(new(function));
            function.PropertyChanged += OnFunctionPropertyChanged;
            UpdateContext();
        }
        public void RemoveFunction(INodeFunction function)
        {
            var coll = (ICollection<FunctionNodeData>)Functions;
            coll.Remove(coll.Where(i => i.Function == function).FirstOrDefault());
            UpdateContext();
        }
        public void Invoke()
        {
            foreach (var function in Functions)
            {
                FindAndSetNodePropertyFunction(function);
                function.Function.Invoke();
                AddOutputPropertyInContext(function);
            }
        }
        private void FindAndSetNodePropertyFunction(FunctionNodeData function)
        {
            if (function.Function.Inputs == null)
                return;
            function.OnBindings();
        }

        private void AddOutputPropertyInContext(FunctionNodeData function)
        {
            if (function.Function.Outputs == null)
                return;
            foreach (var property in function.Function.Outputs)
                OutputContext.Context.Where(i => i.Key == CreateNameParameter(function.Function, property.Name));
        }

        private static string CreateNameParameter(INodeFunction function, string name) => function.Name + "." + name;

        public void InsertFunction(INodeFunction function, int index)
        {
            var coll = Functions;
            coll.Insert(index, new(function));
            function.PropertyChanged += OnFunctionPropertyChanged;
            UpdateContext();
        }

        private void OnFunctionPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Name")
                UpdateContext();
        }

        private ObservableCollection<FunctionNodeData> functions;
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
        private void OnPropertyChanged([CallerMemberName] string? property = null) => PropertyChanged?.Invoke(this, new(property));
        public event PropertyChangedEventHandler? PropertyChanged;

    }
    [JsonObject]
    public class FunctionNodeData : INotifyPropertyChanged
    {
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
        protected internal void OnBindings()
        {
            foreach (var item in BindingDates)
            {
                item.OnBinding(this);
            }
        }
        private List<FunctionNodeBindingData> bindingDates;
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

    [JsonObject]
    public class FunctionNodeBindingData
    {
        [JsonProperty]
        public string NameTargetProperty { get; set; }
        [JsonProperty]
        public string NamePropertyOnContext { get; set; }
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
