using PoissonBlending.Lib.PixelDescription;
using PoissonBlending.Lib.Solver;
using System;
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
                        imageA.SetPixel(options.InsertPosition.x + x, options.InsertPosition.y + y, imageB.GetPixel(x, y));
                    }
                }
            }

            if (options.SaveResultImage)
            {
                imageA.Save(options.ResultImageFilename);
            }

            return new Bitmap(imageA);
        }

        public static Bitmap Impose(ImposeOptions options) => Impose<RgbPixel>(options);

        public static async Task<Bitmap> ImposeAsync(ImposeOptions options) => await ImposeAsync<RgbPixel>(options).ConfigureAwait(false);

        public static Bitmap Impose<Pixel>(ImposeOptions options) where Pixel : IPixel, new()
        {
            options.GetLogService().LogStarted(typeof(Pixel).Name, false);

            var watch = Stopwatch.StartNew();

            using var imageA = new Bitmap(options.BaseImageFilename);
            using var imageB = new Bitmap(options.ImposingImageFilename);

            var mask = new Mask<Pixel>(options.SelectedAreaPoints, imageB.Width, imageB.Height);

            var resultImage = CreateResultBitmap(options, imageA, imageB, mask);

            if (options.SaveResultImage)
            {
                resultImage.Save(options.ResultImageFilename);
            }

            watch.Stop();
            options.GetLogService().LogProcessResult(watch.ElapsedMilliseconds);

            return new Bitmap(resultImage);
        }

        public static async Task<Bitmap> ImposeAsync<Pixel>(ImposeOptions options) where Pixel : IPixel, new()
        {
            options.GetLogService().LogStarted(typeof(Pixel).Name, true);

            var watch = Stopwatch.StartNew();

            using var imageA = new Bitmap(options.BaseImageFilename);
            using var imageB = new Bitmap(options.ImposingImageFilename);

            var mask = new Mask<Pixel>(options.SelectedAreaPoints, imageB.Width, imageB.Height);

            var resultImage = await CreateResultBitmapAsync(options, imageA, imageB, mask).ConfigureAwait(false);

            if (options.SaveResultImage)
            {
                resultImage.Save(options.ResultImageFilename);
            }

            watch.Stop();
            options.GetLogService().LogProcessResult(watch.ElapsedMilliseconds);

            return new Bitmap(resultImage);
        }

        /// <summary>
        /// Составляет результирующее изображение.
        /// </summary>
        /// <param name="options">Параметры наложения.</param>
        /// <param name="imageA">Базовое изображение.</param>
        /// <param name="imageB">Накладываемое изображение.</param>
        /// <param name="mask">Маска изображений.</param>
        /// <returns>Результирующее изображение <see cref="Bitmap"/>.</returns>
        private static Bitmap CreateResultBitmap<Pixel>(ImposeOptions options, Bitmap imageA, Bitmap imageB, Mask<Pixel> mask) where Pixel : IPixel, new()
        {
            AddGuidanceFieldProjection(imageA, imageB, mask, options.GuidanceFieldType);

            AddBorderColors(imageA, options.InsertPosition.x, options.InsertPosition.y, mask);

            var solvedPixels = options.GetSolver().Solve(mask);

            for (var i = 0; i < mask.Pixels.Length; i++)
            {
                (var x, var y) = mask.PixelsMap[i];
                imageA.SetPixel(options.InsertPosition.x + x + mask.OffsetX, options.InsertPosition.y + y + mask.OffsetY, solvedPixels[i].ToColor());
            }

            return imageA;
        }

        /// <summary>
        /// Составляет результирующее изображение.
        /// </summary>
        /// <param name="options">Параметры наложения.</param>
        /// <param name="imageA">Базовое изображение.</param>
        /// <param name="imageB">Накладываемое изображение.</param>
        /// <param name="mask">Маска изображений.</param>
        /// <returns>Результирующее изображение <see cref="Bitmap"/>.</returns>
        private static async Task<Bitmap> CreateResultBitmapAsync<Pixel>(ImposeOptions options, Bitmap imageA, Bitmap imageB, Mask<Pixel> mask) where Pixel : IPixel, new()
        {
            AddGuidanceFieldProjection(imageA, imageB, mask, options.GuidanceFieldType);

            AddBorderColors(imageA, options.InsertPosition.x, options.InsertPosition.y, mask);

            var solvedPixels = await options.GetSolver().SolveAsync(mask).ConfigureAwait(false);

            for (var i = 0; i < mask.Pixels.Length; i++)
            {
                (var x, var y) = mask.PixelsMap[i];
                imageA.SetPixel(options.InsertPosition.x + x + mask.OffsetX, options.InsertPosition.y + y + mask.OffsetY, solvedPixels[i].ToColor());
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
