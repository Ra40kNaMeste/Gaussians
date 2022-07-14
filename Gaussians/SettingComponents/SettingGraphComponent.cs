using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;
using Gaussians.Commands;

namespace Gaussians.SettingComponents
{
    internal class SettingGraphComponent:SettingComponentBase
    {
        private List<SettingGraphPropertyBase> properties = new()
        {
            new SettingGraphName(),
            new SettingGraphColor()
        };
        private List<SettingGraphActionBase> commands = new()
        {
            new SettingGraphRemoved()
        };
        public SettingGraphComponent(GraphVisualData data, GraphViewManager manager)
        {
            Iniz(data, manager);
        }
        private void Iniz(GraphVisualData data, GraphViewManager manager)
        {
            foreach (var item in properties)
            {
                item.SetDates(data);
            }
            SettingProperties = properties;

            foreach (var item in commands)
            {
                item.SetDates(data, manager);
            }
            SettingActions = commands;
        }
    }
    internal abstract class SettingGraphPropertyBase : ISettingComponentProperty
    {
        public abstract void SetDates(GraphVisualData data);
        public abstract string Name { get; }

        public abstract UIElement Value { get; protected set; }

    }
    internal class SettingGraphName : SettingGraphPropertyBase
    {
        public override void SetDates(GraphVisualData data)
        {
            Value = new TextBox();
            BindingOperations.SetBinding(Value, TextBox.TextProperty, new Binding("Name") { Source = data });
        }

        public override string Name => Properties.Resources.SettingNameGraph;

        public override UIElement Value { get; protected set; }
    }
    internal class SettingGraphColor : SettingGraphPropertyBase
    {
        public override void SetDates(GraphVisualData data)
        {
            Value = new ColorPicker();
            ColorPicker colorPicker = new();
            BindingOperations.SetBinding(Value, ColorPicker.SelectedColorProperty, new Binding("GraphBrush") 
            { Source = data, Converter = new SolidColorBrushToColorConverter() });
        }

        public override string Name => Properties.Resources.SettingColorGraph;

        public override UIElement Value { get; protected set; }
    }

    internal class SolidColorBrushToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush brush)
                return brush.Color;
            throw new ArgumentException("value");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color color)
                return new SolidColorBrush(color);
            throw new ArgumentException("value");
        }
    }

    internal abstract class SettingGraphActionBase : IActionCompoentable
    {
        public abstract void SetDates(GraphVisualData data, GraphViewManager manager);
        public abstract string Name { get; }

        public abstract ICommand Command { get; }

        public abstract Brush Brush { get; }
    }
    internal class SettingGraphRemoved : SettingGraphActionBase
    {
        public override string Name => Properties.Resources.SettingRemoveGraph;

        private ICommand command;
        public override ICommand Command => command;

        private SolidColorBrush brush = new(Colors.Red);
        public override Brush Brush => brush;

        public override void SetDates(GraphVisualData data, GraphViewManager manager)
        {
            command = new DefaultCommand(i => RemoveGraph(i, data, manager));
        }
        private void RemoveGraph(object? parameter, GraphVisualData data, GraphViewManager manager)
        {
            data.IsVisible = false;
            manager.RemoveGraph(data);
        }
    }
}
