using GaussiansModel.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GaussianModelTest.ExtensionTests
{
    [TestClass]
    public class NonlinearSolverTests
    {
        [TestMethod]
        public void MinimizeTest()
        {
            Func<Vector, double> func = (v) =>
            {
                return 2 * Math.Pow(v.GetValue(0, 0), 2) - 5 * Math.Pow(v.GetValue(1, 0), 2) + 3;
            };
            Vector startVector = new Vector(2);
            startVector.SetValue(0, 0, 10);
            startVector.SetValue(1, 0, 10);
            Vector valVector = new Vector(2);
            startVector.SetValue(0, 0, 0);
            startVector.SetValue(1, 0, 0);
            Vector minVector = NonlinearSolver.Minimize(func, startVector, 3, 0.8, 0.001, 1000);
            Assert.IsTrue(CanTrue(valVector, startVector, 0.01));
        }

        [TestMethod]
        public void MinimumTest()
        {
            Func<double, double> func = (x) =>
            {
                return Math.Pow(x, 3) - x + 3;
            };
            double startX = 1;
            double res = NonlinearSolver.FindMinimumByNewton(func, startX, 0.000001);
            Assert.IsTrue(CanTrue(res, 0.577, 0.001));
        }

        private static bool CanTrue(Vector val, Vector res, double error)
        {
            return Math.Abs(MatrixMath.Modul(MatrixMath.Sub(val, res))) < error;
        }
        private static bool CanTrue(double val, double res, double error)
        {
            return Math.Abs(Math.Abs(val - res)) < error;
        }
    }
}
