using Gaussians.Commands;
using GaussiansModel.Functions;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Gaussians.DataConverters
{
    internal class InputSourceDataToViewConverter : IPropertyModelToViewable, INotifyPropertyChanged
    {
        public virtual bool CanConvert(Type type)
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
            textBlock.SetBinding(TextBox.TextProperty, new Binding("FileName") { Source = this }); ;
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
        protected virtual void OpenBody(object? parameter)
        {
            try
            {
                if (((StreamData)TargetParameter.Value).Mode == FileMode.Open)
                {
                    var dialog = new VistaOpenFileDialog();
                    if (dialog.ShowDialog() == true)
                    {
                        FileName = dialog.FileName;
                    }
                }
                else
                {

                    var dialog = new VistaFolderBrowserDialog();
                    if (dialog.ShowDialog() == true)
                    {
                        FileName = dialog.SelectedPath;
                    }
                }
            }
            catch (Exception)
            {

            }

        }
        protected FunctionParameter TargetParameter { get; set; }

        public string FileName
        {
            get
            {
                return ((StreamData)TargetParameter.Value).FileName;
            }
            set
            {
                ((StreamData)TargetParameter.Value).FileName = value;
                OnPropertyChanged();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string prop = "") => PropertyChanged?.Invoke(this, new(prop));
        public event PropertyChangedEventHandler? PropertyChanged;
    }

    internal class InputMultiSourceDataToViewConverter : InputSourceDataToViewConverter
    {
        public override bool CanConvert(Type type)
        {
            return type == typeof(IEnumerable<StreamData>);
        }

        protected override void OpenBody(object? parameter)
        {
            try
            {
                var dialog = new VistaOpenFileDialog()
                {
                    Multiselect = true
                };
                if (dialog.ShowDialog() == true)
                {
                    FileNames = dialog.FileNames;
                }
            }
            catch (Exception)
            {

            }

        }
        public new string FileName
        {
            get
            {
                return string.Empty;

            }
            set
            {

            }
        }
        public IEnumerable<string> FileNames
        {
            get
            {
                return ((IEnumerable<StreamData>)TargetParameter.Value).Select(i => i.FileName);
            }
            set
            {
                TargetParameter.Value = value.Select(i => new StreamData(i, FileMode.Open));
                OnPropertyChanged();
            }
        }
    }

}
