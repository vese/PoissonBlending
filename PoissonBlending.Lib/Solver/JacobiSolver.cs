using PoissonBlending.Lib.PixelDescription;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PoissonBlending.Lib.Solver
{
    public class JacobiSolver : BaseSolver
    {
        public double AcceptError { get; set; } = 1;

        public JacobiSolver(params OnProgressHandler[] onProgressHandlers) : base(onProgressHandlers) { }

        private int[] Solve(string colorComponentName, int[] pixels, List<int>[] neighbors)
        {
            var watch = Stopwatch.StartNew();

            var n = pixels.Length;
            var x = new double[n];
            var nextX = new double[n];

            bool errorSuits = false;
            int it = 0;
            while (!errorSuits)
            {
                for (int i = 0; i < n; i++)
                {
                    nextX[i] = pixels[i];
                    neighbors[i].ForEach(neighboard => nextX[i] += x[neighboard]);
                    nextX[i] /= 4;
                }

                var error = Error(x, nextX);
                errorSuits = error < AcceptError;

                for (int i = 0; i < n; i++)
                {
                    x[i] = nextX[i];
                }

                ReportProgress(colorComponentName, it++, error);
            }

            watch.Stop();
            ReportProgress(colorComponentName, it++, 0, watch.ElapsedMilliseconds);

            return x.Select(value => (int)value).ToArray();
        }

        /// <summary>
        /// Решение системы уравнений.
        /// </summary>
        /// <param name="pixels">Массив пикселей.</param>
        /// <param name="neighbors">Массив списков идентификаторов соседних пикселей.</param>
        /// <returns>Вычисленный массив пикселей.</returns>
        public override PixelArray Solve(PixelArray pixels, List<int>[] neighbors)
        {
            var x = new Dictionary<string, int[]>();
            var colorComponentsValues = pixels.GetColorComponentsValues();
            foreach (var colorComponentValues in colorComponentsValues)
            {
                x.Add(colorComponentValues.Key, Solve(colorComponentValues.Key, colorComponentValues.Value, neighbors));
            }
            return new PixelArray(x);
        }

        /// <summary>
        /// Оценивает погрешность.
        /// </summary>
        /// <param name="x">Предыдущее решение.</param>
        /// <param name="nextX">Новое решение.</param>
        /// <returns>Оценка погрешности.</returns>
        private static double Error(double[] x, double[] nextX)
        {
            double error = 0;
            for (int i = 0; i < x.GetLength(0); i++)
            {
                error += Math.Pow(nextX[i] - x[i], 2);
            }
            return Math.Sqrt(error);
        }
    }
}
