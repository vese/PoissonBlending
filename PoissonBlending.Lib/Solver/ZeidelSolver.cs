using System;
using System.Collections.Generic;

namespace PoissonBlending.Lib.Solver
{
    public class ZeidelSolver : BaseSolver
    {
        public ZeidelSolver(params OnProgressHandler[] onProgressHandlers) : base(onProgressHandlers) { }

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
                    neighbors[i].ForEach(neighbor => nextX[i] += nextX[neighbor] / 4);
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
