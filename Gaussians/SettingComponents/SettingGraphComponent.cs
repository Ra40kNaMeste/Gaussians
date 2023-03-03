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
using Gaussians.Properties;
using InteractiveDataDisplay.WPF;

namespace Gaussians.SettingComponents
{
    internal class SettingGraphComponent : SettingComponentBase
    {
        public SettingGraphComponent(GraphVisualData data, GraphViewManager manager, List<SettingGraphPropertyBase> properties, List<SettingGraphActionBase> commands)
        {
            Iniz(data, manager, properties, commands);
        }
        private void Iniz(GraphVisualData data, GraphViewManager manager, List<SettingGraphPropertyBase> properties, List<SettingGraphActionBase> commands)
        {
            foreach (var item in properties)
            {
                item.SetDates(data);
            }
            SettingProperties = properties;

            foreach (var item in commands)
            {
                item.SetDates(data, manager, OnRemoveEvent);
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

    internal class SettingGraphVisualMode : SettingGraphPropertyBase
    {
        public override string Name => Resources.SettingViewModeProperty;

        public override UIElement Value { get; protected set; }
        public override void SetDates(GraphVisualData data)
        {
            PointGraphVisualData pointsData = (PointGraphVisualData)data;
            ComboBox res = new();
            foreach (var item in VisualModeConverter.Names)
                res.Items.Add(item.Value);

            res.SelectedItem = VisualModeConverter.Names[pointsData.VisualMode];
            Binding binding = new("VisualMode") { Source = data, Converter = new VisualModeConverter(), Mode = BindingMode.TwoWay };
            res.SetBinding(ComboBox.SelectedItemProperty, binding);
            Value = res;
        }
    }
    internal class VisualModeConverter : IValueConverter
    {
        protected internal static readonly Dictionary<PointGraphState, string> Names = new()
        {
            {PointGraphState.OnlyPoints, Resources.SettingVisualModeOnlyPoints },
            {PointGraphState.OnlyLine, Resources.SettingVisualModeOnlyLine },
            {PointGraphState.PointsAndLine, Resources.SettingVisualModePointsAndLine }
        };
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Names[(PointGraphState)value];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string name = value.ToString();
            return Names.Where(i => i.Value == name).First().Key;
        }
    }

    internal abstract class SettingGraphActionBase : IActionCompoentable
    {
        public abstract void SetDates(GraphVisualData data, GraphViewManager manager, Action<GraphVisualData> removeAction);
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

        public override void SetDates(GraphVisualData data, GraphViewManager manager, Action<GraphVisualData> removeAction)
        {
            command = new DefaultCommand(i => RemoveGraph(i, data, manager, removeAction));
        }
        private void RemoveGraph(object? parameter, GraphVisualData data, GraphViewManager manager, Action<GraphVisualData> removeAction)
        {
            data.IsVisible = false;
            manager.RemoveGraph(data);
            removeAction?.Invoke(data);
        }
    }
}
