using PoissonBlending.Lib.Solver;
using System;

namespace PoissonBlending.Lib
{
    public class ImposeOptions : ImposeWithoutBlendingOptions
    {
        private ILogService _logService;
        private ISolver _solver;

        public LogProgressDelegate LogProgressDelegate { get; set; }
        public bool ShowIntermediateProgress { get; set; } = false;
        public SolverType SolverType { get; set; } = SolverType.Jacobi;
        public GuidanceFieldType GuidanceFieldType { get; set; } = GuidanceFieldType.Normal;

        public double? SolverParam { get; set; }

        public ILogService GetLogService()
        {
            if (_logService == null)
            {
                _logService = new LogService(LogProgressDelegate, ShowIntermediateProgress);
            }
            return _logService;
        }

        public ISolver GetSolver()
        {
            if (_solver == null)
            {
                _solver = SolverType switch
                {
                    SolverType.Jacobi => new JacobiSolver(GetLogService().LogSolveProgress),
                    SolverType.GaussSeidel => new GaussSeidelSolver(GetLogService().LogSolveProgress),
                    SolverType.Sor => new SorSolver(SolverParam, GetLogService().LogSolveProgress),
                    _ => throw new NotImplementedException(),
                };
            }
            return _solver;
        }
    }
}
