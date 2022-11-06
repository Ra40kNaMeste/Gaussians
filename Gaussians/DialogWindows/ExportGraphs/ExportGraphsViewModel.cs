using Gaussians.Commands;
using Gaussians.Properties;
using Gaussians.ViewFunctions;
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
using System.Windows.Forms.Design;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Gaussians.DialogWindows
{
    internal class ExportGraphsViewModel:INotifyPropertyChanged
    {

        public ExportGraphsViewModel(GraphViewManager graphsManager)
        {
            GraphsManager = graphsManager;
            Width = 600;
            Height = 400;
            Formats = new List<ExportFormat>()
            {
                ExportFormat.PNG,
                ExportFormat.JPEG,
                ExportFormat.GIF
            };
            FileName = Properties.Resources.ExportChartDefaultName;
            FolderName = Properties.Resources.ExportGraphDefaultFolder;
        }

        private DefaultCommand openFolderCommand;
        public DefaultCommand OpenFolderCommand => openFolderCommand ??= new(OpenFolderBody);
        private DefaultCommand acceptCommand;



        public DefaultCommand AcceptCommand => acceptCommand ??= new(AcceptBody);

        private void OpenFolderBody(object parameter)
        {
            VistaFolderBrowserDialog fileDialog = new();
            if (fileDialog.ShowDialog() == true)
                FolderName = fileDialog.SelectedPath;
        }
        private void AcceptBody(object? parameter)
        {
            FrameworkElement chart = (FrameworkElement)GraphsManager.GraphList.Parent;
            var bitmap = ImageExporter.GetRenderBitmap(chart, new Size(Width, Height), PixelFormats.Default);
            var encoder = ImageExporter.GetEncoder(Format);
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            try
            {
                using (Stream stream = new FileStream(FolderName + "\\" +
                    FileName + ImageExporter.ConvertFormatToString(Format), FileMode.Create))
                    encoder.Save(stream);
                MessageBox.Show(Properties.Resources.ExportGraphSuccessfully);
            }
            catch (Exception)
            {
                MessageBox.Show(Properties.Resources.ExportGraphSaveErrorMessage);
            }
        }

        public GraphViewManager GraphsManager { get; init; }
        private int width;
        public int Width 
        {
            get => width;
            set
            {
                width = value;
                OnPropertyChanged();
            }
        }
        private int height;
        public int Height 
        {
            get => height;
            set
            {
                height = value;
                OnPropertyChanged();
            }
        }
        private string folderName;
        public string FolderName 
        {
            get => folderName;
            private set
            {
                folderName = value;
                OnPropertyChanged();
            }
        }
        private string fileName;
        public string FileName 
        {
            get => fileName;
            set
            {
                fileName = value;
                OnPropertyChanged();
            }
        }
        public IEnumerable<ExportFormat> Formats { get; init; }
        private ExportFormat format;
        public ExportFormat Format 
        {
            get => format;
            set
            {
                format = value;
                OnPropertyChanged();
            }
        }

        private void OnPropertyChanged([CallerMemberName] string prop = "") => PropertyChanged?.Invoke(this, new(prop));
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
