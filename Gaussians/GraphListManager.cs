using InteractiveDataDisplay.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using Gaussians.DataConverters;
using GaussiansModel;
using System.Windows;
using Gaussians.SettingComponents;
using System.Windows.Input;

namespace Gaussians
{
    internal class GraphManager: INotifyPropertyChanged, ICloneable
    {
        public GraphManager()
        {
            GraphDataList = new();
        }
        private ObservableCollection<GraphData> graphDataList;
        public ObservableCollection<GraphData> GraphDataList
        {
            get { return graphDataList; }
            set
            {
                graphDataList = value;
                OnPropertyChanged();
                OnPropertyChanged("GraphName");
            }
        }
        public virtual void AddGraph(GraphData data)
        {
            GraphDataList.Add(data);
        }
        public virtual void RemoveGraph(GraphData data)
        {
            GraphDataList.Remove(data);
        }

        protected void OnPropertyChanged([CallerMemberName] string? property = null) => PropertyChanged?.Invoke(this, new(property));
        public event PropertyChangedEventHandler? PropertyChanged;
        public virtual object Clone()
        {
            return new GraphManager() { GraphDataList = GraphDataList };
        }
    }
    internal class GraphViewManager : GraphManager
    {
        public GraphViewManager():base()
        {
            GraphList = new Plot();
            GraphListDates = new();
            GraphDataList.CollectionChanged += OnGraphDataListChanged;
        }

        protected void OnGraphDataListChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("GraphName");
            OnPropertyChanged("GraphList");
        }

        private PlotBase graphList;
        public PlotBase GraphList
        {
            get { return graphList; }
            private set
            {
                graphList = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<GraphVisualData> GraphListDates { get; init; } 


        private void OnVisibleGraphPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender == null)
                throw new ArgumentException("sender");
            if (sender is GraphVisualData visualData)
            {
                if (visualData.IsVisible)
                {
                    if (!GraphList.Children.Contains(visualData.Graph))
                    {
                        GraphList.Children.Add(visualData.Graph);
                        GraphListDates.Add(visualData);
                    }
                }
                else
                {
                    GraphList.Children.Remove(visualData.Graph);
                    GraphListDates.Remove(visualData);
                }
            }

        }
        public override void AddGraph(GraphData data)
        {
            data.PropertyChanged += OnVisibleGraphPropertyChanged;
            if (data is GraphVisualData visualData)
                visualData.Root = GraphList;
            base.AddGraph(data);
        }
        public void RemoveGraph(Plot plot)
        {
            GraphDataList.Remove(GraphDataList.Where(i => i is GraphVisualData visualData && visualData.Graph == plot).First());
        }
        public override object Clone()
        {
            var res = new GraphViewManager();
            foreach (var data in GraphDataList)
            {
                var newData = (GraphData)data.Clone();
                res.AddGraph(newData);
            }
            return res;
        }

    }

    internal class UnionGraphListManager : GraphManager
    {
        public UnionGraphListManager() : base() { }
        public UnionGraphListManager(params ObservableCollection<GraphData>[] graphCollections) : base()
        {
            foreach (var item in graphCollections)
            {
                foreach (var data in item)
                {
                    AddGraph(data);
                }
                item.CollectionChanged += OnChildGraphDataListChanged;
            }
        }

        private void OnChildGraphDataListChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
                foreach (GraphVisualData item in e.NewItems)
                    GraphDataList.Add(item);
            if (e.Action == NotifyCollectionChangedAction.Remove)
                foreach (GraphVisualData item in e.OldItems)
                    GraphDataList.Remove(item);
        }
    }

    internal class GraphVisualDataBuilder
    {
        private static readonly List<IGraphVisualDataCreater> visualDates = new()
        {
            new GraphVisualDataCreater(),
            new PointGraphVisualDataCreater()
        };
        public GraphVisualData Build(string name, IGraph graph)
        {
            return visualDates.Where(i => i.CanSupportGraph(graph)).First()
                .CreateGraphVisualData(name, graph, null);

        }
    }
    internal interface IGraphVisualDataCreater
    {
        public bool CanSupportGraph(IGraph graph);
        public GraphVisualData CreateGraphVisualData(string name, IGraph graph, Brush brush);

    }
    internal class GraphVisualDataCreater : IGraphVisualDataCreater
    {
        public bool CanSupportGraph(IGraph graph)
        {
            return typeof(GaussiansModel.FuncGraph) == graph.GetType();
        }

        public GraphVisualData CreateGraphVisualData(string name, IGraph graph, Brush brush)
        {
            return new GraphVisualData(name, graph, brush);
        }
    }
    internal class GraphVisualData : GraphData
    {

        public GraphVisualData(string name, IGraph graph, Brush brush) : this(name, graph, brush, true) { }
        public GraphVisualData(string name, IGraph graph, Brush brush, bool isVisible) : base(name, graph)
        {
            GraphBrush = brush;
            IsVisible = isVisible;

            Commands = new();
            Properties = new();
            Commands.Add(new SettingGraphRemoved());
            Properties.Add(new SettingGraphName());
            Properties.Add(new SettingGraphColor());
        }

        private Brush graphBrush;
        public Brush GraphBrush
        {
            get { return graphBrush; }
            set
            {
                graphBrush = value;
                OnPropertyChanged();
            }
        }

        private PlotBase root;
        public PlotBase Root
        {
            get { return root; }
            set
            {
                root = value;
                Graph = ModelToViewConverter.CreateVisualGraph(GraphModel, root);
                SetBindings();
                OnPropertyChanged();
            }
        }

        public PlotBase Graph { get; private set; }

        private bool isVisible;
        public bool IsVisible
        {
            get { return isVisible; }
            set
            {
                isVisible = value;
                OnPropertyChanged();
            }
        }

        public virtual void SetBindings()
        {
            if (Graph == null)
                return;
            BindingOperations.SetBinding(Graph, LineGraph.StrokeProperty, new Binding("GraphBrush") { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
        }



        protected List<SettingGraphActionBase> Commands { get; init; }
        protected List<SettingGraphPropertyBase> Properties { get; init; }
        public SettingComponentBase GetSettingGraph(GraphViewManager manager)
        {
            return new SettingGraphComponent(this, manager, Properties, Commands);
        }
        public override object Clone()
        {
            return new GraphVisualData(Name, GraphModel, GraphBrush, IsVisible);
        }
    }
    internal class PointGraphVisualDataCreater : IGraphVisualDataCreater
    {
        public bool CanSupportGraph(IGraph graph)
        {
            return typeof(GaussiansModel.PointGraph) == graph.GetType();
        }

        public GraphVisualData CreateGraphVisualData(string name, IGraph graph, Brush brush)
        {
            return new PointGraphVisualData(name, graph, brush);
        }
    }
    internal class PointGraphVisualData : GraphVisualData
    {
        public PointGraphVisualData(string name, IGraph graph, Brush brush) : this(name, graph, brush, true)
        {
        }

        public PointGraphVisualData(string name, IGraph graph, Brush brush, bool isVisible) : base(name, graph, brush, isVisible)
        {
            VisualMode = PointGraphState.OnlyPoints;
            Properties.Add(new SettingGraphVisualMode());
        }

        private PointGraphState visualMode;
        public PointGraphState VisualMode
        {
            get => visualMode;
            set
            {
                visualMode = value;
                OnPropertyChanged();
            }
        }
        public override void SetBindings()
        {
            base.SetBindings();
            BindingOperations.SetBinding(Graph, InteractiveDataDisplay.WPF.PointGraph.VisualModeProperty, new Binding("VisualMode") 
            { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
        }
    }
    internal class GraphData : INotifyPropertyChanged, ICloneable
    {
        public GraphData(string name, IGraph graph)
        {
            Name = name;
            GraphModel = graph;
        }

        private IGraph graphModel;
        public IGraph GraphModel
        {
            get { return graphModel; }
            set
            {
                graphModel = value;
                OnPropertyChanged();
                OnPropertyChanged("Graph");
            }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? property = null) => PropertyChanged?.Invoke(this, new(property));

        public event PropertyChangedEventHandler? PropertyChanged;

        public override string ToString()
        {
            return Name;
        }
        public virtual object Clone()
        {
            return new GraphData(Name, GraphModel);
        }
    }
}
