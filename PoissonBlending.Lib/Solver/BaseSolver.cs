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
        public (PixelArray<Pixel> result, Dictionary<string, long> times) Solve<Pixel>(Mask<Pixel> mask) where Pixel : IPixel, new()
        {
            var colorComponentsValues = mask.Pixels.GetColorComponentsValues();
            var acceptedError = Errors.GetAcceptedError(this, new Pixel());
            var x = new List<KeyValuePair<string, double[]>>();
            var times = new Dictionary<string, long>();
            foreach (var colorComponentValues in colorComponentsValues)
            {
                var result = Solve(colorComponentValues.Key, colorComponentValues.Value, mask.PixelsNeighbors, acceptedError);
                x.Add(new KeyValuePair<string, double[]>(colorComponentValues.Key, result.x));
                times.Add(colorComponentValues.Key, result.time);
            }
            return (new PixelArray<Pixel>(x), times);
        }

        /// <summary>
        /// Решение системы уравнений.
        /// </summary>
        /// <param name="pixels">Массив пикселей.</param>
        /// <param name="neighbors">Массив списков идентификаторов соседних пикселей.</param>
        /// <returns>Вычисленный массив пикселей.</returns>
        public async Task<(PixelArray<Pixel> result, Dictionary<string, long> times)> SolveAsync<Pixel>(Mask<Pixel> mask) where Pixel : IPixel, new()
        {
            var colorComponentsValues = mask.Pixels.GetColorComponentsValues();
            var acceptedError = Errors.GetAcceptedError(this, new Pixel());
            var tasks = colorComponentsValues
                .Select(colorComponentValues => 
                    new Task<(double[] x, long time)>(() => Solve(colorComponentValues.Key, colorComponentValues.Value, mask.PixelsNeighbors, acceptedError)))
                .ToList();
            tasks.ForEach(task => task.Start());
            var results = await Task.WhenAll(tasks).ConfigureAwait(false);
            var x = new List<KeyValuePair<string, double[]>>();
            var times = new Dictionary<string, long>();
            var colorComponentsValuesList = colorComponentsValues.ToList();
            for (int i = 0; i < colorComponentsValuesList.Count; i++)
            {
                x.Add(new KeyValuePair<string, double[]>(colorComponentsValuesList[i].Key, results[i].x));
                times.Add(colorComponentsValuesList[i].Key, results[i].time);
            }
            return (new PixelArray<Pixel>(x), times);
        }

        protected (double[] x, long time) Solve(string colorComponentName, double[] pixels, List<int>[] neighbors, double acceptedError)
        {
            var watch = Stopwatch.StartNew();

            (var x, var it) = SolveInternal(colorComponentName, pixels, neighbors, acceptedError);

            watch.Stop();
            ReportProgress(colorComponentName, it, 0, watch.ElapsedMilliseconds);

            return (x, watch.ElapsedMilliseconds);
        }

        protected abstract (double[] x, int iteration) SolveInternal(string colorComponentName, double[] pixels, List<int>[] neighbors, double acceptedError);

        protected void ReportProgress(string colorComponentName, int iteration, double error, long? elapsedMs = null)
        {
            OnProgress?.Invoke(colorComponentName, iteration, error, elapsedMs);
        }
    }
}
