using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using GaussiansModel;
using GaussiansModel.Functions;
using InteractiveDataDisplay.WPF;

namespace Gaussians.DataConverters
{
    internal class ShowGraphNode : NodeFunctionBase<ShowGraphNode>
    {
        public ShowGraphNode()
        {
            Inputs = new List<FunctionParameter>()
            {
                new FunctionParameter(Properties.Resources.FunctionParameterGraph, typeof(IGraph), new GaussiansModel.PointGraph())
            };
        }
        public ShowGraphNode(GraphViewManager manager, SourceGenerator generator)
        {
            Inputs = new List<FunctionParameter>()
            {
                new FunctionParameter(Properties.Resources.FunctionParameterGraph, typeof(IGraph), null)
            };
            Manager = manager;
            Generator = generator;
        }
        private GraphViewManager Manager { get; set; }
        private SourceGenerator Generator { get; set; }
        public override string GetName()
        {
            return Properties.Resources.FunctionNodeShowGraphName;
        }

        public override void Invoke()
        {
            var graph = (IGraph)FindInputParameter(Properties.Resources.FunctionParameterGraph).Value;

            GraphVisualData metadata = new(Generator.GetNameGraph(), graph, new SolidColorBrush(Generator.GetColor()));
            Manager.AddGraph(metadata);
            BindingOperations.SetBinding(metadata.Graph, LineGraph.StrokeProperty, new Binding("GraphBrush") { Source = metadata, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
        }
        public override object Clone()
        {
            ShowGraphNode res = (ShowGraphNode)base.Clone();
            res.Manager = Manager;
            res.Generator = Generator;
            return res;
        }
    }
}
