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
            res.SetBinding(TextBlock.TextProperty, new Binding("Name") { Source = parameter, });
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
            if (viewModel.SelectedOperation == null)
                return box;
            box.MinWidth = 50;
            //Менеджер, состоящий из контекста и графиков
            UnionGraphListManager manager = new(viewModel.GraphList.GraphDataList, GraphDataListBuilder.ConvertToManager(viewModel.SelectedOperation.Context, parameter.ValueType).GraphDataList);
            //Установка элементов
            box.SetBinding(ItemsControl.ItemsSourceProperty, new Binding("GraphDataList") { Source = manager });
            //Получение данных о привязке сначала для графиков из нодов, а затем для графиков приложения
            FunctionNodeBindingData? bindingData =
                viewModel.SelectedOperation.BindingDates.Where(i => i.NameTargetProperty == parameter.Name).FirstOrDefault();
            if (bindingData == null)
            {
                bindingData = new FunctionNodeBindingData()
                {
                    NameTargetProperty = parameter.Name
                };
                viewModel.SelectedOperation.BindingDates.Add(bindingData);
            }
            //Установка привязки на существующей привязке
            string namePropertyOnContext = bindingData.NamePropertyOnContext;
            if (namePropertyOnContext == null && box.Items.Count > 0)
            {
                namePropertyOnContext = ((GraphData)box.Items[0]).Name;
                bindingData.NamePropertyOnContext = namePropertyOnContext;
            }
            Binding binding = new("NamePropertyOnContext")
            {
                Source = bindingData,
                Converter = new GraphNameToGraphModelConverter(manager),
                Mode = BindingMode.OneWayToSource
            };
            binding.ValidationRules.Add(new ValidationRuleWithConverter(new GraphNameToGraphModelConverter(manager)));
            box.SetBinding(ComboBox.SelectedValueProperty, binding);





            if (box.Items.Count > 0)
                box.SelectedItem = manager.GraphDataList.Where(i => i.Name == bindingData.NamePropertyOnContext).FirstOrDefault();

            return box;
        }
    }

    internal static class GraphDataListBuilder
    {
        public static GraphViewManager ConvertToManager(FunctionNodeContext context, Type type)
        {
            GraphViewManager manager = new();
            foreach (var item in context.Context)
            {
                if (FunctionParameter.CanValidation(type, item.Value))
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
