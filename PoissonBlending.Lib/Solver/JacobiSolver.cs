using PoissonBlending.Lib.PixelDescription;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PoissonBlending.Lib.Solver
{
    public class JacobiSolver : BaseSolver
    {
        // 1 - rgb
        // 0.00001 - hsl
        public double AcceptError { get; set; } = 0.00001;

        public JacobiSolver(params OnProgressHandler[] onProgressHandlers) : base(onProgressHandlers) { }

        private double[] Solve(string colorComponentName, double[] pixels, List<int>[] neighbors)
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

            return x;
        }

        /// <summary>
        /// Решение системы уравнений.
        /// </summary>
        /// <param name="pixels">Массив пикселей.</param>
        /// <param name="neighbors">Массив списков идентификаторов соседних пикселей.</param>
        /// <returns>Вычисленный массив пикселей.</returns>
        public override PixelArray<Pixel> Solve<Pixel>(PixelArray<Pixel> pixels, List<int>[] neighbors)
        {
            var x = new Dictionary<string, double[]>();
            var colorComponentsValues = pixels.GetColorComponentsValues();
            foreach (var colorComponentValues in colorComponentsValues)
            {
                x.Add(colorComponentValues.Key, Solve(colorComponentValues.Key, colorComponentValues.Value, neighbors));
            }
            return new PixelArray<Pixel>(x);
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
