using PoissonBlending.Lib.PixelDescription;
using System.Collections.Generic;

namespace PoissonBlending.Lib.Solver
{
    public delegate void OnProgressHandler(string colorComponentName, int iteration, double error, long? elapsedMs);

    interface ISolver
    {
        event OnProgressHandler OnProgress;

        /// <summary>
        /// Решение системы уравнений.
        /// </summary>
        /// <param name="pixels">Массив пикселей.</param>
        /// <param name="neighbors">Массив списков идентификаторов соседних пикселей.</param>
        /// <returns>Вычисленный массив пикселей.</returns>
        PixelArray Solve(PixelArray pixels, List<int>[] neighbors);
    }
}
