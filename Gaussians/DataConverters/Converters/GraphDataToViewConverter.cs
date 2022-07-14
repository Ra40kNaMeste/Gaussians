using GaussiansModel.Functions;
using InteractiveDataDisplay.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Gaussians.DataConverters
{
    internal class OutputGraphDataToViewConverter : IPropertyModelToViewable
    {
        public bool CanConvert(Type type)
        {
            return typeof(GaussiansModel.IGraph).IsAssignableFrom(type);
        }

        public FrameworkElement GetView(FunctionParameter parameter, ViewModel viewModel)
        {
            TextBlock res = new();
            res.SetBinding(TextBlock.TextProperty, new Binding("Name") { Source = parameter });
            return res;
        }
    }


    internal class InputGraphDataToViewConverter : IPropertyModelToViewable
    {
        public bool CanConvert(Type type)
        {
            return typeof(GaussiansModel.IGraph).IsAssignableFrom(type);
        }

        public FrameworkElement GetView(FunctionParameter parameter, ViewModel viewModel)
        {
            ComboBox box = new ComboBox();

            UnionGraphListManager manager = new(viewModel.GraphList.GraphDataList, GraphDataListBuilder.ConvertToManager(viewModel.SelectedOperation.Context).GraphDataList);

            box.SetBinding(ItemsControl.ItemsSourceProperty, new Binding("GraphDataList") { Source = manager });

            FunctionNodeBindingData binding =
                viewModel.SelectedOperation.BindingDates.Where(i => i.NameTargetProperty == parameter.Name).FirstOrDefault();
            if (binding == null)
            {
                binding =new FunctionNodeBindingData()
                {
                    NameTargetProperty = parameter.Name
                };
                viewModel.SelectedOperation.BindingDates.Add(binding);
            }
            string namePropertyOnContext = binding.NamePropertyOnContext;
            if (namePropertyOnContext == null && box.Items.Count > 0)
                namePropertyOnContext = ((GraphData)box.Items[0]).Name;

            box.SetBinding(ComboBox.SelectedValueProperty, new Binding("NamePropertyOnContext")
            { Source = binding, Converter = new GraphNameToGraphModelConverter(manager), Mode = BindingMode.OneWayToSource });


            binding.NamePropertyOnContext = namePropertyOnContext;


            if (box.Items.Count > 0)
                box.SelectedItem = manager.GraphDataList.Where(i => i.Name == binding.NamePropertyOnContext).FirstOrDefault();

            return box;
        }
    }

    internal static class GraphDataListBuilder
    {
        public static GraphViewManager ConvertToManager(FunctionNodeContext context)
        {
            GraphViewManager manager = new();
            foreach (var item in context.Context)
            {
                if (typeof(GaussiansModel.IGraph).IsInstanceOfType(item.Value.Value))
                    manager.AddGraph(new GraphData(item.Key, (GaussiansModel.IGraph)item.Value.Value));
            }
            
            return manager;
        }
    }

    internal class GraphdataConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ObservableCollection<GraphVisualData> dates)
                return dates.Select(i => i.Name);
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
