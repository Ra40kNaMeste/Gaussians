using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Gaussians.DialogWindows.ExportGraphs
{
    /// <summary>
    /// Логика взаимодействия для ExportGraphsWindow.xaml
    /// </summary>
    public partial class ExportGraphsWindow : Window
    {
        internal ExportGraphsWindow(GraphViewManager manager)
        {
            DataContext = new ExportGraphsViewModel(manager);
            InitializeComponent();
        }
    }
}
