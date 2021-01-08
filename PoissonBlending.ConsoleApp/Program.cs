using PoissonBlending.Lib;
using PoissonBlending.Lib.PixelDescription;
using System;

namespace PoissonBlending.ConsoleApp
{
    class Program
    {
        static void Main()
        {
            var solver = new PoissonBlendingSolver((string message) => Console.WriteLine(message));
            //solver.Impose("A.jpg", "B.jpg", 300, 70, resultImageFilename: "rgbResult.jpg");
            //solver.Impose<HslPixel>("A.jpg", "B.jpg", 300, 70, resultImageFilename: "hslResult.jpg");
            //solver.Impose<CmyPixel>("A.jpg", "B.jpg", 300, 70, resultImageFilename: "cmyResult.jpg");
            solver.Impose<CmykPixel>("A.jpg", "B.jpg", 300, 70, resultImageFilename: "cmykResult.jpg");
            solver.ImposeWithoutBlending("A.jpg", "B.jpg", 300, 70, resultImageFilename: "resultWithoutBlending.jpg");
        }
    }
}
