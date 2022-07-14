using System;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gaussians
{
    internal class SourceGenerator
    {
        private Queue<Color> Colors { get; set; }
        public SourceGenerator()
        {
            ReadColors();
        }

        public Color GetColor()
        {
            Color color = Colors.Dequeue();
            Colors.Enqueue(color);
            return color;
        }
        private int index = -1;
        public string GetNameGraph()
        {
            index++;
            if (index == 0)
                return Properties.Resources.ReaderGraphName;
            return Properties.Resources.ReaderGraphName + index.ToString();
        }
        private void ReadColors()
        {
            string[] colors = Properties.Resources.ColorsForGraphs.Split('\n');
            Colors = new(colors.Select(i => (Color)ColorConverter.ConvertFromString(i.Trim())));
        }
    }
}
