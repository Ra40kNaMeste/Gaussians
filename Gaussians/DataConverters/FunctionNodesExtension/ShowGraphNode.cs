using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using Gaussians.Properties;
using GaussiansModel;
using GaussiansModel.Functions;
using InteractiveDataDisplay.WPF;
using Newtonsoft.Json;

namespace Gaussians.DataConverters
{
    internal interface ISetGeneratorElements
    {
        public void SetViewModelElements(ViewModel viewModel);
    }

    internal interface IVisualGraph
    {
        public GraphVisualData CreateVisualElements();
    }
    internal interface IVisualMessage
    {
        public string Message { get; set; }
    }

    [JsonObject]
    [ExportGraph]
    internal class ShowGraphNode : NodeFunctionBase<ShowGraphNode>, ISetGeneratorElements, IVisualGraph
    {
        public ShowGraphNode()
        {
            Inputs.Add(new FunctionParameter(Properties.Resources.FunctionParameterGraph, typeof(IGraph), new GaussiansModel.PointGraph()));
        }
        public ShowGraphNode(SourceGenerator generator, GraphVisualDataBuilder builder)
        {
            Inputs.Add(new FunctionParameter(Properties.Resources.FunctionParameterGraph, typeof(IGraph), new GaussiansModel.PointGraph()));

            Generator = generator;
            Builder = builder;
        }
        [JsonIgnore]
        private SourceGenerator Generator { get; set; }
        [JsonIgnore]
        private GraphVisualDataBuilder Builder { get; set; }
        public override string GetName()
        {
            return Properties.Resources.FunctionNodeShowGraphName;
        }
        public void SetViewModelElements(ViewModel viewModel)
        {
            Generator = viewModel.Generator;
            Builder = viewModel.GraphBuilder;
        }

        public override void Invoke(CancellationToken token)
        {
            ResultGraph = (IGraph)FindInputParameter(Resources.FunctionParameterGraph).Value;
        }
        public GraphVisualData CreateVisualElements()
        {
            return Builder.Build(Generator.GraphNameGenerator.Next(), ResultGraph);
        }
        [JsonIgnore]
        public IGraph ResultGraph { get; set; }
        public override object Clone()
        {
            ShowGraphNode res = (ShowGraphNode)base.Clone();
            res.Generator = Generator;
            res.Builder = Builder;
            return res;
        }
    }

    [ExportGraph]
    internal class ShowValueNode : NodeFunctionBase<ShowValueNode>, IVisualMessage
    {
        public ShowValueNode()
        {
            Message = string.Empty;
            Inputs.Add(new(Resources.FunctionNodeValueName, typeof(object), string.Empty));
        }
        public override string GetName()
        {
            return Resources.FunctionNodeShowValueName;
        }

        public override void Invoke(CancellationToken token)
        {
            Message = FindInputParameter(Resources.FunctionNodeValueName).Value.ToString() ?? string.Empty;
        }
        public string Message { get; set; }
    }
}
