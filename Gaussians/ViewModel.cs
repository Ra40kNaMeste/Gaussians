using Gaussians.Commands;
using GaussiansModel.Functions;
using Gaussians.SettingComponents;
using InteractiveDataDisplay.WPF;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using GaussiansModel;
using Gaussians.Extension;
using Gaussians.DataConverters;
using System.Windows;

namespace Gaussians
{
    internal class ViewModel : INotifyPropertyChanged
    {
        #region Settings
        private static Dictionary<string, Type> listOperationGroups = new()
        {
            { Properties.Resources.ReadGraphOperations, typeof(FileReaderAttribute) },
            {Properties.Resources.SmoothingOperations, typeof(SmoothingFinctionAttribute) },
            {Properties.Resources.OperationGraphChangers, typeof(FunctionChangedAttribute) },
            {Properties.Resources.ApproximationOperations, typeof(ApproximationFunctionAttribute) },
            {Properties.Resources.GraphProperties, typeof(FunctionParameterAttribute) },
            {Properties.Resources.GraphGaussianFinders, typeof(GaussianFinderAttribute) }
        };

        #endregion

        #region Constructons
        public ViewModel()
        {
            Init();
        }

        private void Init()
        {
            CreateGrapgs();
            CreateListReaders();
            CreateOperationGropList();
            PropertyConverter = new(this);
            Generator = new();
        }

        private void CreateGrapgs()
        {
            GraphList = new();
        }

        private void CreateListReaders()
        {
            MenuFileReaders = FunctionManagerOperations.FindFunctionRealizationsByAttribute(typeof(FileReaderAttribute));
        }
        private void CreateOperationGropList()
        {
            OperationGroupList = listOperationGroups.ToDictionary(i => i.Key,
                i => FunctionManagerOperations.FindFunctionRealizationsByAttribute(i.Value));
        }
        #endregion //Constructions

        #region VisualProperties


        private GraphViewManager graphList;
        /// <summary>
        /// Коллекция графиков
        /// </summary>
        public GraphViewManager GraphList
        {
            get { return graphList; }
            private set
            {
                graphList = value;
                OnPropertyChanged();
            }
        }

        private SettingComponentBase viewProperties;
        public SettingComponentBase ViewProperties
        {
            get { return viewProperties; }
            private set
            {
                viewProperties = value;
                OnPropertyChanged();
            }
        }

        private Dictionary<string, INodeFunction> menuFileReaders;
        public Dictionary<string, INodeFunction> MenuFileReaders
        {
            get { return menuFileReaders; }
            private set
            {
                menuFileReaders = value;
                OnPropertyChanged();
            }
        }

        private Dictionary<string, Dictionary<string, INodeFunction>> operationGroupList;
        public Dictionary<string, Dictionary<string, INodeFunction>> OperationGroupList
        {
            get { return operationGroupList; }
            set
            {
                operationGroupList = value;
                OnPropertyChanged();
            }
        }

        private FunctionNodeTree nodes;
        public FunctionNodeTree Nodes
        {
            get { return nodes; }
            private set
            {
                nodes = value;
                OnPropertyChanged();
            }
        }

        private FunctionNodeData selectedOperation;
        public FunctionNodeData SelectedOperation
        {
            get { return selectedOperation; }
            set
            {
                selectedOperation = value;
                OnPropertyChanged();
            }
        }

        private InputPropertyModelToVisualConverter propertyConverter;
        public InputPropertyModelToVisualConverter PropertyConverter
        {
            get { return propertyConverter; }
            set
            {
                propertyConverter = value;
                OnPropertyChanged();
            }
        }

        #endregion //VisualProperties

        #region PrivateProperties

        #endregion//PeivateProperties

        #region Commands

        #region HeaderComamnd

        private DefaultCommand readFileCommand;
        public DefaultCommand ReadFileCommand { get { return readFileCommand ??= new(ReadFileCommandBody); } }

        private DefaultCommand showPropertiesGraphCommand;
        public DefaultCommand ShowPropertiesGraphCommand => showPropertiesGraphCommand ??= new(ShowPropertiesGraphBody);

        private DefaultCommand swapVisibleGraphCommand;
        public DefaultCommand SwapVisibleGraphCommand => swapVisibleGraphCommand ??= new(SwapVisibleGraphBody);

        private DefaultCommand invokeFunctionGraphCommand;
        public DefaultCommand InvokeFunctionGraphCommand => invokeFunctionGraphCommand ??= new(InvokeFunctionGraphBody);

        private DefaultCommand createOperationCommand;
        public DefaultCommand CreateOperationCommand => createOperationCommand ??= new(CreateOperationBody);

        private DefaultCommand closeGraphCommand;
        public DefaultCommand CloseGraphCommand => closeGraphCommand ??= new(CloseGraphBody);

        private DefaultCommand addOperationInNodeCommand;
        public DefaultCommand AddOpertionInNodeCommand => addOperationInNodeCommand ??= new(AddOperationInNodeBody);

        private DefaultCommand insertOperationInNodeCommand;
        public DefaultCommand InsertOperationInNodeCommand => insertOperationInNodeCommand ??= new(InsertOperationInNodeBody);

        private DefaultCommand removeOperationInNodeCommand;
        public DefaultCommand RemoveOperationInNodeCommand => removeOperationInNodeCommand ??= new(RemoveOperationInNodeBody);


        #endregion //HeaderCommand

        #region BodyCommand

        public void ReadFileCommandBody(object? parameter)
        {
            try
            {
                if (parameter is IFileReader reader)
                {
                    OpenFileDialog dialog = new()
                    {
                        Title = Properties.Resources.MenuOpen
                    };
                    if (dialog.ShowDialog() == true)
                    {
                        Stream file = dialog.OpenFile();
                        GaussiansModel.PointGraph chart = reader.ReadFile(file);
                        AddPointsGraph(chart);
                    }
                }
            }
            catch (Exception)
            {

            }

        }

        public void ShowPropertiesGraphBody(object? parameter)
        {
            if (parameter is GraphVisualData metadata)
                ViewProperties = new SettingGraphComponent(metadata, GraphList);
        }

        public void SwapVisibleGraphBody(object? parameter)
        {
            if (parameter is GraphVisualData metadata)
                metadata.IsVisible = !metadata.IsVisible;
        }

        public void InvokeFunctionGraphBody(object? parameter)
        {
            try
            {
                if (Nodes != null)
                {
                    Nodes.InputContext = new(GraphList.GraphDataList.DistinctBy(i => i.Name).ToDictionary(i => i.Name, i => new FunctionParameter(i.Name, typeof(IGraph), null) { Value = i.GraphModel }));
                    Nodes.Invoke();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error");
            }

        }

        public void CreateOperationBody(object? parameter)
        {
            if (parameter is INodeFunction function)
            {
                FunctionNodeTree tree = new();
                tree.AddFunction((INodeFunction)function.Clone());
                tree.AddFunction(new ShowGraphNode(GraphList, Generator));
                Nodes = tree;
            }
        }

        public void CloseGraphBody(object? parameter)
        {
            ViewProperties = null;
        }

        public void AddOperationInNodeBody(object? parameter)
        {
            if (parameter is INodeFunction function && Nodes != null)
                Nodes.AddFunction(function);
        }
        public void InsertOperationInNodeBody(object? parameter)
        {
            if (parameter is INodeFunction function && Nodes != null)
            {
                int index = Nodes.Functions.IndexOf(SelectedOperation);
                Nodes.InsertFunction(function, index);
            }
        }
        public void RemoveOperationInNodeBody(object? parameter)
        {
            Nodes.RemoveFunction(SelectedOperation.Function);
        }


        #endregion //BodyCommand

        #endregion //Commands

        #region PrivateMethods

        private void AddPointsGraph(IGraph graph)
        {
            GraphVisualData metadata = new(Generator.GetNameGraph(), graph, new SolidColorBrush(Generator.GetColor()));
            GraphList.AddGraph(metadata);
            BindingOperations.SetBinding(metadata.Graph, LineGraph.StrokeProperty, new Binding("GraphBrush") { Source = metadata, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
        }
        #endregion//PrivateMethods

        #region PrivateProperties
        private SourceGenerator Generator { get; set; }
        #endregion//PriavteProperties

        #region Events
        private void OnPropertyChanged([CallerMemberName] string? prop = null) => PropertyChanged?.Invoke(this, new(prop));
        public event PropertyChangedEventHandler? PropertyChanged;
        #endregion//Events
    }
}
