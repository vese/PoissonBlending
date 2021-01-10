using PoissonBlending.Lib;
using PoissonBlending.Lib.PixelDescription;
using System;
using System.Threading.Tasks;

namespace PoissonBlending.ConsoleApp
{
    class Program
    {
        static async Task Main()
        {
            var solver = new PoissonBlendingSolver((string message) => Console.WriteLine(message))
            {
                //ShowIntermediateProgress = true
            };
            var points = new Point[] 
            {
                new Point(0, 0), 
                new Point(50, 20), 
                new Point(70, 30), 
                new Point(85, 1), // 2 пика
                new Point(100, 31), 
                new Point(149, 41), 
                new Point(149, 91), 
                new Point(0, 91) 
            };
            await solver.ImposeAsync("A.jpg", "B.jpg", 300, 70, points, resultImageFilename: "rgbResultAsync points.jpg");
            //solver.Impose("A.jpg", "B.jpg", 300, 70, points, resultImageFilename: "rgbResult points.jpg");
            //solver.Impose("A.jpg", "B.jpg", 300, 70, resultImageFilename: "rgbResult.jpg");
            //solver.Impose<HslPixel>("A.jpg", "B.jpg", 300, 70, resultImageFilename: "hslResult.jpg");
            //solver.Impose<CmyPixel>("A.jpg", "B.jpg", 300, 70, resultImageFilename: "cmyResult.jpg");
            //solver.Impose<CmykPixel>("A.jpg", "B.jpg", 300, 70, resultImageFilename: "cmykResult.jpg");
            //solver.ImposeWithoutBlending("A.jpg", "B.jpg", 300, 70, resultImageFilename: "resultWithoutBlending.jpg");
            solver.ImposeWithoutBlending("A.jpg", "B.jpg", 300, 70, points, resultImageFilename: "resultWithoutBlending points.jpg");
        }
    }
}
