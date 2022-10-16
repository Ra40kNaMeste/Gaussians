using GaussiansModel.Functions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xceed.Wpf.Toolkit.Primitives;

namespace Gaussians.DataConverters
{
    internal abstract class ElementaryDataToViewConverterBase : IPropertyModelToViewable
    {
        public ElementaryDataToViewConverterBase(IDataToViewShowConverter converter)
        {
            ShowConverter = converter;
        }
        public IDataToViewShowConverter ShowConverter { get; set; }
        public abstract Type Type { get; }

        protected abstract IValidationConverter GetConverter();
        public FrameworkElement GetView(FunctionParameter property, ViewModel viewModel)
        {
            return ShowConverter.Show(property, viewModel, GetConverter());
        }

        public bool CanConvert(Type type)
        {
            return type == Type;
        }
    }
    internal class ElementaryDataConverter : IValidationConverter
    {
        public ElementaryDataConverter(IValidationConverter nextConverter, List<FunctionNodeBindingData> bindingDates, FunctionNodeBindingData? selectData)
        {
            this.nextConverter = nextConverter;
            this.bindingDates = bindingDates;
            bindingData = selectData;
        }
        public BindingExpression CurrentBinding { get; set; }
        private readonly IValidationConverter nextConverter;
        private List<FunctionNodeBindingData> bindingDates;
        private FunctionNodeBindingData bindingData;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return nextConverter.Convert(value, targetType, parameter, culture);
        }

        private void OnChangedString(object? sender, PropertyChangedEventArgs e)
        {
            CurrentBinding?.UpdateSource();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is StringItem str)
            {
                return nextConverter.ConvertBack(str.Text, targetType, parameter, culture);
            }
            if(value is InputDataBindingItem item)
            {
                return item.Name;
            }
            throw new NotImplementedException();
        }

        public bool CanValidation(object value, CultureInfo cultureInfo)
        {
            if(bindingDates.Contains(bindingData))
                bindingDates.Remove(bindingData);
            if (value is StringItem str)
            {
                str.PropertyChanged -= OnChangedString;
                str.PropertyChanged += OnChangedString;
                return nextConverter.CanValidation(str.Text, cultureInfo);
            }
            if(value is InputDataBindingItem item)
            {
                bindingData = new() { NamePropertyOnContext = item.Name, NameTargetProperty = item.Parameter.Name };
                bindingDates.Add(bindingData);
                return true;
            }
            return false;

        }
    }
    internal interface IDataToViewShowConverter
    {
        public FrameworkElement Show(FunctionParameter property, ViewModel viewModel, IValidationConverter converter);
    }
    internal class InputDataShow : IDataToViewShowConverter
    {
        public FrameworkElement Show(FunctionParameter property, ViewModel viewModel, IValidationConverter converter)
        {
            if (viewModel.SelectedOperation == null)
                return null;
            FunctionNodeContext context = viewModel.SelectedOperation.Context;
            //Формирование списка предложений
            ComboBox box = new();
            box.Style = Application.Current.Resources["ComboBoxWithEditedSelectedItem"] as Style;
            StringItem stringItem = new() { Text = property.Value?.ToString() ?? String.Empty };
            //Получение текущей привязки, если есть
            FunctionNodeBindingData? bindingData = viewModel.SelectedOperation.BindingDates
                .Where(i => i.NameTargetProperty == property.Name).FirstOrDefault();
            object selectItem = null;
            List<object> values = new() { stringItem };

            foreach (var item in context.Context)
            {
                if (FunctionParameter.CanValidation(property.ValueType, item.Value))
                {
                    InputDataBindingItem input = new InputDataBindingItem(item.Key, property);
                    values.Add(input);
                    if (bindingData != null && bindingData.NamePropertyOnContext == item.Key)
                        selectItem = input;
                }
            }
            box.SetBinding(ComboBox.ItemsSourceProperty, new Binding() { Source = values });

            ElementaryDataConverter elementaryConverter = new(converter, viewModel.SelectedOperation.BindingDates, bindingData);
            Binding binding = new Binding("Value") { Source = property, Converter = elementaryConverter, Mode = BindingMode.OneWayToSource };
            binding.ValidationRules.Add(new ValidationRuleWithConverter(elementaryConverter));
            box.SetBinding(ComboBox.SelectedItemProperty, binding);
            elementaryConverter.CurrentBinding = BindingOperations.GetBindingExpression(box, ComboBox.SelectedItemProperty);
            box.ItemTemplateSelector = new ElementaryDataToViewDataTempateSelector();

            box.SelectedItem = selectItem ?? stringItem;

            return box;
        }
    }
    internal class InputDataBindingItem
    {
        public InputDataBindingItem(string name, FunctionParameter parameter)
        {
            Name = name;
            Parameter = parameter;
        }
        public string Name { get; init; }
        public FunctionParameter Parameter { get; init; }
        public override string ToString()
        {
            return Name;
        }
    }
    public class SelectItemToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "";
            if (value is string str)
                if (str == Properties.Resources.NotBindingParameter)
                    return "";
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class StringItem : INotifyPropertyChanged
    {
        private string text = string.Empty;
        public string Text
        {
            get => text;
            set
            {
                text = value;
                PropertyChanged?.Invoke(this, new("Text"));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
    public class ElementaryDataToViewDataTempateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            ContentPresenter presenter = (ContentPresenter)container;
            if (item is StringItem)
            {
                if (presenter.TemplatedParent is ComboBoxItem)
                    return Application.Current.Resources["TextBlockDataTemplate"] as DataTemplate;
                else
                    return Application.Current.Resources["TextBoxDataTemplate"] as DataTemplate;

            }
            return base.SelectTemplate(item, container);
        }
    }
    internal class OutputDataShow : IDataToViewShowConverter
    {
        public FrameworkElement Show(FunctionParameter property, ViewModel viewModel, IValidationConverter converter)
        {
            TextBlock res = new();
            Binding binding = new("Value") { Source = property, Converter = converter };
            res.SetBinding(TextBlock.TextProperty, binding);
            return res;
        }

    }

    internal class DoubleDataToViewConverter : ElementaryDataToViewConverterBase
    {
        public DoubleDataToViewConverter(IDataToViewShowConverter converter) : base(converter) { }
        public override Type Type => typeof(double);

        protected override IValidationConverter GetConverter() => new DoubleConverter();
    }
    internal class IntDataToViewConverter : ElementaryDataToViewConverterBase
    {
        public IntDataToViewConverter(IDataToViewShowConverter converter) : base(converter) { }

        public override Type Type => typeof(int);

        protected override IValidationConverter GetConverter() => new IntConverter();
    }
    internal class StringDataToViewConverter : ElementaryDataToViewConverterBase
    {
        public StringDataToViewConverter(IDataToViewShowConverter converter) : base(converter) { }

        public override Type Type => typeof(string);

        protected override IValidationConverter GetConverter() => new StringConverter();
    }
    internal class BooleanDataToViewConverter : ElementaryDataToViewConverterBase
    {
        public BooleanDataToViewConverter(IDataToViewShowConverter converter) : base(converter) { }

        public override Type Type => typeof(bool);

        protected override IValidationConverter GetConverter() => new BoolConverter();
    }
    internal class ObjectDataToViewConverter : ElementaryDataToViewConverterBase
    {
        public ObjectDataToViewConverter(IDataToViewShowConverter converter) : base(converter) { }

        public override Type Type => typeof(object);

        protected override IValidationConverter GetConverter()=>new ObjectConverter();
    }
}
