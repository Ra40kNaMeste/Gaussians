using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GaussiansModel.Extension
{
    public static class NonlinearSolver
    {
        private static Vector GetGradient(Func<Vector, double> func, Vector point, double delta)
        {
            int rows = point.Rows;
            Vector res = (Vector)point.Clone();
            ParallelLoopResult result = Parallel.For(0, rows, (row) =>
            {
                Vector temp = (Vector)point.Clone();
                temp.SetValue(row, 0, temp.GetValue(row, 0) + delta);
                res.SetValue(row, 0, func.Invoke(temp) - func.Invoke(point));
                temp.SetValue(row, 0, temp.GetValue(row, 0) - delta);
            });
            if(result.IsCompleted)
                return res;
            throw new Exception();
        }
        public static Vector Minimize(Func<Vector, double> func, Vector startPoint, double firstStep, double stepChangedStep, double maxAngle, int maxSteps)
        {
            try
            {
                double step = firstStep;
                double cur, next;
                int i = 0;
                do
                {
                    cur = func.Invoke(startPoint);
                    if (i > maxSteps)
                        return startPoint;
                    Vector grad = GetGradient(func, startPoint, step / 100);
                    startPoint = (Vector)MatrixMath.Sum(startPoint, MatrixMath.Multiply(MatrixMath.Multiply(grad, -1/ MatrixMath.Modul(grad)), step));
                    if (i>180)
                         i = 0;
                    next = func.Invoke(startPoint);
                    step = step * stepChangedStep;
                    i++;
                } while (Math.Abs(next - cur)/step > maxAngle);
                return startPoint;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("func");
            }

        }

        public static double Derivative(Func<double, double> func, double x, double delta)
        {
            return (func(x + delta) - func(x)) / delta;
        }
        public static double FindMinimumByNewton(Func<double, double> func, double x, double error)
        {
            double oldX = x;
            x = x - Derivative(func, x, error / 20) / Derivative((x) => Derivative(func, x, error / 20), x, error / 20);
            while (Math.Abs(oldX-x)>error)
            {
                oldX = x;
                x = x - Derivative(func, x, error / 20) / Derivative((x) => Derivative(func, x, error / 20), x, error / 20);
            }
            return x;
        }
        public static double FindMaximumByNewton(Func<double, double> func, double x, double error)
        {
            double oldX = x;
            x = x + Derivative(func, x, error / 20) / Derivative((x) => Derivative(func, x, error / 20), x, error / 20);
            while (Math.Abs(oldX - x) > error)
            {
                oldX = x;
                x = x + Derivative(func, x, error / 20) / Derivative((x) => Derivative(func, x, error / 20), x, error / 20);
            }
            return x;
        }
        public static double FindExtremum(Func<double, double> func, double min, double max, double error)
        {
            double derMin = Derivative(func, min, error / 100);
            double derMax = Derivative(func, max, error / 100);
            if (derMin * derMax > 0)
                throw new ArgumentException("range");
            while (Math.Abs(max-min)>error)
            {
                double cur = (max + min) / 2;
                double derCur = Derivative(func, cur, error / 10);
                if (derCur * derMin > 0)
                    min = cur;
                else
                    max = cur;
            }
            return Math.Abs(max + min) / 2;
        }
    }
}
