using System;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;

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
        private void ReadColors()
        {
            string[] colors = Properties.Resources.ColorsForGraphs.Split('\n');
            Colors = new(colors.Select(i => (Color)ColorConverter.ConvertFromString(i.Trim())));
        }

        public NameGenerator GraphNameGenerator { get; set; } = new(Properties.Resources.ReaderGraphName);
        public NameGenerator NodeNameGenerator { get; set; } = new(Properties.Resources.ReaderNodeName);
    }
    internal class NameGenerator
    {
        private int index = -1;
        private string firstName;
        public NameGenerator(string firstName)
        {
            this.firstName = firstName;
        }
        public string Next()
        {
            index++;
            if (index == 0)
                return firstName;
            return firstName + index.ToString();
        }
    }
}
