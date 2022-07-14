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

namespace Gaussians
{
    internal class GraphManager: INotifyPropertyChanged
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

    }
    internal class GraphViewManager : GraphManager
    {
        public GraphViewManager():base()
        {
            GraphList = new Plot();
            GraphDataList.CollectionChanged += OnGraphDataListChanged;
        }

        protected void OnGraphDataListChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (GraphData item in e.NewItems)
                {
                    if (item is GraphVisualData viewData)
                    {
                        if (viewData.Graph is LineGraph)
                            BindingOperations.SetBinding(viewData.Graph, LineGraph.StrokeProperty, new Binding("GraphBrush") { Source = item });

                    }
                }
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



        private void OnVisibleGraphPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender == null)
                throw new ArgumentException("sender");
            if (sender is GraphVisualData visualData)
            {
                if (visualData.IsVisible)
                {
                    if (!GraphList.Children.Contains(visualData.Graph))
                        GraphList.Children.Add(visualData.Graph);
                }
                else
                    GraphList.Children.Remove(visualData.Graph);
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
    internal class GraphVisualData : GraphData
    {
        public GraphVisualData(string name, GaussiansModel.IGraph graph, Brush brush) : this(name, graph, brush, true) { }
        public GraphVisualData(string name, GaussiansModel.IGraph graph, Brush brush, bool isVisible) : base(name, graph)
        {
            GraphBrush = brush;
            IsVisible = isVisible;
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
    }
    internal class GraphData : INotifyPropertyChanged
    {
        public GraphData(string name, GaussiansModel.IGraph graph)
        {
            Name = name;
            GraphModel = graph;
        }

        private GaussiansModel.IGraph graphModel;
        public GaussiansModel.IGraph GraphModel
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
    }
}
