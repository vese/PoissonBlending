using System;
using System.Collections.Generic;

namespace PoissonBlending.Lib.Solver
{
    public class JacobiSolver : BaseSolver
    {
        //public event EventHandler<ProgressEventArgs> OnProgress;

        public double AcceptError { get; set; } = 1;

        public JacobiSolver(params OnProgressHandler[] onProgressHandlers) : base(onProgressHandlers) { }

        /// <summary>
        /// Решение системы уравнений.
        /// </summary>
        /// <param name="pixels">Массив пикселей.</param>
        /// <param name="neighbors">Массив списков идентификаторов соседних пикселей.</param>
        /// <returns>Вычисленный массив пикселей.</returns>
        public override Pixel[] Solve(Pixel[] pixels, List<int>[] neighbors)
        {
            int n = pixels.Length;
            double[,] x = new double[n, 3], nextX = new double[n, 3];
            int[,] pixelsRGB = new int[n, 3];
            for (int i = 0; i < n; i++)
            {
                pixelsRGB[i, 0] = pixels[i].R;
                pixelsRGB[i, 1] = pixels[i].G;
                pixelsRGB[i, 2] = pixels[i].B;
            }
            bool errorSuits = false;
            int it = 0;
            while (!errorSuits)
            {
                for (int k = 0; k < 3; k++)
                {
                    for (int i = 0; i < n; i++)
                    {
                        nextX[i, k] = pixelsRGB[i, k];
                        neighbors[i].ForEach(neighboard => nextX[i, k] += x[neighboard, k]);
                        nextX[i, k] /= 4;
                    }
                }
                var error = Error(x, nextX);
                errorSuits = error < AcceptError;
                for (int k = 0; k < 3; k++)
                {
                    for (int i = 0; i < n; i++)
                    {
                        x[i, k] = nextX[i, k];
                    }
                }
                ReportProgress(it++, error);
            }

            Pixel[] result = new Pixel[n];
            for (int i = 0; i < n; i++)
            {
                result[i] = new Pixel { R = (int)x[i, 0], G = (int)x[i, 1], B = (int)x[i, 2] };
            }
            return result;
        }

        /// <summary>
        /// Оценивает погрешность.
        /// </summary>
        /// <param name="x">Предыдущее решение.</param>
        /// <param name="nextX">Новое решение.</param>
        /// <returns>Оценка погрешности.</returns>
        private static double Error(double[,] x, double[,] nextX)
        {
            double error = 0;
            for (int k = 0; k < 3; k++)
            {
                for (int i = 0; i < x.GetLength(0); i++)
                {
                    error += Math.Pow(nextX[i, k] - x[i, k], 2);
                }
            }
            return Math.Sqrt(error);
        }
    }
}
