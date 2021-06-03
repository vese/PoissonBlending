using PoissonBlending.Lib.PixelDescription;
using PoissonBlending.Lib.Solver;
using System;

namespace PoissonBlending.Lib
{
    static class Errors
    {
        public const double JacobiRgbError = 0.01;
        public const double JacobiHslError = 0.00001;
        public const double JacobiCmyError = 0.001;
        public const double JacobiCmykError = 0.001;

        public static double GetAcceptedError(ISolver solver, IPixel colorModel)
        {
            return (solver, colorModel) switch
            {
                (JacobiSolver, RgbPixel) => 0.003,
                (JacobiSolver, HslPixel) => 0.000001,
                (JacobiSolver, CmyPixel) => 0.000001,
                (JacobiSolver, CmykPixel) => 0.000001,
                (GaussSeidelSolver, RgbPixel) => 0.003,
                (GaussSeidelSolver, HslPixel) => 0.000001,
                (GaussSeidelSolver, CmyPixel) => 0.000001,
                (GaussSeidelSolver, CmykPixel) => 0.000001,
                (SorSolver, RgbPixel) => 0.003,
                (SorSolver, HslPixel) => 0.000001,
                (SorSolver, CmyPixel) => 0.000001,
                (SorSolver, CmykPixel) => 0.000001,
                _ => throw new ArgumentException($"Unknown solver or color model")
            };
        }

        /// <summary>
        /// Оценивает погрешность.
        /// </summary>
        /// <param name="x">Предыдущее решение.</param>
        /// <param name="nextX">Новое решение.</param>
        /// <returns>Оценка погрешности.</returns>
        public static double CalculateError(double[] x, double[] nextX)
        {
            double error = 0;
            for (int i = 0; i < x.GetLength(0); i++)
            {
                var newError = nextX[i] - x[i];
                if (error < newError)
                {
                    error = newError;
                }
            }
            return error;
        }
    }
}
