using PoissonBlending.Lib.PixelDescription;
using PoissonBlending.Lib.Solver;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;

namespace PoissonBlending.Lib
{
    public delegate void LogProgress(string message);

    public class PoissonBlendingSolver
    {
        private readonly LogProgress logProgress;

        private readonly ISolver solver;

        public const string DefaultResultFilename = "result.jpg";

        public bool ShowIntermediateProgress { get; set; } = false;

        public PoissonBlendingSolver(LogProgress logProgress = null)
        {
            this.logProgress = logProgress;
            solver = new JacobiSolver(LogSolveProgress);
        }

        public Bitmap ImposeWithoutBlending(string baseImageFilename, string imposingImageFilename, int insertX, int insertY,
            bool saveResultImage = true, string resultImageFilename = DefaultResultFilename)
        {
            using var imageA = new Bitmap(baseImageFilename);
            using var imageB = new Bitmap(imposingImageFilename);

            for (int i = 0; i < imageB.Height; i++)
            {
                for (int j = 1; j < imageB.Width; j++)
                {
                    imageA.SetPixel(insertX + j, insertY + i, imageB.GetPixel(j, i));
                }
            }

            if (saveResultImage)
            {
                imageA.Save(resultImageFilename);
            }

            return new Bitmap(imageA);
        }

        public Bitmap Impose(string baseImageFilename, string imposingImageFilename, int insertX, int insertY,
            bool saveResultImage = true, string resultImageFilename = DefaultResultFilename) =>
            Impose<RgbPixel>(baseImageFilename, imposingImageFilename, insertX, insertY, saveResultImage, resultImageFilename);

        public async Task<Bitmap> ImposeAsync(string baseImageFilename, string imposingImageFilename, int insertX, int insertY,
            bool saveResultImage = true, string resultImageFilename = DefaultResultFilename) =>
            await ImposeAsync<RgbPixel>(baseImageFilename, imposingImageFilename, insertX, insertY, saveResultImage, resultImageFilename).ConfigureAwait(false);

        public Bitmap Impose<Pixel>(string baseImageFilename, string imposingImageFilename, int insertX, int insertY,
            bool saveResultImage = true, string resultImageFilename = DefaultResultFilename) where Pixel : BasePixel, new()
        {
            LogStarted(typeof(Pixel).Name, false);

            var watch = Stopwatch.StartNew();

            using var imageA = new Bitmap(baseImageFilename);
            using var imageB = new Bitmap(imposingImageFilename);

            var resultImage = CreateResultBitmap<Pixel>(imageA, imageB, insertX, insertY);

            if (saveResultImage)
            {
                resultImage.Save(resultImageFilename);
            }

            watch.Stop();
            LogProcessResult(watch.ElapsedMilliseconds);

            return new Bitmap(resultImage);
        }

        public async Task<Bitmap> ImposeAsync<Pixel>(string baseImageFilename, string imposingImageFilename, int insertX, int insertY,
            bool saveResultImage = true, string resultImageFilename = DefaultResultFilename) where Pixel : BasePixel, new()
        {
            LogStarted(typeof(Pixel).Name, true);

            var watch = Stopwatch.StartNew();

            using var imageA = new Bitmap(baseImageFilename);
            using var imageB = new Bitmap(imposingImageFilename);

            var resultImage = await CreateResultBitmapAsync<Pixel>(imageA, imageB, insertX, insertY).ConfigureAwait(false);

            if (saveResultImage)
            {
                resultImage.Save(resultImageFilename);
            }

            watch.Stop();
            LogProcessResult(watch.ElapsedMilliseconds);

            return new Bitmap(resultImage);
        }

        #region Private functions

        /// <summary>
        /// Составляет результирующее изображение.
        /// </summary>
        /// <param name="imageA">Базовое изображение.</param>
        /// <param name="imageB">Накладываемое изображение.</param>
        /// <param name="insertX">Позиция x наложения.</param>
        /// <param name="insertY">Позиция y наложения.</param>
        /// <returns>Результирующее изображение <see cref="Bitmap"/>.</returns>
        private Bitmap CreateResultBitmap<Pixel>(Bitmap imageA, Bitmap imageB, int insertX, int insertY) where Pixel : BasePixel, new()
        {
            var guidanceFieldProjection = CreateGuidanceFieldProjection<Pixel>(imageB);

            (var pixels, var neighbors) = GetPixelsWithNeighboards(imageA, imageB, insertX, insertY, guidanceFieldProjection);

            var solvedPixels = solver.Solve(pixels, neighbors);

            var resultImagePixels = GetResultImageBorderPixels<Pixel>(imageA, imageB, insertX, insertY);

            int insertHeight = imageB.Height, insertWidth = imageB.Width;

            for (int i = 0; i < insertHeight - 2; i++)
            {
                for (int j = 0; j < insertWidth - 2; j++)
                {
                    resultImagePixels[i + 1, j + 1] = solvedPixels[i * (insertWidth - 2) + j];
                }
            }

            for (int i = 0; i < insertHeight; i++)
            {
                for (int j = 1; j < insertWidth; j++)
                {
                    imageA.SetPixel(insertX + j, insertY + i, resultImagePixels[i, j].ToColor());
                }
            }

            return imageA;
        }

        /// <summary>
        /// Составляет результирующее изображение.
        /// </summary>
        /// <param name="imageA">Базовое изображение.</param>
        /// <param name="imageB">Накладываемое изображение.</param>
        /// <param name="insertX">Позиция x наложения.</param>
        /// <param name="insertY">Позиция y наложения.</param>
        /// <returns>Результирующее изображение <see cref="Bitmap"/>.</returns>
        private async Task<Bitmap> CreateResultBitmapAsync<Pixel>(Bitmap imageA, Bitmap imageB, int insertX, int insertY) where Pixel : BasePixel, new()
        {
            var guidanceFieldProjection = CreateGuidanceFieldProjection<Pixel>(imageB);

            (var pixels, var neighbors) = GetPixelsWithNeighboards(imageA, imageB, insertX, insertY, guidanceFieldProjection);

            var solvedPixels = await solver.SolveAsync(pixels, neighbors).ConfigureAwait(false);

            var resultImagePixels = GetResultImageBorderPixels<Pixel>(imageA, imageB, insertX, insertY);

            int insertHeight = imageB.Height, insertWidth = imageB.Width;

            for (int i = 0; i < insertHeight - 2; i++)
            {
                for (int j = 0; j < insertWidth - 2; j++)
                {
                    resultImagePixels[i + 1, j + 1] = solvedPixels[i * (insertWidth - 2) + j];
                }
            }

            for (int i = 0; i < insertHeight; i++)
            {
                for (int j = 1; j < insertWidth; j++)
                {
                    imageA.SetPixel(insertX + j, insertY + i, resultImagePixels[i, j].ToColor());
                }
            }

            return imageA;
        }

        #region Static

        /// <summary>
        /// Создает двумерный массив пикселей для результирующего изображения и заполняет границы пикселями базового изображения.
        /// </summary>
        /// <param name="imageA">Базовое изображение.</param>
        /// <param name="imageB">Накладываемое изображение.</param>
        /// <param name="insertX">Позиция x наложения.</param>
        /// <param name="insertY">Позиция y наложения.</param>
        /// <returns>Двумерный массив <see cref="BasePixel"/> с заполненными граничными пикселями из базового изображения.</returns>
        private static Pixel[,] GetResultImageBorderPixels<Pixel>(Bitmap imageA, Bitmap imageB, int insertX, int insertY) where Pixel : BasePixel, new()
        {
            int insertHeight = imageB.Height, insertWidth = imageB.Width;
            var resultImagePixels = new Pixel[insertHeight, insertWidth];
            for (int j = 0; j < insertWidth; j++)
            {
                resultImagePixels[0, j] = (Pixel)new Pixel().FromColor(imageA.GetPixel(insertX + j, insertY));
                resultImagePixels[insertHeight - 1, j] = (Pixel)new Pixel().FromColor(imageA.GetPixel(insertX + j, insertY + insertHeight - 1));
            }
            for (int i = 1; i < insertHeight - 1; i++)
            {
                resultImagePixels[i, 0] = (Pixel)new Pixel().FromColor(imageA.GetPixel(insertY, insertY + i));
                resultImagePixels[i, insertWidth - 1] = (Pixel)new Pixel().FromColor(imageA.GetPixel(insertX + insertWidth - 1, insertY + i));
            }
            return resultImagePixels;
        }

        /// <summary>
        /// Создает двумерный массив числовых проекций поля направлений для накладываемого изображения.
        /// </summary>
        /// <param name="imageB">Накладываемое изображение.</param>
        /// <returns>Двумерный массив <see cref="Pixel"/> проекций поля направлений.</returns>
        private static Pixel[,] CreateGuidanceFieldProjection<Pixel>(Bitmap imageB) where Pixel : BasePixel, new()
        {
            int insertHeight = imageB.Height, insertWidth = imageB.Width;
            var guidanceFieldProjection = new Pixel[insertHeight, insertWidth];
            for (int i = 0; i < insertHeight; i++)
            {
                for (int j = 0; j < insertWidth; j++)
                {
                    guidanceFieldProjection[i, j] = new Pixel();
                    if (i > 0)
                    {
                        guidanceFieldProjection[i, j].Add(new Pixel().FromColor(imageB.GetPixel(j, i)).Minus(new Pixel().FromColor(imageB.GetPixel(j, i - 1))));
                    }
                    if (j > 0)
                    {
                        guidanceFieldProjection[i, j].Add(new Pixel().FromColor(imageB.GetPixel(j, i)).Minus(new Pixel().FromColor(imageB.GetPixel(j - 1, i))));
                    }
                    if (i < insertHeight - 1)
                    {
                        guidanceFieldProjection[i, j].Add(new Pixel().FromColor(imageB.GetPixel(j, i)).Minus(new Pixel().FromColor(imageB.GetPixel(j, i + 1))));
                    }
                    if (j < insertWidth - 1)
                    {
                        guidanceFieldProjection[i, j].Add(new Pixel().FromColor(imageB.GetPixel(j, i)).Minus(new Pixel().FromColor(imageB.GetPixel(j + 1, i))));
                    }
                }
            }
            return guidanceFieldProjection;
        }

        /// <summary>
        /// Составляет набор накладываемых пикселей и соседей. К пикселям прибавляется значение проекции градиента.  
        /// </summary>
        /// <param name="imageA">Базовое изображение.</param>
        /// <param name="imageB">Накладываемое изображение.</param>
        /// <param name="insertX">Позиция x наложения.</param>
        /// <param name="insertY">Позиция y наложения.</param>
        /// <param name="guidanceFieldProjection">Проекции поля направлений.</param>
        /// <returns>Массив пикселей <see cref="Pixel"/> и массив индексов соседних пикселей.</returns>
        private static (PixelArray<Pixel> pixels, List<int>[] neighbors) GetPixelsWithNeighboards<Pixel>(Bitmap imageA, Bitmap imageB, int insertX, int insertY, Pixel[,] guidanceFieldProjection) where Pixel : BasePixel, new()
        {
            int insertHeight = imageB.Height, insertWidth = imageB.Width;
            var neighbors = new List<int>[(insertHeight - 2) * (insertWidth - 2)];
            for (int i = 0; i < (insertHeight - 2) * (insertWidth - 2); i++)
            {
                neighbors[i] = new List<int>();
            }
            var pixels = new PixelArray<Pixel>((insertHeight - 2) * (insertWidth - 2));
            for (int i = 0; i < (insertHeight - 2) * (insertWidth - 2); i++)
            {
                pixels[i] = new Pixel();
            }
            for (int i = 0; i < insertHeight - 2; i++)
            {
                for (int j = 0; j < insertWidth - 2; j++)
                {
                    int index = i * (insertWidth - 2) + j;
                    if (i > 0)
                    {
                        neighbors[index].Add((i - 1) * (insertWidth - 2) + j);
                    }
                    else
                    {
                        pixels[index].Add(new Pixel().FromColor(imageA.GetPixel(insertX + j + 1, insertY + i)));
                    }
                    if (j > 0)
                    {
                        neighbors[index].Add(i * (insertWidth - 2) + j - 1);
                    }
                    else
                    {
                        pixels[index].Add(new Pixel().FromColor(imageA.GetPixel(insertX + j, insertY + i + 1)));
                    }
                    if (i + 1 < insertHeight - 2)
                    {
                        neighbors[index].Add((i + 1) * (insertWidth - 2) + j);
                    }
                    else
                    {
                        pixels[index].Add(new Pixel().FromColor(imageA.GetPixel(insertX + j + 1, insertY + i + 2)));
                    }
                    if (j + 1 < insertWidth - 2)
                    {
                        neighbors[index].Add(i * (insertWidth - 2) + j + 1);
                    }
                    else
                    {
                        pixels[index].Add(new Pixel().FromColor(imageA.GetPixel(insertX + j + 2, insertY + i + 1)));
                    }
                    pixels[index].Add(guidanceFieldProjection[i + 1, j + 1]);
                }
            }

            return (pixels, neighbors);
        }

        #endregion

        #region Log
        /// <summary>
        /// Логирование результата наложения.
        /// </summary>
        /// <param name="elapsedMs">Время выполнения в миллисекундах.</param>
        private void LogStarted(string colorModel, bool isAsync)
        {
            if (logProgress == null)
            {
                return;
            }

            logProgress($"{(isAsync ? "Async b" : "B")}lending started for {colorModel} color model.");
        }

        /// <summary>
        /// Логирование результатов шага решения.
        /// </summary>
        /// <param name="colorComponentName">Название цветовой компоненты.</param>
        /// <param name="iteration">Номер шага.</param>
        /// <param name="error">Оценка погрешности.</param>
        private void LogSolveProgress(string colorComponentName, int iteration, double error, long? elapsedMs)
        {
            if (logProgress == null)
            {
                return;
            }

            if (elapsedMs.HasValue)
            {
                logProgress($"Blending finished in {elapsedMs}ms; Color component: {colorComponentName}; Iterations: {iteration}.");
            }
            else if (ShowIntermediateProgress)
            {
                logProgress($"Color component: {colorComponentName}; Iteration: {iteration}; Error: {error}.");
            }
        }

        /// <summary>
        /// Логирование результата наложения.
        /// </summary>
        /// <param name="elapsedMs">Время выполнения в миллисекундах.</param>
        private void LogProcessResult(long elapsedMs)
        {
            if (logProgress == null)
            {
                return;
            }

            logProgress($"Blending finished in {elapsedMs}ms.");
            logProgress("");
        }

        #endregion

        #endregion
    }
}
