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
using Xceed.Wpf.Toolkit.Primitives;

namespace Gaussians.DataConverters
{

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
    internal class InputPointGraphsDataToViewConverter : IPropertyModelToViewable
    {
        public bool CanConvert(Type type)
        {
            return typeof(IEnumerable<GaussiansModel.PointGraph>).IsAssignableFrom(type);
        }

        public FrameworkElement GetView(FunctionParameter parameter, ViewModel viewModel)
        {
            ComboBox box = new ComboBox();
            if (viewModel.SelectedOperation == null)
                return box;
            box.MinWidth = 50;

            var context = viewModel.SelectedOperation.Context;
            //Получение данных о привязке сначала для графиков из нодов
            FunctionNodeBindingData? bindingData =
                viewModel.SelectedOperation.BindingDates.Where(i => i.NameTargetProperty == parameter.Name).FirstOrDefault();

            InputDataBindingItem? selectItem = null;
            //Сбор элементов для выюора
            List<object> values = new();

            foreach (var item in context.Context)
            {
                if (FunctionParameter.CanValidation(parameter.ValueType, item.Value))
                {
                    InputDataBindingItem input = new InputDataBindingItem(item.Key, parameter);
                    values.Add(input);
                    if (bindingData != null && bindingData.NamePropertyOnContext == item.Key)
                        selectItem = input;
                }
            }
            //Установка элементов
            box.SetBinding(ComboBox.ItemsSourceProperty, new Binding() { Source = values });

            PointGraphsDataConverter converter = new();
            ElementaryDataConverter elementaryConverter = new(converter, viewModel.SelectedOperation.BindingDates, bindingData);
            Binding binding = new Binding("Value") { Source = parameter, Converter = elementaryConverter, Mode = BindingMode.OneWayToSource };
            binding.ValidationRules.Add(new ValidationRuleWithConverter(elementaryConverter));
            box.SetBinding(ComboBox.SelectedItemProperty, binding);
            elementaryConverter.CurrentBinding = BindingOperations.GetBindingExpression(box, ComboBox.SelectedItemProperty);
            box.ItemTemplateSelector = new ElementaryDataToViewDataTempateSelector();
            box.SelectedItem = selectItem;
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

    internal class PointGraphsDataConverter : IValidationConverter
    {
        public bool CanValidation(object value, CultureInfo cultureInfo)
        {
            return true;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

}
