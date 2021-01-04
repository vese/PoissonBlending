using System;
using System.Collections.Generic;

namespace PoissonBlending.Lib.Solver
{
    public delegate void OnProgressHandler(int iteration, double error);

    interface ISolver
    {
        event OnProgressHandler OnProgress;

        /// <summary>
        /// Решение системы уравнений.
        /// </summary>
        /// <param name="pixels">Массив пикселей.</param>
        /// <param name="neighbors">Массив списков идентификаторов соседних пикселей.</param>
        /// <returns>Вычисленный массив пикселей.</returns>
        Pixel[] Solve(Pixel[] pixels, List<int>[] neighbors);
    }
}
