using GaussiansModel.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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
            IValidationConverter valueConverter = GetConverter();
            Binding binding = new("Value") { Source = property, Converter = valueConverter };
            binding.ValidationRules.Add(new ValidationRuleWithConverter(valueConverter));
            return ShowConverter.Show(binding);
        }

        public bool CanConvert(Type type)
        {
            return type == Type;
        }
    }

    internal interface IDataToViewShowConverter
    {
        public FrameworkElement Show(Binding binding);
    }
    internal class InputDataShow : IDataToViewShowConverter
    {
        public FrameworkElement Show(Binding binding)
        {
            TextBox res = new();
            res.SetBinding(TextBox.TextProperty, binding);
            return res;
        }
    }
    internal class OutputDataShow : IDataToViewShowConverter
    {
        public FrameworkElement Show(Binding binding)
        {
            TextBlock res = new();
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
}
