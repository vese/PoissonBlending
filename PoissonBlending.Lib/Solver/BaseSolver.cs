using System;
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

        public abstract Pixel[] Solve(Pixel[] pixels, List<int>[] neighbors);

        protected void ReportProgress(int iteration, double error)
        {
            OnProgress?.Invoke(iteration, error);
        }
    }
}
