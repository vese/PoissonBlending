using PoissonBlending.Lib.PixelDescription;
using System.Collections.Generic;

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

        public abstract PixelArray Solve(PixelArray pixels, List<int>[] neighbors);

        protected void ReportProgress(string colorComponentName, int iteration, double error, long? elapsedMs = null)
        {
            OnProgress?.Invoke(colorComponentName, iteration, error, elapsedMs);
        }
    }
}
