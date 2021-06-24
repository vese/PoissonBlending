using PoissonBlending.Lib.PixelDescription;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PoissonBlending.Lib.Solver
{
    public delegate void OnProgressHandler(string colorComponentName, int iteration, double error, long? elapsedMs);

    public interface ISolver
    {
        event OnProgressHandler OnProgress;

        /// <summary>
        /// Решение системы уравнений.
        /// </summary>
        /// <param name="pixels">Массив пикселей.</param>
        /// <param name="neighbors">Массив списков идентификаторов соседних пикселей.</param>
        /// <returns>Вычисленный массив пикселей.</returns>
        (PixelArray<Pixel> result, Dictionary<string, long> times) Solve<Pixel>(Mask<Pixel> mask) where Pixel : IPixel, new();

        /// <summary>
        /// Решение системы уравнений.
        /// </summary>
        /// <param name="pixels">Массив пикселей.</param>
        /// <param name="neighbors">Массив списков идентификаторов соседних пикселей.</param>
        /// <returns>Вычисленный массив пикселей.</returns>
        Task<(PixelArray<Pixel> result, Dictionary<string, long> times)> SolveAsync<Pixel>(Mask<Pixel> mask) where Pixel : IPixel, new();
    }
}
