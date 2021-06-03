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
            var points = new Point[]
            {
                new Point(0, 0),
                new Point(20, 10),
                new Point(40, 25),
                new Point(60, 30),
                new Point(70, 25),
                new Point(85, 1),
                new Point(100, 26),
                new Point(145, 38),
                new Point(149, 42),
                new Point(149, 75),
                new Point(145, 80),
                new Point(130, 80),
                new Point(110, 80),
                new Point(10, 91),
                new Point(5, 85)
            };

            var points2 = new Point[]
            {
                new Point(0, 40),
                new Point(40, 0),
                new Point(110, 0),
                new Point(149, 40),
                new Point(149, 50),
                new Point(110, 89),
                new Point(40, 89),
            };

            //PoissonBlendingSolver.ImposeWithoutBlending(new ImposeWithoutBlendingOptions()
            //{
            //    BaseImageFilename = "1\\A.jpg",
            //    ImposingImageFilename = "1\\B.jpg",
            //    InsertPosition = new Point(300, 70),
            //    SelectedAreaPoints = points,
            //    ResultImageFilename = "1\\1.jpg",
            //});

            #region Methods
            await PoissonBlendingSolver.ImposeAsync(new ImposeOptions()
            {
                BaseImageFilename = "1\\A.jpg",
                ImposingImageFilename = "1\\B.jpg",
                InsertPosition = new Point(300, 70),
                SelectedAreaPoints = points,
                ResultImageFilename = "1\\Methods\\rgb async jacobi.jpg",
                LogProgressDelegate = (string message) => Console.WriteLine(message),
                ShowIntermediateProgress = false,
                SolverType = SolverType.Jacobi
            });

            await PoissonBlendingSolver.ImposeAsync(new ImposeOptions()
            {
                BaseImageFilename = "1\\A.jpg",
                ImposingImageFilename = "1\\B.jpg",
                InsertPosition = new Point(300, 70),
                SelectedAreaPoints = points,
                ResultImageFilename = "1\\Methods\\rgb async gauss-seidel.jpg",
                LogProgressDelegate = (string message) => Console.WriteLine(message),
                ShowIntermediateProgress = false,
                SolverType = SolverType.GaussSeidel
            });

            await PoissonBlendingSolver.ImposeAsync(new ImposeOptions()
            {
                BaseImageFilename = "1\\A.jpg",
                ImposingImageFilename = "1\\B.jpg",
                InsertPosition = new Point(300, 70),
                SelectedAreaPoints = points,
                ResultImageFilename = "1\\Methods\\rgb async sor.jpg",
                LogProgressDelegate = (string message) => Console.WriteLine(message),
                ShowIntermediateProgress = false,
                SolverType = SolverType.Sor
            });
            ///
            await PoissonBlendingSolver.ImposeAsync(new ImposeOptions()
            {
                BaseImageFilename = "1\\A2.jpg",
                ImposingImageFilename = "1\\B2.jpg",
                InsertPosition = new Point(320, 90),
                SelectedAreaPoints = points2,
                ResultImageFilename = "1\\Methods\\2 rgb async jacobi.jpg",
                LogProgressDelegate = (string message) => Console.WriteLine(message),
                ShowIntermediateProgress = false,
                SolverType = SolverType.Jacobi
            });

            await PoissonBlendingSolver.ImposeAsync(new ImposeOptions()
            {
                BaseImageFilename = "1\\A2.jpg",
                ImposingImageFilename = "1\\B2.jpg",
                InsertPosition = new Point(320, 90),
                SelectedAreaPoints = points2,
                ResultImageFilename = "1\\Methods\\2 rgb async gauss-seidel.jpg",
                LogProgressDelegate = (string message) => Console.WriteLine(message),
                ShowIntermediateProgress = false,
                SolverType = SolverType.GaussSeidel
            });

            await PoissonBlendingSolver.ImposeAsync(new ImposeOptions()
            {
                BaseImageFilename = "1\\A2.jpg",
                ImposingImageFilename = "1\\B2.jpg",
                InsertPosition = new Point(320, 90),
                SelectedAreaPoints = points2,
                ResultImageFilename = "1\\Methods\\2 rgb async sor.jpg",
                LogProgressDelegate = (string message) => Console.WriteLine(message),
                ShowIntermediateProgress = false,
                SolverType = SolverType.Sor
            });
            ///
            await PoissonBlendingSolver.ImposeAsync(new ImposeOptions()
            {
                BaseImageFilename = "1\\A3.jpg",
                ImposingImageFilename = "1\\B3.jpg",
                InsertPosition = new Point(500, 300),
                ResultImageFilename = "1\\Methods\\3 rgb async jacobi.jpg",
                LogProgressDelegate = (string message) => Console.WriteLine(message),
                ShowIntermediateProgress = false,
                SolverType = SolverType.Jacobi
            });

            await PoissonBlendingSolver.ImposeAsync(new ImposeOptions()
            {
                BaseImageFilename = "1\\A3.jpg",
                ImposingImageFilename = "1\\B3.jpg",
                InsertPosition = new Point(500, 300),
                ResultImageFilename = "1\\Methods\\3 rgb async gauss-seidel.jpg",
                LogProgressDelegate = (string message) => Console.WriteLine(message),
                ShowIntermediateProgress = false,
                SolverType = SolverType.GaussSeidel
            });

            await PoissonBlendingSolver.ImposeAsync(new ImposeOptions()
            {
                BaseImageFilename = "1\\A3.jpg",
                ImposingImageFilename = "1\\B3.jpg",
                InsertPosition = new Point(500, 300),
                ResultImageFilename = "1\\Methods\\3 rgb async sor.jpg",
                LogProgressDelegate = (string message) => Console.WriteLine(message),
                ShowIntermediateProgress = false,
                SolverType = SolverType.Sor
            });
            #endregion

            #region Color models
            //var points = new Point[]
            //{
            //    new Point(0, 0),
            //    new Point(20, 10),
            //    new Point(40, 25),
            //    new Point(60, 30),
            //    new Point(70, 25),
            //    new Point(85, 1),
            //    new Point(100, 26),
            //    new Point(145, 38),
            //    new Point(149, 42),
            //    new Point(149, 75),
            //    new Point(145, 80),
            //    new Point(130, 80),
            //    new Point(110, 80),
            //    new Point(10, 91),
            //    new Point(5, 85)
            //};
            //var options = new ImposeOptions()
            //{
            //    BaseImageFilename = "1\\A.jpg",
            //    ImposingImageFilename = "1\\B.jpg",
            //    InsertPosition = new Point(300, 70),
            //    SelectedAreaPoints = points,
            //    LogProgressDelegate = (string message) => Console.WriteLine(message),
            //};
            //options.ResultImageFilename = "1\\Color models\\async rgb.jpg";
            //await PoissonBlendingSolver.ImposeAsync<RgbPixel>(options);
            //options.ResultImageFilename = "1\\Color models\\async hsl.jpg";
            //await PoissonBlendingSolver.ImposeAsync<HslPixel>(options);
            //options.ResultImageFilename = "1\\Color models\\async cmy.jpg";
            //await PoissonBlendingSolver.ImposeAsync<CmyPixel>(options);
            //options.ResultImageFilename = "1\\Color models\\async cmyk.jpg";
            //await PoissonBlendingSolver.ImposeAsync<CmykPixel>(options);

            //(JacobiSolver, RgbPixel) => 0.003,
            //(JacobiSolver, HslPixel) => 0.000001,
            //(JacobiSolver, CmyPixel) => 0.000001,
            //(JacobiSolver, CmykPixel) => 0.000001,

            //var points = new Point[]
            //{
            //    new Point(0, 40),
            //    new Point(40, 0),
            //    new Point(110, 0),
            //    new Point(149, 40),
            //    new Point(149, 50),
            //    new Point(110, 89),
            //    new Point(40, 89),
            //};
            //var options = new ImposeOptions()
            //{
            //    BaseImageFilename = "1\\A2.jpg",
            //    ImposingImageFilename = "1\\B2.jpg",
            //    InsertPosition = new Point(320, 90),
            //    LogProgressDelegate = (string message) => Console.WriteLine(message),
            //    SelectedAreaPoints = points,
            //};
            //options.ResultImageFilename = "1\\Color models\\2 async rgb.jpg";
            //await PoissonBlendingSolver.ImposeAsync<RgbPixel>(options);
            //options.ResultImageFilename = "1\\Color models\\2 async hsl.jpg";
            //await PoissonBlendingSolver.ImposeAsync<HslPixel>(options);
            //options.ResultImageFilename = "1\\Color models\\2 async cmy.jpg";
            //await PoissonBlendingSolver.ImposeAsync<CmyPixel>(options);
            //options.ResultImageFilename = "1\\Color models\\2 async cmyk.jpg";
            //await PoissonBlendingSolver.ImposeAsync<CmykPixel>(options);

            //(SorSolver, RgbPixel) => 0.5,
            //(SorSolver, HslPixel) => 0.0001,
            //(SorSolver, CmyPixel) => 0.00001,
            //(SorSolver, CmykPixel) => 0.0001,

            //var options = new ImposeOptions()
            //{
            //    BaseImageFilename = "1\\A3.jpg",
            //    ImposingImageFilename = "1\\B3.jpg",
            //    InsertPosition = new Point(500, 300),
            //    LogProgressDelegate = (string message) => Console.WriteLine(message),
            //};
            //options.ResultImageFilename = "1\\Color models\\3 async rgb.jpg";
            //await PoissonBlendingSolver.ImposeAsync<RgbPixel>(options);
            //options.ResultImageFilename = "1\\Color models\\3 async hsl.jpg";
            //await PoissonBlendingSolver.ImposeAsync<HslPixel>(options);
            //options.ResultImageFilename = "1\\Color models\\3 async cmy.jpg";
            //await PoissonBlendingSolver.ImposeAsync<CmyPixel>(options);
            //options.ResultImageFilename = "1\\Color models\\3 async cmyk.jpg";
            //await PoissonBlendingSolver.ImposeAsync<CmykPixel>(options);
            #endregion




            //var solver = new PoissonBlendingSolver((string message) => Console.WriteLine(message))
            //{
            //    ShowIntermediateProgress = true
            //};
            //var points = new Point[]
            //{
            //    new Point(0, 0),
            //    new Point(50, 20),
            //    new Point(70, 30),
            //    new Point(85, 1), // 2 пика
            //    new Point(100, 31),
            //    new Point(149, 41),
            //    new Point(149, 91),
            //    new Point(0, 91)
            //};
            //await solver.ImposeAsync("A.jpg", "B.jpg", 300, 70, points, resultImageFilename: "rgbResultAsync points.jpg");
            //solver.Impose("A.jpg", "B.jpg", 300, 70, points, resultImageFilename: "rgbResult points.jpg");
            //solver.Impose("A.jpg", "B.jpg", 300, 70, resultImageFilename: "rgbResult.jpg");
            //solver.Impose<HslPixel>("A.jpg", "B.jpg", 300, 70, resultImageFilename: "hslResult.jpg");
            //solver.Impose<CmyPixel>("A.jpg", "B.jpg", 300, 70, resultImageFilename: "cmyResult.jpg");
            //solver.Impose<CmykPixel>("A.jpg", "B.jpg", 300, 70, resultImageFilename: "cmykResult.jpg");
            //solver.ImposeWithoutBlending("A.jpg", "B.jpg", 300, 70, resultImageFilename: "resultWithoutBlending.jpg");
            //solver.ImposeWithoutBlending("A.jpg", "B.jpg", 300, 70, points, resultImageFilename: "resultWithoutBlending points.jpg");


            //await solver.ImposeAsync("A.jpg", "B.jpg", 300, 70, points, resultImageFilename: "guidanceField.jpg", guidanceFieldType: GuidanceFieldType.LinearCombination);

            //await solver.ImposeAsync("A3.png", "B3.jpg", 50, 50, resultImageFilename: "monochrome 3.jpg");
            //await solver.ImposeAsync("A33.png", "B3M2.jpg", 50, 50, resultImageFilename: "monochrome 6.jpg");



            //await solver.ImposeAsync("A4.jpg", "B4.png", 50, 50, resultImageFilename: "ring 1.jpg", guidanceFieldType: GuidanceFieldType.Normal);
            //await solver.ImposeAsync("A4.jpg", "B4.png", 50, 50, resultImageFilename: "ring 2.jpg", guidanceFieldType: GuidanceFieldType.LinearCombination);
            //await solver.ImposeAsync("A4.jpg", "B4.png", 50, 50, resultImageFilename: "ring 3.jpg", guidanceFieldType: GuidanceFieldType.Mixed);
            //await solver.ImposeAsync("A4.jpg", "B5.png", 50, 50, resultImageFilename: "ring 4.jpg", guidanceFieldType: GuidanceFieldType.Mixed);


            var points1 = new Point[]
            {
                new Point(5, 35),
                new Point(20, 30),
                new Point(150, 20),
                new Point(165, 5),
                new Point(175, 7),
                new Point(190, 25),
                new Point(190, 42),
                new Point(155, 80),
                new Point(155, 90),
                new Point(175, 95),
                new Point(175, 110),
                new Point(150, 110),
                new Point(140, 190),
                new Point(110, 190),
                new Point(105, 165),
                new Point(50, 160)
            };

            //await solver.ImposeAsync("A6.png", "B6.png", 0, 0, points1, resultImageFilename: "ring 5.jpg", guidanceFieldType: GuidanceFieldType.Mixed);
            //await solver.ImposeAsync("A6.png", "B6.png", 0, 0, points1, resultImageFilename: "ring 6.jpg", guidanceFieldType: GuidanceFieldType.LinearCombination);

            //solver.ImposeWithoutBlending("A6.png", "B6.png", 0, 0, points1, resultImageFilename: "ring 0.jpg");


            //var points2 = new Point[]
            //{
            //    new Point(260, 0),
            //    new Point(399, 0),
            //    new Point(399, 80),
            //    new Point(300, 210),
            //    new Point(105, 205),
            //    new Point(190, 100),
            //};
            //await solver.ImposeAsync("A7.jpg", "B7.jpg", 0, 0, points2, resultImageFilename: "transparent 1.jpg");
            //await solver.ImposeAsync("A7.jpg", "B7.jpg", 0, 0, points2, resultImageFilename: "transparent 2.jpg", guidanceFieldType: GuidanceFieldType.Mixed);


            var points3 = new Point[]
            {
                new Point(149, 15),
                new Point(149, 30),
                new Point(85, 60),
                new Point(60, 90),
                new Point(8, 90),
                new Point(8, 30),
            };
            //await solver.ImposeAsync("A8.png", "B8.jpg", 65, 90, points3, resultImageFilename: "near 1.jpg");
            //await solver.ImposeAsync("A8.png", "B8.jpg", 65, 90, points3, resultImageFilename: "near 2.jpg", guidanceFieldType: GuidanceFieldType.Mixed);
        }
    }
}
