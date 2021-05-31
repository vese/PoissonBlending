using System;
using System.Collections.Generic;

namespace PoissonBlending.Lib.Solver
{
    // Метод последовательной верхней релаксации (SOR - Successive OverRelaxation)
    public class SorSolver : BaseSolver
    {
        // Сходится при 9 < K < 2
        public double K { get; set; } = 1.9;

        public SorSolver(double? k, params OnProgressHandler[] onProgressHandlers) : base(onProgressHandlers)
        {
            if (k.HasValue)
            {
                K = k.Value;
            }
        }

        protected override (double[] x, int iteration) SolveInternal(string colorComponentName, double[] pixels, List<int>[] neighbors, double acceptedError)
        {
            var n = pixels.Length;
            var x = new double[n];
            var nextX = new double[n];

            bool errorSuits = false;
            int it = 0;
            while (!errorSuits)
            {
                for (int i = 0; i < n; i++)
                {
                    nextX[i] = pixels[i] / 4;
                    neighbors[i].ForEach(neighbor => nextX[i] += (K * nextX[neighbor] + (1 - K) * x[neighbor]) / 4);
                }

                var error = Errors.CalculateError(x, nextX);
                errorSuits = error < acceptedError;

                Array.Copy(nextX, x, n);

                ReportProgress(colorComponentName, it++, error);
            }

            return (x, ++it);
        }
    }
}
