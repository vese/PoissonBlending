using PoissonBlending.Lib.Models;
using PoissonBlending.Lib.PixelDescription;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace PoissonBlending.Lib
{
    public delegate void LogProgressDelegate(string message);

    public static class PoissonBlendingSolver
    {
        public static readonly string DefaultResultFilename = "result.jpg";

        public static Bitmap ImposeWithoutBlending(ImposeWithoutBlendingOptions options)
        {
            using var imageA = new Bitmap(options.BaseImageFilename);
            using var imageB = new Bitmap(options.ImposingImageFilename);

            var mask = new Mask<RgbPixel>(options.SelectedAreaPoints, imageB.Width, imageB.Height);

            for (var i = 0; i < mask.Height; i++)
            {
                for (var j = 0; j < mask.Width; j++)
                {
                    if (mask.FullMask[i, j])
                    {
                        var x = j + mask.OffsetX;
                        var y = i + mask.OffsetY;
                        imageA.SetPixel(options.InsertPosition.X + x, options.InsertPosition.Y + y, imageB.GetPixel(x, y));
                    }
                }
            }

            if (options.SaveResultImage)
            {
                imageA.Save(options.ResultImageFilename);
            }

            return new Bitmap(imageA);
        }

        public static ImposeResult Impose(ImposeOptions options) => Impose<RgbPixel>(options);

        public static async Task<ImposeResult> ImposeAsync(ImposeOptions options) => await ImposeAsync<RgbPixel>(options).ConfigureAwait(false);

        public static ImposeResult Impose<Pixel>(ImposeOptions options) where Pixel : IPixel, new()
        {
            options.GetLogService().LogStarted(typeof(Pixel).Name, false);

            var watch = Stopwatch.StartNew();

            using var imageA = new Bitmap(options.BaseImageFilename);
            using var imageB = new Bitmap(options.ImposingImageFilename);

            var mask = new Mask<Pixel>(options.SelectedAreaPoints, imageB.Width, imageB.Height);

            AddGuidanceFieldProjection(imageA, imageB, mask, options.GuidanceFieldType);

            AddBorderColors(imageA, options.InsertPosition.X, options.InsertPosition.Y, mask);

            var solveResult = options.GetSolver().Solve(mask);

            var resultImage = CreateResultBitmap(options, imageA, mask, solveResult.result);

            if (options.SaveResultImage)
            {
                resultImage.Save(options.ResultImageFilename);
            }

            watch.Stop();

            var averagePixels = ComputeAveragePixels(options, imageA, imageB, mask, solveResult.result);

            options.GetLogService().LogProcessResult(watch.ElapsedMilliseconds);

            return new ImposeResult()
            {
                Image = new Bitmap(resultImage),
                ImposeTimeMilliseconds = watch.ElapsedMilliseconds,
                ColorComponentSolveTimes = solveResult.times,
                ChangeResult = (averagePixels.changeA, averagePixels.changeB),
                AverageBorderPixelA = averagePixels.borderA,
                AverageBorderPixelC = averagePixels.borderC,
                AverageInternalPixelB = averagePixels.internalB,
                AverageInternalPixelC = averagePixels.internalC
            };
        }

        public static async Task<ImposeResult> ImposeAsync<Pixel>(ImposeOptions options) where Pixel : IPixel, new()
        {
            options.GetLogService().LogStarted(typeof(Pixel).Name, true);

            var watch = Stopwatch.StartNew();

            using var imageA = new Bitmap(options.BaseImageFilename);
            using var imageB = new Bitmap(options.ImposingImageFilename);

            var mask = new Mask<Pixel>(options.SelectedAreaPoints, imageB.Width, imageB.Height);

            AddGuidanceFieldProjection(imageA, imageB, mask, options.GuidanceFieldType);

            AddBorderColors(imageA, options.InsertPosition.X, options.InsertPosition.Y, mask);

            var solveResult = await options.GetSolver().SolveAsync(mask).ConfigureAwait(false);

            var resultImage = CreateResultBitmap(options, imageA, mask, solveResult.result);

            if (options.SaveResultImage)
            {
                resultImage.Save(options.ResultImageFilename);
            }

            watch.Stop();

            var averagePixels = ComputeAveragePixels(options, imageA, imageB, mask, solveResult.result);

            options.GetLogService().LogProcessResult(watch.ElapsedMilliseconds);

            return new ImposeResult()
            {
                Image = new Bitmap(resultImage),
                ImposeTimeMilliseconds = watch.ElapsedMilliseconds,
                ColorComponentSolveTimes = solveResult.times,
                ChangeResult = (averagePixels.changeA, averagePixels.changeB),
                AverageBorderPixelA = averagePixels.borderA,
                AverageBorderPixelC = averagePixels.borderC,
                AverageInternalPixelB = averagePixels.internalB,
                AverageInternalPixelC = averagePixels.internalC
            };
        }

        private static List<(int x, int y)> GetNearPixels<Pixel>(int x, int y, int radius) where Pixel : IPixel, new()
        {
            var neigbors = Mask<Pixel>.GetNeighbors(x, y);
            for (int i = 0; i < radius - 1; i++)
            {
                neigbors = neigbors.SelectMany(p => Mask<Pixel>.GetNeighbors(x, y)).ToList();
            }
            return neigbors.Distinct().ToList();
        }

        private static (double changeA, double changeB, Dictionary<string, double> borderA, Dictionary<string, double> borderC, 
            Dictionary<string, double> internalB, Dictionary<string, double> internalC)
            ComputeAveragePixels<Pixel>(ImposeOptions options, Bitmap imageA, Bitmap imageB, Mask<Pixel> mask, 
            PixelArray<Pixel> solvedPixels) where Pixel : IPixel, new()
        {
            int radius = 3;

            var resultA = new Pixel();
            var resultB = new Pixel();
            var solvedResult = new Pixel();
            var solvedResultInternal = new Pixel();
            var count = 0;
            var countInternal = 0;
            for (var i = 0; i < mask.Pixels.Length; i++)
            {
                (var x, var y) = mask.PixelsMap[i];
                if (mask.BorderMask[y, x] || GetNearPixels<Pixel>(x, y, radius).Where(p => p.x > 0 && p.x < mask.Width && p.y > 0 && p.y < mask.Height).Any(p => mask.BorderMask[p.y, p.x]))
                {
                    count++;
                    solvedResult.Add(solvedPixels[i]);
                    //resultB.Add(new Pixel().FromColor(imageB.GetPixel(x + mask.OffsetX, y + mask.OffsetY)));
                }
                else if (mask.BorderlessMask[y, x])
                {
                    countInternal++;
                    solvedResultInternal.Add(solvedPixels[i]);
                    resultB.Add(new Pixel().FromColor(imageB.GetPixel(x + mask.OffsetX, y + mask.OffsetY)));
                }
            }

            var multiplier = 1d / count;
            solvedResult.Multiply(multiplier);

            multiplier = 1d / countInternal;
            solvedResultInternal.Multiply(multiplier);
            resultB.Multiply(multiplier);

            count = 0;
            for (int y = 0; y < mask.Height; y++)
            {
                for (int x = 0; x < mask.Width; x++)
                {
                    if (mask.BorderMask[y, x] || GetNearPixels<Pixel>(x, y, radius).Where(p => p.x > 0 && p.x < mask.Width && p.y > 0 && p.y < mask.Height).Any(p => mask.BorderMask[p.y, p.x]))
                    {
                        count++;
                        var pixel = new Pixel().FromColor(imageA.GetPixel(options.InsertPosition.X + x + mask.OffsetX, options.InsertPosition.Y + y + mask.OffsetY));
                        resultA.Add(pixel);
                    }
                }
            }

            multiplier = 1d / count;
            resultA.Multiply(multiplier);

            double changeA = 0;
            double changeB = 0;
            var colorComponents = solvedResult.GetColorComponentsNames();
            double maxValue = new Pixel() switch
            {
                RgbPixel => 255,
                _ => 1
            };
            var averageBorderPixelA = new Dictionary<string, double>();
            var averageBorderPixelC = new Dictionary<string, double>();
            var averageInternalPixelB = new Dictionary<string, double>();
            var averageInternalPixelC = new Dictionary<string, double>();
            foreach (var colorComponent in colorComponents)
            {
                changeA += (solvedResult[colorComponent] - resultA[colorComponent]) / maxValue;
                changeB += (solvedResultInternal[colorComponent] - resultB[colorComponent]) / maxValue;

                averageBorderPixelA.Add(colorComponent, resultA[colorComponent]);
                averageBorderPixelC.Add(colorComponent, solvedResult[colorComponent]);
                averageInternalPixelB.Add(colorComponent, resultB[colorComponent]);
                averageInternalPixelC.Add(colorComponent, solvedResultInternal[colorComponent]);
            }
            changeA /= colorComponents.Count;
            changeB /= colorComponents.Count;

            return (changeA, changeB, averageBorderPixelA, averageBorderPixelC, averageInternalPixelB, averageInternalPixelC);
        }

        /// <summary>
        /// Составляет результирующее изображение.
        /// </summary>
        /// <param name="options">Параметры наложения.</param>
        /// <param name="imageA">Базовое изображение.</param>
        /// <param name="mask">Маска изображений.</param>
        /// <returns>Результирующее изображение <see cref="Bitmap"/>.</returns>
        private static Bitmap CreateResultBitmap<Pixel>(ImposeOptions options, Bitmap imageA, Mask<Pixel> mask, PixelArray<Pixel> solvedPixels) where Pixel : IPixel, new()
        {
            for (var i = 0; i < mask.Pixels.Length; i++)
            {
                (var x, var y) = mask.PixelsMap[i];
                imageA.SetPixel(options.InsertPosition.X + x + mask.OffsetX, options.InsertPosition.Y + y + mask.OffsetY, solvedPixels[i].ToColor());
            }

            return imageA;
        }

        /// <summary>
        /// Создает двумерный массив числовых проекций поля направлений для накладываемого изображения.
        /// </summary>
        /// <param name="imageA">Базовое изображение.</param>
        /// <param name="imageB">Накладываемое изображение.</param>
        /// <param name="mask">Маска изображений.</param>
        /// <param name="guidanceFieldType">Тип направляющего поля.</param>
        private static void AddGuidanceFieldProjection<Pixel>(Bitmap imageA, Bitmap imageB, Mask<Pixel> mask, GuidanceFieldType guidanceFieldType) where Pixel : IPixel, new()
        {
            for (var i = 0; i < mask.PixelsMap.Length; i++)
            {
                (var x, var y) = mask.PixelsMap[i];
                var pixelA = imageA.GetPixel(x + mask.OffsetX, y + mask.OffsetY);
                var pixelB = imageB.GetPixel(x + mask.OffsetX, y + mask.OffsetY);
                var projectionPixels = Mask<Pixel>.GetNeighbors(x, y).Select(p =>
                    new Pixel().FromColor(pixelB).Minus(
                        new Pixel().FromColor(imageB.GetPixel(p.x + mask.OffsetX, p.y + mask.OffsetY)))).ToList();

                if (guidanceFieldType == GuidanceFieldType.LinearCombination)
                {
                    var projectionPixelsA = Mask<Pixel>.GetNeighbors(x, y).Select(p =>
                        new Pixel().FromColor(pixelA).Minus(
                            new Pixel().FromColor(imageA.GetPixel(p.x + mask.OffsetX, p.y + mask.OffsetY)))).ToList();

                    for (int j = 0; j < projectionPixels.Count; j++)
                    {
                        projectionPixels[j] = projectionPixels[j].Multiply(0.5).Add(projectionPixelsA[j].Multiply(0.5));
                    }
                }

                if (guidanceFieldType == GuidanceFieldType.Mixed)
                {
                    var projectionPixelsA = Mask<Pixel>.GetNeighbors(x, y).Select(p =>
                        new Pixel().FromColor(pixelA).Minus(
                            new Pixel().FromColor(imageA.GetPixel(p.x + mask.OffsetX, p.y + mask.OffsetY)))).ToList();

                    for (int j = 0; j < projectionPixels.Count; j++)
                    {
                        projectionPixels[j] = projectionPixels[j].Norm > projectionPixelsA[j].Norm ?
                            projectionPixels[j] : projectionPixelsA[j];
                    }
                }

                projectionPixels.ForEach(pixel => mask.Pixels[i].Add(pixel));
            }
        }

        /// <summary>
        /// Составляет набор накладываемых пикселей и соседей. К пикселям прибавляется значение проекции градиента.  
        /// </summary>
        /// <param name="imageA">Базовое изображение.</param>
        /// <param name="insertX">Позиция x наложения.</param>
        /// <param name="insertY">Позиция y наложения.</param>
        /// <param name="mask">Маска изображений.</param>
        private static void AddBorderColors<Pixel>(Bitmap imageA, int insertX, int insertY, Mask<Pixel> mask) where Pixel : IPixel, new()
        {
            for (var i = 0; i < mask.PixelsMap.Length; i++)
            {
                (var x, var y) = mask.PixelsMap[i];
                Mask<Pixel>.GetNeighbors(x, y).Where(p => mask.BorderMask[p.y, p.x]).ToList().ForEach(p =>
                {
                    if (mask.BorderMask[p.y, p.x])
                    {
                        mask.Pixels[i].Add(new Pixel().FromColor(imageA.GetPixel(insertX + p.x + mask.OffsetX, insertY + p.y + mask.OffsetY)));
                    }
                });
            }
        }
    }
}
