using PoissonBlending.Lib.PixelDescription;
using System.Collections.Generic;
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

        public abstract PixelArray<Pixel> Solve<Pixel>(PixelArray<Pixel> pixels, List<int>[] neighbors) where Pixel : BasePixel, new();

        public abstract Task<PixelArray<Pixel>> SolveAsync<Pixel>(PixelArray<Pixel> pixels, List<int>[] neighbors) where Pixel : BasePixel, new();

        protected void ReportProgress(string colorComponentName, int iteration, double error, long? elapsedMs = null)
        {
            OnProgress?.Invoke(colorComponentName, iteration, error, elapsedMs);
        }
    }
}
