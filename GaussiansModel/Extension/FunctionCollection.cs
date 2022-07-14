using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GaussiansModel.Extension
{
    internal static class FunctionCollection
    {
        /// <summary>
        /// Поиск среднеквадратичного отклонения
        /// </summary>
        /// <param name="graph">Гарфик</param>
        /// <param name="values">Точки</param>
        /// <returns></returns>
        public static double GetErrorValue(Func<double, double> graph, PointGraph values)
        {
            double res = 0;
            foreach (var item in values)
                res += Math.Pow(Math.Abs(Math.Pow(item.Y, 2) - Math.Pow(graph.Invoke(item.X), 2)), 0.5);
            return res;
        }
    }
    /// <summary>
    /// сплайны выда y = A0+A1*X1+A2*X2^2+...+An*Xn^n
    /// </summary>
    internal class BeziersSpline
    {

        public BeziersSpline(IEnumerable<Point> points)
        {
            SetValuesByPoints(points);
        }
        public BeziersSpline(Vector xPoints, Vector yPoints)
        {
            SetValuesByPoints(xPoints, yPoints);
        }

        public void SetValuesByPoints(IEnumerable<Point> points)
        {
            int size = points.Count();
            Matrix matrixX = new(size, size);
            int j;
            for (int i = 0; i < size; i++)
            {
                j = 0;
                foreach (var point in points)
                {
                    matrixX.SetValue(j, i, Math.Pow(point.X, i));
                    j++;
                }
            }
            Vector vectorY = new Vector(size);
            j = 0;
            foreach (var point in points)
            {
                vectorY.SetValue(j, 0, point.Y);
                j++;
            }
            vectorA = Vector.ConvertMatrixToVector(MatrixMath.Multiply(MatrixMath.Invers(matrixX), vectorY));

        }

        public void SetValuesByPoints(Vector xPoints, Vector yPoints)
        {
            int size = xPoints.Rows;
            if (yPoints.Rows != size)
                throw new ArgumentException("yPoints");
            Matrix matrixX = new(size, size);
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    matrixX.SetValue(j, i, Math.Pow(xPoints.GetValue(j,0), i));
                }
            }
            Vector vectorY = new Vector(size);
            for (int i = 0; i < size; i++)
            {
                vectorY.SetValue(i, 0, yPoints.GetValue(i,0));
            }
            vectorA = Vector.ConvertMatrixToVector(MatrixMath.Multiply(MatrixMath.Invers(matrixX), vectorY));

        }


        private Vector vectorA;
        public double GetPoint(double x)
        {
            int size = vectorA.Rows;
            Matrix vector = new Matrix(1, size);
            for (int i = 0; i < size; i++)
            {
                vector.SetValue(0, i, Math.Pow(x, i));
            }
            double res = MatrixMath.Multiply(vector, vectorA).GetValue(0, 0);
            return res;
        }
    }
}
