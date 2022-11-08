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
using System.Reflection;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using Ookii.Dialogs.Wpf;
using Gaussians.ViewFunctions;
using System.Windows.Media.Imaging;
using Gaussians.DialogWindows.ExportGraphs;

namespace Gaussians
{
    internal class ViewModel : INotifyPropertyChanged
    {
        #region Settings
        private static Dictionary<string, Type> listOperationGroups = new()
        {
            {Properties.Resources.ReadGraphOperations, typeof(FileReaderAttribute) },
            {Properties.Resources.SmoothingOperations, typeof(SmoothingFinctionAttribute) },
            {Properties.Resources.OperationGraphChangers, typeof(FunctionChangedAttribute) },
            {Properties.Resources.GraphProperties, typeof(FunctionParameterAttribute) },
            {Properties.Resources.ExportGraphOperations, typeof(ExportGraphAttribute) },
            {Properties.Resources.StringOperations, typeof(StringFunctionsAttribute) },
            {Properties.Resources.ForeachOperations, typeof(ForeachFunctionAttribute) }
        };

        #endregion

        #region Constructons
        public ViewModel()
        {
            Init();
        }

        private void Init()
        {
            TreesManager = new();
            PropertyConverter = new(this);
            Generator = new();
            GraphBuilder = new();
            IsInvoke = false;
            CreateGrapgs();
            CreateListReaders();
            CreateOperationGroupList();
        }

        private void CreateGrapgs()
        {
            GraphList = new();
        }

        private void CreateListReaders()
        {
            MenuFileReaders = FunctionManagerOperations.FindFunctionRealizationsByAttribute(typeof(FileReaderAttribute));
        }
        private void CreateOperationGroupList()
        {
            OperationGroupList = listOperationGroups.ToDictionary(i => i.Key,
                i => FunctionManagerOperations.FindFunctionRealizationsByAttribute(i.Value)
                .Union(FunctionManagerOperations.FindFunctionRealizationsByAttribute(i.Value, Assembly.GetExecutingAssembly())
                .Select(i =>
                {
                    if (i.Value is ISetGeneratorElements t)
                        t.SetViewModelElements(this);
                    return i;
                }))
                .ToDictionary(i => i.Key, i => i.Value)
                );

        }
        #endregion //Constructions

        #region VisualProperties

        private string invokeFunctionName;
        public string InvokeFunctionName
        {
            get => invokeFunctionName;
            set
            {
                invokeFunctionName = value;
                OnPropertyChanged();
            }
        }
        private double invokeFunctionProgress;
        public double InvokeFunctionProgress
        {
            get => invokeFunctionProgress;
            set
            {
                invokeFunctionProgress = value;
                OnPropertyChanged();
            }
        }

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

        private TreesManager treesManager;
        public TreesManager TreesManager
        {
            get { return treesManager; }
            set
            {
                treesManager = value;
                OnPropertyChanged();
            }
        }

        private FunctionNodeTree selectNodeTree;
        public FunctionNodeTree SelectNodeTree
        {
            get { return selectNodeTree; }
            set
            {
                selectNodeTree = value;
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
        private IEnumerable<string> themes;
        public IEnumerable<string> Themes 
        {
            get => themes;
            set
            {
                themes = value;
                OnPropertyChanged();
            }
        }

        #endregion //VisualProperties

        #region Commands

        #region HeaderComamnd

        private DefaultCommand readFileCommand;
        public DefaultCommand ReadFileCommand { get { return readFileCommand ??= new(ReadFileBody); } }

        private DefaultCommand showPropertiesGraphCommand;
        public DefaultCommand ShowPropertiesGraphCommand => showPropertiesGraphCommand ??= new(ShowPropertiesGraphBody);

        private DefaultCommand swapVisibleGraphCommand;
        public DefaultCommand SwapVisibleGraphCommand => swapVisibleGraphCommand ??= new(SwapVisibleGraphBody);

        private DefaultCommand invokeFunctionGraphCommand;
        public DefaultCommand InvokeFunctionGraphCommand => invokeFunctionGraphCommand ??= new(InvokeFunctionGraphBody, CanInvokeFunctionGraphBody);

        private DefaultCommand cancelInvokeFunctionGraphCommand;
        public DefaultCommand CancelInvokeFunctionGraphCommand => cancelInvokeFunctionGraphCommand ??= new(CancelInvokeFunctionGraphBody, CanCancelInvokeFunctionGraphBody);

        private DefaultCommand addNodeTreeCommand;
        public DefaultCommand AddNodeTreeCommand => addNodeTreeCommand ??= new(AddNodeTreeBody);

        private DefaultCommand removeNodeTreeCommand;
        public DefaultCommand RemoveNodeTreeCommand => removeNodeTreeCommand ??= new(RemoveNodeTreeBody, CanRemoveNodeTreeBody);

        private DefaultCommand closeGraphCommand;
        public DefaultCommand CloseGraphCommand => closeGraphCommand ??= new(CloseGraphBody);

        private OnlyConditionCommand canChangedNodeCommand;
        public OnlyConditionCommand CanChangedNodeCommand => canChangedNodeCommand ??= new(CanChangedNodeBody);

        private DefaultCommand appendNodeCommand;
        public DefaultCommand AppendNodeCommand => appendNodeCommand ??= new(AppendNodeBody);

        private DefaultCommand insertNodeCommand;
        public DefaultCommand InsertNodeCommand => insertNodeCommand ??= new(InsertNodeBody, CanInsertNodeBody);

        private DefaultCommand removeNodeCommand;
        public DefaultCommand RemoveNodeCommand => removeNodeCommand ??= new(RemoveNodeBody, CanRemoveNodeBody);

        private DefaultCommand writeTreeCommand;
        public DefaultCommand WriteTreeCommand => writeTreeCommand ??= new(WriteTreeBody, CanWriteTreeBody);

        private DefaultCommand readTreeCommand;
        public DefaultCommand ReadTreeCommand => readTreeCommand ??= new(ReadTreeBody);

        private DefaultCommand exportGraphsCommand;
        public DefaultCommand ExportGraphsCommand => exportGraphsCommand ??= new(ExportGraphsBody);

        #endregion //HeaderCommand

        #region BodyCommand
        public void ReadFileBody(object? parameter)
        {
            try
            {
                if (parameter is IFileReader reader)
                {
                    VistaOpenFileDialog dialog = new()
                    {
                        Multiselect = true,
                    };
                    if (dialog.ShowDialog() == true)
                    {
                        foreach (string fileName in dialog.FileNames)
                        {
                            using (FileStream file = new(fileName, FileMode.Open))
                            {
                                GaussiansModel.PointGraph chart = reader.ReadFile(file);
                                CreateVisualGraphByModel(chart);
                            }
                        }
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
                ViewProperties = metadata.GetSettingGraph(GraphList);
        }

        public void SwapVisibleGraphBody(object? parameter)
        {
            if (parameter is GraphVisualData metadata)
                metadata.IsVisible = !metadata.IsVisible;
        }

        public bool CanInvokeFunctionGraphBody(object? parameter)
        {
            return !IsInvoke;
        }
        public void InvokeFunctionGraphBody(object? parameter)
        {
            InvokeFunctionGraphBodyAsync();
        }
        public async void InvokeFunctionGraphBodyAsync()
        {
            IsInvoke = true;
            TokenSource = new();
            if (SelectNodeTree != null)
            {

                SelectNodeTree.InputContext = new(GraphList.GraphDataList.DistinctBy(i => 
                i.Name)
                    .ToDictionary(i => i.Name, i => new FunctionParameter(i.Name, typeof(IGraph), null) { Value = i.GraphModel }));
                SelectNodeTree.FunctionProgressChanged += SetInvokeFunctionProgress;
                try
                {
                    await Task.Run(() => SelectNodeTree.Invoke(TokenSource.Token));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error");
                    SelectNodeTree.FunctionProgressChanged -= SetInvokeFunctionProgress;
                    IsInvoke = false;
                    return;
                }
                SelectNodeTree.FunctionProgressChanged -= SetInvokeFunctionProgress;
                
                foreach (var function in SelectNodeTree.Functions)
                {
                    if (function.Function is IVisualGraph graph)
                    {
                        var metadates = graph.CreateVisualElements();
                        foreach (var metadata in metadates)
                        {
                            metadata.GraphBrush = new SolidColorBrush(Generator.GetColor());
                            GraphList.AddGraph(metadata);
                        }

                    }
                    else if (function.Function is IVisualMessage message)
                        MessageBox.Show(message.Message);
                }

                TokenSource.Dispose();

                SelectNodeTree.InputContext = new();
            }
            IsInvoke = false;
        }

        public bool CanCancelInvokeFunctionGraphBody(object? parameter)
        {
            return IsInvoke;
        }
        public void CancelInvokeFunctionGraphBody(object? parameter)
        {
            TokenSource.Cancel();
        }

        public void AddNodeTreeBody(object? parameter)
        {
            FunctionNodeTree tree = new();
            tree.Name = Generator.NodeNameGenerator.Next();
            TreesManager.Trees.Add(tree);
            SelectNodeTree = tree;
        }

        public bool CanRemoveNodeTreeBody(object? parameter)
        {
            return SelectNodeTree != null;
        }
        public void RemoveNodeTreeBody(object? parameter)
        {
            if(SelectNodeTree != null && TreesManager.Trees.Contains(SelectNodeTree))
            {
                TreesManager.Trees.Remove(SelectNodeTree);
                SelectNodeTree = null;
            }
        }

        public void CloseGraphBody(object? parameter)
        {
            ViewProperties = null;
        }

        public bool CanChangedNodeBody(object? parameter)
        {
            return SelectNodeTree != null;
        }

        public void AppendNodeBody(object? parameter)
        {
            if (parameter is INodeFunction function && SelectNodeTree != null)
                SelectNodeTree.AddFunction((INodeFunction)function.Clone());
        }
        public bool CanInsertNodeBody(object? parameter)
        {
            return SelectedOperation != null;
        }
        public void InsertNodeBody(object? parameter)
        {
            if (parameter is INodeFunction function && SelectNodeTree != null)
            {
                var functions = SelectNodeTree.Functions;
                if (functions.Contains(SelectedOperation))
                {
                    int index = SelectNodeTree.Functions.IndexOf(SelectedOperation);
                    SelectNodeTree.InsertFunction(function, index);
                }
                else
                    AppendNodeBody(function);
            }
        }
        public bool CanRemoveNodeBody(object? parameter)
        {
            return SelectedOperation != null;
        }
        public void RemoveNodeBody(object? parameter)
        {
            SelectNodeTree.RemoveFunction(SelectedOperation.Function);
        }

        public bool CanWriteTreeBody(object? parameter)
        {
            return SelectNodeTree != null;
        }
        public void WriteTreeBody(object? parameter)
        {
            try
            {

                SaveFileDialog dialog = new()
                {
                    Title = Properties.Resources.WriteNodesOperation,
                    Filter = "All nodes|*.nodes"
                };
                if (dialog.ShowDialog() == true)
                {
                    using (Stream file = dialog.OpenFile())
                    {
                        TreesManager.WriteFile(file, SelectNodeTree);
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Write error");
            }
        }

        public void ReadTreeBody(object? parameter)
        {
            try
            {

                OpenFileDialog dialog = new()
                {
                    Title = Properties.Resources.WriteNodesOperation,
                    Filter = "Nodes|*.nodes"
                };
                if (dialog.ShowDialog() == true)
                {
                    Stream file = dialog.OpenFile();
                    var nodes = TreesManager.ReadFile(file);
                    foreach (var node in nodes.Functions)
                        if (node.Function is ISetGeneratorElements t)
                            t.SetViewModelElements(this);
                    TreesManager.Trees.Add(nodes);
                }
            }
            catch (Exception)
            {

            }
        }

        public void ExportGraphsBody(object parameter)
        {
            ExportGraphsWindow window = new((GraphViewManager)GraphList.Clone());
            window.Owner = TargetWindow;
            window.Show();

        }
        #endregion //BodyCommand

        #endregion //Commands

        #region PrivateMethods

        private void SetInvokeFunctionProgress(object? sender, FunctionProgressEventArgs e)
        {
            InvokeFunctionName = e.FuncName;
            InvokeFunctionProgress = e.Progress;
        }

        private void CreateVisualGraphByModel(IGraph graph)
        {
            GraphVisualData metadata = GraphBuilder.Build(Generator.GraphNameGenerator.Next(), graph);
            metadata.GraphBrush = new SolidColorBrush(Generator.GetColor());
            GraphList.AddGraph(metadata);
            metadata.SetBindings();
        }
        #endregion//PrivateMethods

        #region PrivateProperties
        private CancellationTokenSource TokenSource { get; set; }
        private bool IsInvoke { get; set; }
        public Window TargetWindow { get; set; }
        #endregion //PrivateProperties

        #region InternalProperties
        protected internal SourceGenerator Generator { get; set; }
        protected internal GraphVisualDataBuilder GraphBuilder { get; set; }
        #endregion //InternalProperties

        #region Events
        private void OnPropertyChanged([CallerMemberName] string? prop = null) => PropertyChanged?.Invoke(this, new(prop));
        public event PropertyChangedEventHandler? PropertyChanged;
        #endregion//Events
    }
}
