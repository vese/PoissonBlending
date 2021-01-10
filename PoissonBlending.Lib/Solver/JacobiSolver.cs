using PoissonBlending.Lib.PixelDescription;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PoissonBlending.Lib.Solver
{
    public class JacobiSolver : BaseSolver
    {
        public JacobiSolver(params OnProgressHandler[] onProgressHandlers) : base(onProgressHandlers) { }

        private double[] Solve(string colorComponentName, double[] pixels, List<int>[] neighbors, double acceptedError)
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
                errorSuits = error < acceptedError;

                Array.Copy(nextX, x, n);

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
            var colorComponentsValues = pixels.GetColorComponentsValues();
            var acceptedError = Errors.GetAcceptedError(this, new Pixel());
            var x = colorComponentsValues.Select(colorComponentValues =>
                new KeyValuePair<string, double[]>(colorComponentValues.Key, Solve(colorComponentValues.Key, colorComponentValues.Value, neighbors, acceptedError))
            );
            return new PixelArray<Pixel>(x);
        }

        /// <summary>
        /// Решение системы уравнений.
        /// </summary>
        /// <param name="pixels">Массив пикселей.</param>
        /// <param name="neighbors">Массив списков идентификаторов соседних пикселей.</param>
        /// <returns>Вычисленный массив пикселей.</returns>
        public override async Task<PixelArray<Pixel>> SolveAsync<Pixel>(PixelArray<Pixel> pixels, List<int>[] neighbors)
        {
            var colorComponentsValues = pixels.GetColorComponentsValues();
            var acceptedError = Errors.GetAcceptedError(this, new Pixel());
            var tasks = colorComponentsValues.ToList().Select(colorComponentValues =>
                new Task<KeyValuePair<string, double[]>>(() =>
                    new(colorComponentValues.Key, Solve(colorComponentValues.Key, colorComponentValues.Value, neighbors, acceptedError))
                    )
                ).ToList();
            tasks.ForEach(task => task.Start());
            var x = await Task.WhenAll(tasks).ConfigureAwait(false);
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
