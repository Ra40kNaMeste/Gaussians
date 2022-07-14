using GaussiansModel.Extension;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GaussiansModel
{
    /// <summary>
    /// Одинокая точка в двумерном бренном пространстве
    /// </summary>
    public struct Point
    {
        public Point()
        {
            X = 0;
            Y = 0;
        }
        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }
        public double X;
        public double Y;
    }

    /// <summary>
    /// График
    /// </summary>
    public interface IGraph
    {
        /// <summary>
        /// Значение y от координаты x
        /// </summary>
        /// <param name="x">координата</param>
        /// <returns>Значение y</returns>
        public double GetValue(double x);
    }
    /// <summary>
    /// График из точек
    /// </summary>
    public class PointGraph : ObservableCollection<Point>, IGraph
    {
        public PointGraph() : base()
        {

        }
        public PointGraph(IEnumerable<Point> points) : base(points)
        {

        }
        public PointGraph(ICollection<Point> points) : base(points)
        {

        }
        /// <summary>
        /// Отсортировать по x
        /// </summary>
        /// <returns>Отсортированный график</returns>
        public PointGraph SortByX() => new(this.OrderBy(i => i.X));
        /// <summary>
        /// Значение y от координаты x
        /// </summary>
        /// <param name="x">Координата</param>
        /// <returns>y</returns>
        public double GetValue(double x)
        {

            //Определяем случаи при 0 и 1 точке
            var en = GetEnumerator();
            if (!en.MoveNext())
                return 0;
            Point point = en.Current;
            if (!en.MoveNext())
                return point.Y;
            Point newPoint = en.Current;
            //тангенс наклона первых 2-х точек
            double tan = MathPoint.Tan(point, newPoint);
            if (!(x <= newPoint.X))
            {
                while (en.Current.X < x && en.MoveNext())
                {
                    point = newPoint;
                    newPoint = en.Current;
                    tan = MathPoint.Tan(point, newPoint);
                }
            }


            //определение точки, если бы она лежала на прямой между 2-мя начениями x заданной точки 
            return (x - point.X) * tan + point.Y;
        }
    }
    /// <summary>
    /// График-функция
    /// </summary>
    public class FuncGraph : IGraph
    {
        /// <summary>
        /// Функция
        /// </summary>
        public Func<double, double>? Func { get; set; }
        /// <summary>
        /// Значение y от координаты x
        /// </summary>
        /// <param name="x">Координата</param>
        /// <returns>y</returns>
        public double GetValue(double x)
        {
            return Func(x);
        }
    }
    public class MultiGraph : FuncGraph
    {
        public MultiGraph()
        {
            Graphs = new List<MultiGraphItem>();
            Func = GetFunctionValue;
        }
        public ICollection<MultiGraphItem> Graphs { get; set; }
        private double GetFunctionValue(double x)
        {
            if (Graphs.Count != 0 && Graphs.ElementAt(0).MinX > x)
                return 0;
            MultiGraphItem? old = null;
            foreach (var item in Graphs)
            {
                if (item.MaxX >= x && (item.MinX <= x || old != null && old.MaxX <= x))
                    return item.Graph.GetValue(x);
                old = item;
            }
            return 0;
        }
    }
    public class MultiGraphItem
    {
        public MultiGraphItem(double minX, double maxX, IGraph graph)
        {
            MinX = minX;
            MaxX = maxX;
            Graph = graph;
        }
        public MultiGraphItem()
        {
            MinX = 0;
            MaxX = 0;
        }
        public double MinX { get; set; }
        public double MaxX { get; set; }
        public IGraph Graph { get; set; }
    }
}
