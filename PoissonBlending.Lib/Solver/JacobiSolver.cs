﻿using System;
using System.Collections.Generic;

namespace PoissonBlending.Lib.Solver
{
    public class JacobiSolver : BaseSolver
    {
        public JacobiSolver(params OnProgressHandler[] onProgressHandlers) : base(onProgressHandlers) { }

        protected override (double[] x, int iteration) SolveInternal(string colorComponentName, double[] pixels, List<int>[] neighbors, double acceptedError)
        {
            var n = pixels.Length;
            var x = new double[n];
            var nextX = new double[n];

            bool errorSuits = false;
            int it = 0;
            while (!errorSuits)
            {
                Array.Copy(pixels, nextX, n);
                for (int i = 0; i < n; i++)
                {
                    neighbors[i].ForEach(neighbor => nextX[i] += x[neighbor]);
                    nextX[i] /= 4;
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
