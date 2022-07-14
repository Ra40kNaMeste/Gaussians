using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Gaussians.Commands;

namespace Gaussians.SettingComponents
{
    internal abstract class SettingComponentBase
    {
        public SettingComponentBase()
        {
            SettingProperties = new List<ISettingComponentProperty>();
            SettingActions = new List<IActionCompoentable>();
        }
        public IEnumerable<ISettingComponentProperty> SettingProperties { get; protected set; }
        public IEnumerable<IActionCompoentable> SettingActions { get; protected set; }
    }
    public interface ISettingComponentProperty
    {
        public string Name { get; }
        public UIElement Value { get; }
    }
    public interface IActionCompoentable
    {
        public ICommand Command { get; }
        public string Name { get; }
        public Brush Brush { get; }
    }
}
