using Gaussians.Commands;
using GaussiansModel.Functions;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Gaussians.DataConverters
{
    internal class InputSourceDataToViewConverter : IPropertyModelToViewable
    {
        public bool CanConvert(Type type)
        {
            return type == typeof(StreamData);
        }

        public FrameworkElement GetView(FunctionParameter property, ViewModel viewModel)
        {
            TargetParameter = property;

            Grid res = new();
            Button button = new();
            button.Content = "...";
            button.Command = OpenCommand;

            TextBlock textBlock = new();
            textBlock.SetBinding(TextBox.TextProperty, "Stream");
            textBlock.MinWidth = 20;

            res.ColumnDefinitions.Add(new());
            res.ColumnDefinitions.Add(new());

            Grid.SetColumn(button, 1);
            res.Children.Add(button);
            res.Children.Add(textBlock);
            return res;
        }
        private DefaultCommand? openCommand;
        public DefaultCommand OpenCommand => openCommand ??= new(OpenBody);
        private void OpenBody(object? parameter)
        {
            try
            {
                if (((StreamData)TargetParameter.Value).Mode == FileMode.Open)
                {
                    var dialog = new OpenFileDialog() { Title = Properties.Resources.MenuOpen };
                    if (dialog.ShowDialog() == true)
                    {
                        Stream = dialog.OpenFile();
                    }
                }
                else
                {
                    var dialog = new SaveFileDialog() { Title = Properties.Resources.MenuSave };
                    if (dialog.ShowDialog() == true)
                    {
                        Stream = dialog.OpenFile();
                    }
                }

 
            }
            catch (Exception)
            {

            }

        }
        private FunctionParameter TargetParameter { get; set; }
        private Stream stream;
        public Stream Stream 
        {
            get { return stream; }
            set 
            {
                stream = value;
                OnStreamChanged();
            }
        }
        private void OnStreamChanged()
        {
            StreamData data = (StreamData)TargetParameter.Value;
            data.Stream = Stream;
        }
    }
}
