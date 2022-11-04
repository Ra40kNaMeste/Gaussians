using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using GaussiansModel.Functions;
using InteractiveDataDisplay.WPF;

namespace Gaussians.DataConverters
{
    internal static class ModelToViewConverter
    {
        public static Point ConvertModelPointToPoint(GaussiansModel.Point point)
        {
            return new(point.X, point.Y);
        }
        public static IEnumerable<Point> ConverModelPointCollectionToPointCollection(IEnumerable<GaussiansModel.Point> points)
        {
            List<Point> res = new();
            foreach (var item in points)
            {
                res.Add(ConvertModelPointToPoint(item));
            }
            return res;
        }
        public static LineGraph CreateVisualGraph(GaussiansModel.IGraph graph, PlotBase rootPlot)
        {
            if (graph is GaussiansModel.PointGraph pointGraph)
            {
                PointGraph res = new PointGraph();
                res.VisualMode = PointGraphState.OnlyPoints;
                res.RadiusPoint = 2;
                res.SelectionPoints = new MultiDeletePointsWithShowAll(new DeleteNoisePotins(20), new DeleteAngleDeviationPoints(0.1))
                {
                    MaxPointsForShowAll = 300
                };
                BindingOperations.SetBinding(res, PointGraph.PointsProperty, new Binding() { Source = pointGraph, Converter = new ModelPointsToPointCollection() });
                return res;
            }
            if (graph is GaussiansModel.FuncGraph funcGraph)
            {
                FuncGraph res = new();

                BindingOperations.SetBinding(res, FuncGraph.MinProperty,
                    new Binding("PlotOriginX") { Source = rootPlot });

                res.TargetFunction = funcGraph.Func;

                BindingOperations.SetBinding(res, FuncGraph.TargetFunctionProperty,
                    new Binding("Func") { Source = funcGraph });

                MultiBinding binding = new() { Converter = new SumDoubleConverter() };
                binding.Bindings.Add(new Binding("PlotOriginX") { Source = rootPlot });
                binding.Bindings.Add(new Binding("PlotWidth") { Source = rootPlot });

                BindingOperations.SetBinding(res, FuncGraph.MaxProperty, binding);
                return res;
            }
            throw new ArgumentException("graph");
        }

        public static GaussiansModel.IGraph FindModel(PlotBase plot)
        {
            BindingExpression? expression = null;
            if (plot is PointGraph pointGraph)
                expression = pointGraph.GetBindingExpression(LineGraph.PointsProperty);
            else if (plot is FuncGraph funcGraph)
                expression = funcGraph.GetBindingExpression(FuncGraph.TargetFunctionProperty);
            if (expression == null)
                throw new ArgumentException("plot");
            return (GaussiansModel.IGraph)expression.DataItem;
        }

        public static Type ConvertTypeViewGraphToTypeModelGraph(Type modelGaph)
        {
            var temp = modelGraphByView.Where(i => i.Key == modelGaph || modelGaph.IsSubclassOf(i.Key));
            if (temp.Count() == 0)
                throw new ArgumentException("modelGraph");
            var res = temp.First().Value;
            return res;
        }

        private static Dictionary<Type, Type> modelGraphByView = new()
        {
            {typeof(FuncGraph), typeof(GaussiansModel.FuncGraph) },
            {typeof(PointGraph), typeof(GaussiansModel.PointGraph) }
        };
    }
    internal class SumDoubleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double sum = 0;
            foreach (var item in values)
                if (item is double temp)
                    sum += temp;
            return sum;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    internal class ModelPointsToPointCollection : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<GaussiansModel.Point> points)
                return new PointCollection(ModelToViewConverter.ConverModelPointCollectionToPointCollection(points));
            throw new ArgumentException("value");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class GraphNameToGraphModelConverter : IValidationConverter
    {
        public GraphNameToGraphModelConverter(GraphManager manager)
        {
            Manager = manager;
        }
        public GraphManager Manager { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                throw new ArgumentException("value");
            if (value is GraphData data)
                return data.Name;
            throw new ArgumentException("value");
        }

        public bool CanValidation(object value, CultureInfo cultureInfo)
        {
            return value != null && value is GraphData;
        }
    }

    internal interface IPropertyModelToViewable
    {
        public bool CanConvert(Type type);
        public FrameworkElement GetView(FunctionParameter property, ViewModel viewModel);
    }
    internal class InputPropertyModelToVisualConverter : IValueConverter
    {
        public InputPropertyModelToVisualConverter()
        {
        }
        public InputPropertyModelToVisualConverter(ViewModel viewModel)
        {
            ViewModel = viewModel;
        }
        public ViewModel ViewModel { get; set; }

        private static List<IPropertyModelToViewable> converters = new()
        {
            new DoubleDataToViewConverter(new InputDataShow()),
            new IntDataToViewConverter(new InputDataShow()),
            new StringDataToViewConverter(new InputDataShow()),
            new BooleanDataToViewConverter(new InputDataShow()),
            new ObjectDataToViewConverter(new InputDataShow()),
            new InputGraphDataToViewConverter(),
            new InputPointGraphsDataToViewConverter(),
            new InputMultiSourceDataToViewConverter(),
            new InputSourceDataToViewConverter()
        };
        public static PropertyNodeFuncView ConvertPropertyModelToVisualProperty(FunctionParameter property, ViewModel viewModel)
        {
            foreach (var converter in converters)
                if (converter.CanConvert(property.ValueType))
                    return new(property.Name, converter.GetView(property, viewModel));
            throw new Exception();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FunctionParameter par && ViewModel != null)
                return ConvertPropertyModelToVisualProperty(par, ViewModel).Value;
            throw new ArgumentException("value");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    internal class OutputPropertyModelToVisualConverter : IValueConverter
    {
        public OutputPropertyModelToVisualConverter()
        {
        }
        public OutputPropertyModelToVisualConverter(ViewModel viewModel)
        {
            ViewModel = viewModel;
        }
        public ViewModel? ViewModel { get; set; }

        private static List<IPropertyModelToViewable> converters = new()
        {
            new DefaultOutputDataShow()
        };
        public static PropertyNodeFuncView ConvertPropertyModelToVisualProperty(FunctionParameter property, ViewModel viewModel)
        {
            foreach (var converter in converters)
                if (converter.CanConvert(property.ValueType))
                    return new(property.Name, converter.GetView(property, viewModel));
            throw new Exception();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FunctionParameter par && ViewModel != null)
                return ConvertPropertyModelToVisualProperty(par, ViewModel).Value;
            throw new ArgumentException("value");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class PropertyNodeFuncView
    {
        public PropertyNodeFuncView(string name, FrameworkElement value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; init; }
        public FrameworkElement Value { get; init; }
    }

    internal class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Hidden : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    internal class DefaultOutputDataShow : IPropertyModelToViewable
    {
        public bool CanConvert(Type type)
        {
            return true;
        }

        public FrameworkElement GetView(FunctionParameter property, ViewModel viewModel)
        {

            TextBlock res = new();
            res.SetBinding(TextBlock.TextProperty, new Binding("Name") { Source = property });
            return res;
        }
    }
}
