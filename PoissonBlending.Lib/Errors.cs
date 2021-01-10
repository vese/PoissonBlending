using PoissonBlending.Lib.PixelDescription;
using PoissonBlending.Lib.Solver;
using System;

namespace PoissonBlending.Lib
{
    static class Errors
    {
        public const double JacobiRgbError = 1;
        public const double JacobiHslError = 0.00001;
        public const double JacobiCmyError = 0.001;
        public const double JacobiCmykError = 0.001;

        public static double GetAcceptedError(ISolver solver, BasePixel colorModel)
        {
            return (solver, colorModel) switch
            {
                (JacobiSolver, RgbPixel) => JacobiRgbError,
                (JacobiSolver, HslPixel) => JacobiHslError,
                (JacobiSolver, CmyPixel) => JacobiCmyError,
                (JacobiSolver, CmykPixel) => JacobiCmykError,
                _ => throw new ArgumentException($"Unknown solver or color model")
            };
        }
    }
}
