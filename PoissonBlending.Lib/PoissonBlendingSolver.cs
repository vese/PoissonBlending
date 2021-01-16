using PoissonBlending.Lib.PixelDescription;
using PoissonBlending.Lib.Solver;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
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

        public Bitmap ImposeWithoutBlending(string baseImageFilename, string imposingImageFilename, int insertX, int insertY, Point[] selectedAreaPoints = default,
            bool saveResultImage = true, string resultImageFilename = DefaultResultFilename)
        {
            using var imageA = new Bitmap(baseImageFilename);
            using var imageB = new Bitmap(imposingImageFilename);

            var mask = new Mask<RgbPixel>(selectedAreaPoints, imageB.Width, imageB.Height);

            for (var i = 0; i < mask.Height; i++)
            {
                for (var j = 0; j < mask.Width; j++)
                {
                    if (mask.FullMask[i, j])
                    {
                        var x = j + mask.OffsetX;
                        var y = i + mask.OffsetY;
                        imageA.SetPixel(insertX + x, insertY + y, imageB.GetPixel(x, y));
                    }
                }
            }

            if (saveResultImage)
            {
                imageA.Save(resultImageFilename);
            }

            return new Bitmap(imageA);
        }

        public Bitmap Impose(string baseImageFilename, string imposingImageFilename, int insertX, int insertY, Point[] selectedAreaPoints = default,
            bool saveResultImage = true, string resultImageFilename = DefaultResultFilename) =>
            Impose<RgbPixel>(baseImageFilename, imposingImageFilename, insertX, insertY, selectedAreaPoints, saveResultImage, resultImageFilename);

        public async Task<Bitmap> ImposeAsync(string baseImageFilename, string imposingImageFilename, int insertX, int insertY, Point[] selectedAreaPoints = default,
            bool saveResultImage = true, string resultImageFilename = DefaultResultFilename) =>
            await ImposeAsync<RgbPixel>(baseImageFilename, imposingImageFilename, insertX, insertY, selectedAreaPoints, saveResultImage, resultImageFilename).ConfigureAwait(false);

        public Bitmap Impose<Pixel>(string baseImageFilename, string imposingImageFilename, int insertX, int insertY, Point[] selectedAreaPoints = default,
            bool saveResultImage = true, string resultImageFilename = DefaultResultFilename) where Pixel : IPixel, new()
        {
            LogStarted(typeof(Pixel).Name, false);

            var watch = Stopwatch.StartNew();

            using var imageA = new Bitmap(baseImageFilename);
            using var imageB = new Bitmap(imposingImageFilename);

            var mask = new Mask<Pixel>(selectedAreaPoints, imageB.Width, imageB.Height);

            var resultImage = CreateResultBitmap(imageA, imageB, insertX, insertY, mask);

            if (saveResultImage)
            {
                resultImage.Save(resultImageFilename);
            }

            watch.Stop();
            LogProcessResult(watch.ElapsedMilliseconds);

            return new Bitmap(resultImage);
        }

        public async Task<Bitmap> ImposeAsync<Pixel>(string baseImageFilename, string imposingImageFilename, int insertX, int insertY, Point[] selectedAreaPoints = default,
            bool saveResultImage = true, string resultImageFilename = DefaultResultFilename) where Pixel : IPixel, new()
        {
            LogStarted(typeof(Pixel).Name, true);

            var watch = Stopwatch.StartNew();

            using var imageA = new Bitmap(baseImageFilename);
            using var imageB = new Bitmap(imposingImageFilename);

            var mask = new Mask<Pixel>(selectedAreaPoints, imageB.Width, imageB.Height);

            var resultImage = await CreateResultBitmapAsync(imageA, imageB, insertX, insertY, mask).ConfigureAwait(false);

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
        private Bitmap CreateResultBitmap<Pixel>(Bitmap imageA, Bitmap imageB, int insertX, int insertY, Mask<Pixel> mask) where Pixel : IPixel, new()
        {
            AddGuidanceFieldProjection(imageB, mask);

            AddBorderColors(imageA, insertX, insertY, mask);

            var solvedPixels = solver.Solve(mask);

            for (var i = 0; i < mask.Pixels.Length; i++)
            {
                (var x, var y) = mask.PixelsMap[i];
                imageA.SetPixel(insertX + x + mask.OffsetX, insertY + y + mask.OffsetY, solvedPixels[i].ToColor());
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
        private async Task<Bitmap> CreateResultBitmapAsync<Pixel>(Bitmap imageA, Bitmap imageB, int insertX, int insertY, Mask<Pixel> mask) where Pixel : IPixel, new()
        {
            AddGuidanceFieldProjection(imageB, mask);

            AddBorderColors(imageA, insertX, insertY, mask);

            var solvedPixels = await solver.SolveAsync(mask).ConfigureAwait(false);

            for (var i = 0; i < mask.Pixels.Length; i++)
            {
                (var x, var y) = mask.PixelsMap[i];
                imageA.SetPixel(insertX + x + mask.OffsetX, insertY + y + mask.OffsetY, solvedPixels[i].ToColor());
            }

            return imageA;
        }

        #region Static

        /// <summary>
        /// Создает двумерный массив числовых проекций поля направлений для накладываемого изображения.
        /// </summary>
        /// <param name="imageB">Накладываемое изображение.</param>
        /// <returns>Двумерный массив <see cref="Pixel"/> проекций поля направлений.</returns>
        private static void AddGuidanceFieldProjection<Pixel>(Bitmap imageB, Mask<Pixel> mask) where Pixel : IPixel, new()
        {
            for (var i = 0; i < mask.PixelsMap.Length; i++)
            {
                (var x, var y) = mask.PixelsMap[i];
                var pixel = imageB.GetPixel(x + mask.OffsetX, y + mask.OffsetY);
                Mask<Pixel>.GetNeighbors(x, y).ForEach(p =>
                    mask.Pixels[i].Add(new Pixel().FromColor(pixel).Minus(new Pixel().FromColor(imageB.GetPixel(p.x + mask.OffsetX, p.y + mask.OffsetY)))));
            }
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
