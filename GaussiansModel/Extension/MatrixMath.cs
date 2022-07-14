using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GaussiansModel.Extension
{
    public interface IMatrixable : ICloneable
    {
        public double GetValue(int row, int column);
        public void SetValue(int row, int column, double value);
        public int GetRows();
        public int GetColumns();
    }

    public class Vector : IMatrixable
    {
        private double[] values;
        public int Rows { get; init; }

        public static Vector ConvertMatrixToVector(IMatrixable matrix)
        {
            int rows = matrix.GetRows(), columns = matrix.GetColumns();
            Vector res = new(rows * columns);
            for (int i = 0; i < columns; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    res.SetValue(i * rows + j, 0, matrix.GetValue(j, i));
                }
            }
            return res;
        }
        public Vector(int rows)
        {
            Rows = rows;
            values = new double[rows];
        }

        public double this[int row]
        {
            get
            {
                if (!CanVeritificationIndexes(row))
                    throw new IndexOutOfRangeException();
                return values[row];
            }
            set
            {
                if (!CanVeritificationIndexes(row))
                    throw new IndexOutOfRangeException();
                values[row] = value;
            }
        }
        private bool CanVeritificationIndexes(int row)
        {
            return row >= 0 && row < Rows;
        }

        public double GetValue(int row, int column)
        {
            if (column != 0)
                throw new IndexOutOfRangeException();
            return this[row];
        }

        public void SetValue(int row, int column, double value)
        {
            if (column != 0)
                throw new IndexOutOfRangeException();
            this[row] = (double)value;
        }

        public int GetRows() => Rows;
        public int GetColumns() => 1;
        public object Clone()
        {
            Vector res = new(Rows);
            for (int i = 0; i < Rows; i++)
                res[i] = this[i];
            return res;
        }
    }
    public class Matrix : IMatrixable
    {
        public Matrix(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
            values = new double[rows, columns];
        }
        private double[,] values;

        public int Rows { get; init; }
        public int Columns { get; init; }

        public double this[int row, int column]
        {
            get
            {
                if (!CanVeritificationIndexes(row, column))
                    throw new IndexOutOfRangeException();
                return values[row, column];
            }
            set
            {
                if (!CanVeritificationIndexes(row, column))
                    throw new IndexOutOfRangeException();
                values[row, column] = value;
            }
        }
        private bool CanVeritificationIndexes(int row, int column)
        {
            return row >= 0 && row < Rows && column >= 0 && column < Columns;
        }

        public double GetValue(int row, int column)
        {
            return this[row, column];
        }

        public void SetValue(int row, int column, double value)
        {
            this[row, column] = (double)value;
        }
        public int GetRows() => Rows;
        public int GetColumns() => Columns;
        public object Clone()
        {
            Matrix res = new(Rows, Columns);
            for (int row = 0; row < Rows; row++)
                for (int column = 0; column < Columns; column++)
                    res[row, column] = this[row, column];
            return res;
        }
    }
    public static class MatrixMath
    {

        public static IMatrixable Sum(params IMatrixable[] matrixes)
        {
            if (matrixes.Length == 0)
                return null;
            IMatrixable first = matrixes[0];
            int rows = first.GetRows(), columns = first.GetColumns();
            if (!(matrixes.All(i => i.GetRows() == rows && i.GetColumns() == columns)))
                throw new ArgumentException("matrixes");
            IMatrixable res = (IMatrixable)first.Clone();
            for (int row = 0; row < rows; row++)
                for (int column = 0; column < columns; column++)
                {
                    double val = 0;
                    foreach (var item in matrixes)
                    {
                        val += item.GetValue(row, column);
                    }
                    res.SetValue(row, column, val);
                }
            return res;
        }
        public static IMatrixable Sum(IMatrixable matrix1, double value)
        {
            int rows = matrix1.GetRows(), columns = matrix1.GetColumns();
            IMatrixable res = (IMatrixable)matrix1.Clone();
            for (int row = 0; row < rows; row++)
                for (int column = 0; column < columns; column++)
                    res.SetValue(row, column, matrix1.GetValue(row, column) + value);
            return res;
        }


        public static IMatrixable Sub(IMatrixable matrix1, IMatrixable matrix2)
        {
            int rows = matrix1.GetRows(), columns = matrix1.GetColumns();
            if (matrix2.GetRows() != rows || matrix2.GetColumns() != columns)
                throw new ArgumentException("matrixes");
            IMatrixable res = (IMatrixable)matrix1.Clone();
            for (int row = 0; row < rows; row++)
                for (int column = 0; column < columns; column++)
                    res.SetValue(row, column, matrix1.GetValue(row, column) - matrix2.GetValue(row, column));
            return res;
        }

        public static IMatrixable Sub(IMatrixable matrix1, double value)
        {
            int rows = matrix1.GetRows(), columns = matrix1.GetColumns();
            IMatrixable res = (IMatrixable)matrix1.Clone();
            for (int row = 0; row < rows; row++)
                for (int column = 0; column < columns; column++)
                    res.SetValue(row, column, matrix1.GetValue(row, column) - value);
            return res;
        }

        public static IMatrixable Multiply(IMatrixable matrix1, IMatrixable matrix2)
        {
            int rowsRes = matrix1.GetRows(), columnsRes = matrix2.GetColumns(), culumns1 = matrix1.GetColumns();
            if (culumns1 != matrix2.GetRows())
                throw new ArgumentException("matrixes");
            IMatrixable res = new Matrix(rowsRes, columnsRes);
            for (int rowRes = 0; rowRes < rowsRes; rowRes++)
                for (int colRes = 0; colRes < columnsRes; colRes++)
                {
                    double val = 0;
                    for (int column = 0; column < culumns1; column++)
                    {
                        val += matrix1.GetValue(rowRes, column) * matrix2.GetValue(column, colRes);
                    }
                    res.SetValue(rowRes, colRes, val);
                }
            return res;
        }

        public static IMatrixable Multiply(IMatrixable matrix1, double value)
        {
            int rows = matrix1.GetRows(), columns = matrix1.GetColumns();
            IMatrixable res = (IMatrixable)matrix1.Clone();
            for (int row = 0; row < rows; row++)
                for (int column = 0; column < columns; column++)
                    res.SetValue(row, column, matrix1.GetValue(row, column) * value);
            return res;
        }

        public static IMatrixable Transpose(IMatrixable matrix)
        {
            int rows = matrix.GetRows(), columns = matrix.GetColumns();
            IMatrixable res = new Matrix(columns, rows);
            for (int row = 0; row < columns; row++)
                for (int column = 0; column < rows; column++)
                    res.SetValue(row, column, matrix.GetValue(column, row));
            return res;
        }

        public static double GetDet(IMatrixable matrix)
        {
            int rows = matrix.GetRows();
            if (rows != matrix.GetColumns())
                throw new ArgumentException("matrix");
            if (rows == 1)
                return matrix.GetValue(0, 0);
            if (rows == 2)
                return matrix.GetValue(0, 0) * matrix.GetValue(1, 1) - matrix.GetValue(0, 1) * matrix.GetValue(1, 0);
            double res = 0;
            if (rows == 3)
            {
                for (int row = 0; row < rows; row++)
                {
                    double temp = 1;
                    for (int column = 0; column < rows; column++)
                        temp *= matrix.GetValue(column, (row + column) % rows);
                    res += temp;
                    temp = 1;
                    for (int column = 0; column < rows; column++)
                        temp *= matrix.GetValue(column, (rows + row - column) % rows);
                    res -= temp;
                }
                return res;
            }

            List<double> doubles = new();

            ParallelLoopResult result = Parallel.For(0, rows, (row) =>
            {
                doubles.Add((-(row % 2) * 2 + 1) * matrix.GetValue(row, 0) * GetDet(Minor(matrix, row, 0)));
            });


            if (result.IsCompleted)
                return doubles.Sum();
            throw new Exception();
        }
        public static IMatrixable Minor(IMatrixable matrix, int row, int column)
        {
            int corrColumnRes, corrRowRes = 0;
            int rows = matrix.GetRows(), columns = matrix.GetColumns();
            IMatrixable res = new Matrix(rows - 1, columns - 1);
            for (int rowRes = 0; rowRes < rows; rowRes++)
            {
                if (rowRes == row)
                {
                    corrRowRes = -1;
                    continue;
                }
                corrColumnRes = 0;
                for (int columnRes = 0; columnRes < columns; columnRes++)
                {
                    if (columnRes == column)
                    {
                        corrColumnRes = -1;
                        continue;
                    }
                    res.SetValue(rowRes + corrRowRes, columnRes + corrColumnRes, matrix.GetValue(rowRes, columnRes));
                }
            }
            return res;
        }
        public static IMatrixable Invers(IMatrixable matrix)
        {
            int rows = matrix.GetRows();
            if (rows != matrix.GetColumns())
                throw new ArgumentException("matrix");
            IMatrixable res = (IMatrixable)matrix.Clone();
            double det = GetDet(matrix);
            for (int rowRes = 0; rowRes < rows; rowRes++)
                for (int columnRes = 0; columnRes < rows; columnRes++)
                {
                    res.SetValue(rowRes, columnRes,
                        (-((rowRes + columnRes) % 2) * 2 + 1) * GetDet(Minor(matrix, rowRes, columnRes)) / det);
                }
            return Transpose(res);
        }
        public static double Modul(IMatrixable matrix)
        {
            double res = 0;
            int rows = matrix.GetRows(), columns = matrix.GetColumns();
            for (int row = 0; row < rows; row++)
                for (int column = 0; column < columns; column++)
                    res += Math.Pow(matrix.GetValue(row, column), 2);
            return Math.Pow(res, 1.0 / 2);
        }
        public static Vector GetColumn(IMatrixable matrix, int columnIndex)
        {
            if (matrix.GetColumns() < columnIndex || columnIndex < 0)
                throw new ArgumentException("columnIndex");
            int rows = matrix.GetRows();
            Vector res = new(rows);
            for (int i = 0; i < rows; i++)
            {
                res.SetValue(i, 0, matrix.GetValue(i, columnIndex));
            }
            return res;
        }
    }

}
