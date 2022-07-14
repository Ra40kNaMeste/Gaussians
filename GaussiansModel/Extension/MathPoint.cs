using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GaussiansModel.Extension
{
    internal static class MathPoint
    {
        /// <summary>
        /// Тангенс между линией по 2 точками и осью ох
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns></returns>
        public static double Tan(Point point1, Point point2)
        {
            return (point2.Y - point1.Y)/(point2.X - point1.X);
        }
        /// <summary>
        /// Дистанция между двумя точками
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns></returns>
        public static double Distance(Point point1, Point point2)
        {
            double X = point2.X - point1.X;
            double Y = point2.Y - point1.Y;
            return Math.Sqrt(X*X + Y*Y);
        }
        /// <summary>
        /// Усреднение массива точек
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static Point CenterPoint(params Point[] points)
        {
            Point point = new Point();
            foreach (var item in points)
            {
                point.X += item.X;
                point.Y += item.Y;
            }
            point.X /= points.Length;
            point.Y /= points.Length;
            return point;
        }
    }
}
