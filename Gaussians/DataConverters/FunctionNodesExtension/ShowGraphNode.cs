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
        public void SetViewModelElements(SourceGenerator generator);
    }

    internal interface IVisualGraph
    {
        public GraphVisualData ResultGraphMetadata { get; set; }


    }
    [JsonObject]
    [ExportGraph]
    internal class ShowGraphNode : NodeFunctionBase<ShowGraphNode>, ISetGeneratorElements, IVisualGraph
    {
        public ShowGraphNode()
        {
            Inputs.Add(new FunctionParameter(Properties.Resources.FunctionParameterGraph, typeof(IGraph), new GaussiansModel.PointGraph()));
        }
        public ShowGraphNode(SourceGenerator generator)
        {
            Inputs.Add(new FunctionParameter(Properties.Resources.FunctionParameterGraph, typeof(IGraph), new GaussiansModel.PointGraph()));

            Generator = generator;
        }
        [JsonIgnore]
        private SourceGenerator Generator { get; set; }
        public override string GetName()
        {
            return Properties.Resources.FunctionNodeShowGraphName;
        }
        public void SetViewModelElements(SourceGenerator generator)
        {
            Generator = generator;
        }

        public override void Invoke(CancellationToken token)
        {
            var graph = (IGraph)FindInputParameter(Properties.Resources.FunctionParameterGraph).Value;

            ResultGraphMetadata = new(Generator.GraphNameGenerator.Next(), graph, null);
        }
        [JsonIgnore]
        public GraphVisualData ResultGraphMetadata { get; set; }
        public override object Clone()
        {
            ShowGraphNode res = (ShowGraphNode)base.Clone();
            res.Generator = Generator;
            return res;
        }
    }
    //[ExportGraph]
    //internal class ShowValueNode : NodeFunctionBase<ShowValueNode>
    //{
    //    public ShowValueNode()
    //    {
    //        Inputs.Add(new(Resources.FunctionNodeValueName, typeof(object), string.Empty));
    //    }
    //    public override string GetName()
    //    {
    //        return Resources.FunctionNodeShowGraphName;
    //    }

    //    public override void Invoke(CancellationToken token)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
