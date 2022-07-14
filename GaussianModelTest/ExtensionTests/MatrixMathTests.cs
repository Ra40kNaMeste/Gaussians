using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GaussiansModel.Extension;

namespace GaussianModelTest.ExtensionTests
{
    [TestClass]
    public class MatrixMathTests
    {
        [TestMethod]
        public void SumTwoMatrixTest()
        {
            int rows = 3, cols = 2;
            Matrix matrix1 = CreateAndFillMatrix(rows, cols, (r, c) => r * cols + c);
            Matrix matrix2 = CreateAndFillMatrix(rows, cols, (r, c) => r * cols + c);
            Assert.IsTrue(CanEqualsMatrix(MatrixMath.Sum(matrix1, matrix2), (r, c) => 2 * (r * cols + c)));

        }
        [TestMethod]
        public void SumTwoInvalidMatrixTest()
        {
            int rows = 3, cols = 2;
            Matrix matrix1 = CreateAndFillMatrix(rows, cols, (r, c) => r * cols + c);
            Matrix matrix2 = CreateAndFillMatrix(rows, cols + 1, (r, c) => r * cols + c);
            Assert.ThrowsException<ArgumentException>(() => MatrixMath.Sum(matrix1, matrix2));
        }
        [TestMethod]
        public void SumMatrixAndDoubleTest()
        {
            int rows = 3, cols = 2;
            Matrix matrix1 = CreateAndFillMatrix(rows, cols, (r, c) => r * cols + c);
            Assert.IsTrue(CanEqualsMatrix(MatrixMath.Sum(matrix1, 2), (r, c) => r * cols + c + 2));
        }

        [TestMethod]
        public void SubTwoMatrixTest()
        {
            int rows = 3, cols = 2;
            Matrix matrix1 = CreateAndFillMatrix(rows, cols, (r, c) => r * cols + c);
            Matrix matrix2 = CreateAndFillMatrix(rows, cols, (r, c) => r * cols + c);

            Assert.IsTrue(CanEqualsMatrix(MatrixMath.Sub(matrix1, matrix2), (r, c) => 0));
        }
        [TestMethod]
        public void SubTwoInvalidMatrixTest()
        {
            int rows = 3, cols = 2;
            Matrix matrix1 = CreateAndFillMatrix(rows, cols, (r, c) => r * cols + c);
            Matrix matrix2 = CreateAndFillMatrix(rows, cols + 1, (r, c) => r * cols + c);
            Assert.ThrowsException<ArgumentException>(() => MatrixMath.Sub(matrix1, matrix2));
        }
        [TestMethod]
        public void SubMatrixAndDoubleTest()
        {
            int rows = 3, cols = 2;
            Matrix matrix1 = CreateAndFillMatrix(rows, cols, (r, c) => r * cols + c);
            Assert.IsTrue(CanEqualsMatrix(MatrixMath.Sub(matrix1, 2), (r, c) => r * cols + c - 2));
        }

        [TestMethod]
        public void MulitplyTwoQuadratMatrixTest()
        {
            int rows = 3, cols = 3;
            Matrix matrix1 = CreateAndFillMatrix(rows, cols, (r, c) => r * cols + c);
            Matrix matrix2 = CreateAndFillMatrix(rows, cols, (r, c) => r * cols + c);
            Assert.IsTrue(CanEqualsMatrix(MatrixMath.Multiply(matrix1, matrix2), new double[]
            {
                15, 18, 21,
                42, 54, 66,
                69, 90, 111
            }));
        }

        [TestMethod]
        public void MulitplyTwoNotEqualsMatrixTest()
        {
            int rows = 3, cols = 2;
            Matrix matrix1 = CreateAndFillMatrix(rows, cols, (r, c) => r * cols + c);
            Matrix matrix2 = CreateAndFillMatrix(cols, rows, (r, c) => r * rows + c);
            Assert.IsTrue(CanEqualsMatrix(MatrixMath.Multiply(matrix1, matrix2), new double[]
            {
                3, 4, 5,
                9, 14, 19,
                15, 24, 33
            }));
        }

        [TestMethod]
        public void MulitplyTwoNotEquals2MatrixTest()
        {
            int rows = 3, cols = 2;
            Matrix matrix1 = CreateAndFillMatrix(rows, cols, (r, c) => r * cols + c);
            Matrix matrix2 = CreateAndFillMatrix(cols, rows, (r, c) => r * rows + c);
            Assert.IsTrue(CanEqualsMatrix(MatrixMath.Multiply(matrix2, matrix1), new double[]
            {
                10, 13,
                28, 40
            }));
        }

        [TestMethod]
        public void MulitplyTwoErrorMatrixTest()
        {
            int rows = 3, cols = 2;
            Matrix matrix1 = CreateAndFillMatrix(rows + 1, cols, (r, c) => r * cols + c);
            Matrix matrix2 = CreateAndFillMatrix(cols, rows, (r, c) => r * rows + c);
            Assert.ThrowsException<ArgumentException>(() => MatrixMath.Multiply(matrix2, matrix1));
        }
        [TestMethod]
        public void MultiPlyMatrixAndDoubleTest()
        {
            int rows = 3, cols = 2;
            Matrix matrix1 = CreateAndFillMatrix(rows, cols, (r, c) => r * cols + c);
            Assert.IsTrue(CanEqualsMatrix(MatrixMath.Multiply(matrix1, 3), (r, c) => 3 * (r * cols + c)));
        }

        [TestMethod]
        public void TransposeQuadMatrixTest()
        {
            int rows = 3, cols = 3;
            Matrix matrix1 = CreateAndFillMatrix(rows, cols, (r, c) => r * cols + c);
            Assert.IsTrue(CanEqualsMatrix(MatrixMath.Transpose(matrix1), (r, c) => c * cols + r));
        }
        [TestMethod]
        public void TransposeMatrixTest()
        {
            int rows = 3, cols = 2;
            Matrix matrix1 = CreateAndFillMatrix(rows, cols, (r, c) => r * cols + c);
            IMatrixable res = MatrixMath.Transpose(matrix1);
            Assert.IsTrue(CanEqualsMatrix(res, (r, c) => c * cols + r) && res.GetRows() == cols && res.GetColumns() == rows);
        }


        [TestMethod]
        public void GetDetQuadMatrixTest()
        {
            int rows = 3, cols = 3;
            Matrix matrix1 = CreateAndFillMatrix(rows, cols, (r, c) =>
            {
                if (r == c)
                    return -(r * cols + c + 1);
                return r * cols + c + 1;
            });
            Assert.IsTrue(MatrixMath.GetDet(matrix1) == 360);
        }
        [TestMethod]
        public void GetDetTwoQuadMatrixTest()
        {
            int rows = 2, cols = 2;
            Matrix matrix1 = CreateAndFillMatrix(rows, cols, (r, c) =>
            {
                if (r == c)
                    return -(r * cols + c + 1);
                return r * cols + c + 1;
            });
            Assert.IsTrue(MatrixMath.GetDet(matrix1) == -2);
        }
        [TestMethod]
        public void GetDetNotQuadMatrixTest()
        {
            int rows = 3, cols = 2;
            Matrix matrix1 = CreateAndFillMatrix(rows, cols, (r, c) =>
            {
                if (r == c)
                    return -r * cols + c;
                return r * cols + c;
            });
            Assert.ThrowsException<ArgumentException>(() => MatrixMath.GetDet(matrix1));
        }

        [TestMethod]
        public void GetMinorQuadMatrixTest()
        {
            int rows = 3, cols = 3;
            Matrix matrix1 = CreateAndFillMatrix(rows, cols, (r, c) =>
            {
                return r * cols + c + 1;
            });
            Assert.IsTrue(CanEqualsMatrix(MatrixMath.Minor(matrix1, 1, 1), new double[]
            {
                1, 3,
                7, 9
            }));
        }
        [TestMethod]
        public void GetMinorNotQuadMatrixTest()
        {
            int rows = 4, cols = 3;
            Matrix matrix1 = CreateAndFillMatrix(rows, cols, (r, c) =>
            {
                return r * cols + c + 1;
            });
            Assert.IsTrue(CanEqualsMatrix(MatrixMath.Minor(matrix1, 1, 1), new double[]
            {
                1, 3,
                7, 9,
                10, 12
            }));
        }

        [TestMethod]
        public void GetInversQuadMatrixTest()
        {
            int rows = 3, cols = 3;
            Matrix matrix1 = CreateAndFillMatrix(rows, cols, new double[]
            {
                1, 2, 3,
                2, 1, 2,
                3, 2, 1
            });
            Assert.IsTrue(CanEqualsMatrix(MatrixMath.Invers(matrix1), new double[]
            {
                -0.375, 0.5, 0.125,
                0.5, -1, 0.5, 
                0.125, 0.5, -0.375
            }));
        }
        [TestMethod]
        public void GetModulVectorTest()
        {
            int rows = 2;
            Vector vector = new(rows);
            vector.SetValue(0, 0, 3);
            vector.SetValue(1, 0, 4);
             Assert.IsTrue(MatrixMath.Modul(vector) == 5);
        }

        [TestMethod]
        public void GetColumnTest()
        {
            int rows = 3, cols = 3;
            Matrix matrix1 = CreateAndFillMatrix(rows, cols, new double[]
            {
                1, 2, 3,
                2, 1, 2,
                3, 2, 1
            });
            Assert.IsTrue(CanEqualsMatrix(MatrixMath.GetColumn(matrix1, 1), new double[]
            {
                2, 1, 2
            }));
        }

        private static Matrix CreateAndFillMatrix(int rows, int cols, Func<int, int, double> func)
        {
            Matrix matrix = new Matrix(rows, cols);
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    matrix.SetValue(row, col, func(row, col));
                }
            }
            return matrix;
        }
        private static Matrix CreateAndFillMatrix(int rows, int cols, double[] values)
        {
            Matrix matrix = new Matrix(rows, cols);
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    matrix.SetValue(row, col, values[row * cols + col]);
                }
            }
            return matrix;
        }

        private static bool CanEqualsMatrix(IMatrixable matrix, double[] values)
        {
            int rows = matrix.GetRows(), cols = matrix.GetColumns();
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    if (matrix.GetValue(row, col) != values[row * cols + col])
                        return false;
                }
            }
            return true;
        }

        private static bool CanEqualsMatrix(IMatrixable matrix, Func<int, int, double> func)
        {
            int rows = matrix.GetRows(), cols = matrix.GetColumns();
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    if (matrix.GetValue(row, col) != func(row, col))
                        return false;

                }
            }
            return true;
        }
    }
}
