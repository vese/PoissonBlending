using PoissonBlending.Lib.PixelDescription;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PoissonBlending.Lib.Solver
{
    public abstract class BaseSolver : ISolver
    {
        public event OnProgressHandler OnProgress;

        public BaseSolver(OnProgressHandler[] onProgressHandlers)
        {
            foreach (var handler in onProgressHandlers)
            {
                OnProgress += handler;
            }
        }

        /// <summary>
        /// Решение системы уравнений.
        /// </summary>
        /// <param name="pixels">Массив пикселей.</param>
        /// <param name="neighbors">Массив списков идентификаторов соседних пикселей.</param>
        /// <returns>Вычисленный массив пикселей.</returns>
        public PixelArray<Pixel> Solve<Pixel>(Mask<Pixel> mask) where Pixel : IPixel, new()
        {
            var colorComponentsValues = mask.Pixels.GetColorComponentsValues();
            var acceptedError = Errors.GetAcceptedError(this, new Pixel());
            var x = colorComponentsValues.Select(colorComponentValues =>
                new KeyValuePair<string, double[]>(colorComponentValues.Key, Solve(colorComponentValues.Key, colorComponentValues.Value, mask.PixelsNeighboards, acceptedError))
            );
            return new PixelArray<Pixel>(x);
        }

        /// <summary>
        /// Решение системы уравнений.
        /// </summary>
        /// <param name="pixels">Массив пикселей.</param>
        /// <param name="neighbors">Массив списков идентификаторов соседних пикселей.</param>
        /// <returns>Вычисленный массив пикселей.</returns>
        public async Task<PixelArray<Pixel>> SolveAsync<Pixel>(Mask<Pixel> mask) where Pixel : IPixel, new()
        {
            var colorComponentsValues = mask.Pixels.GetColorComponentsValues();
            var acceptedError = Errors.GetAcceptedError(this, new Pixel());
            var tasks = colorComponentsValues.ToList().Select(colorComponentValues =>
                new Task<KeyValuePair<string, double[]>>(() =>
                    new(colorComponentValues.Key, Solve(colorComponentValues.Key, colorComponentValues.Value, mask.PixelsNeighboards, acceptedError))
                    )
                ).ToList();
            tasks.ForEach(task => task.Start());
            var x = await Task.WhenAll(tasks).ConfigureAwait(false);
            return new PixelArray<Pixel>(x);
        }

        protected double[] Solve(string colorComponentName, double[] pixels, List<int>[] neighbors, double acceptedError)
        {
            var watch = Stopwatch.StartNew();

            (var x, var it) = SolveInternal(colorComponentName, pixels, neighbors, acceptedError);

            watch.Stop();
            ReportProgress(colorComponentName, it, 0, watch.ElapsedMilliseconds);

            return x;
        }

        protected abstract (double[] x, int iteration) SolveInternal(string colorComponentName, double[] pixels, List<int>[] neighbors, double acceptedError);

        protected void ReportProgress(string colorComponentName, int iteration, double error, long? elapsedMs = null)
        {
            OnProgress?.Invoke(colorComponentName, iteration, error, elapsedMs);
        }
    }
}
