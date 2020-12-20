using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace PoissonBlending.Lib
{
    public delegate void LogProgress(string message);

    public class PoissonBlendingSolver
    {
        public const string DefaultResultFilename = "result.jpg";

        private LogProgress logProgress;

        public double AcceptError { get; set; } = 1;

        public PoissonBlendingSolver(LogProgress logProgress = null)
        {
            this.logProgress = logProgress;
        }

        public Bitmap Blend(string baseImageFilename, string imposingImageFilename, int insertX, int insertY,
            bool saveResultImage = true, string resultImageFilename = DefaultResultFilename)
        {
            var watch = Stopwatch.StartNew();

            using var imageA = new Bitmap(baseImageFilename);
            using var imageB = new Bitmap(imposingImageFilename);

            var resultImage = CreateResultBitmap(imageA, imageB, insertX, insertY);

            if (saveResultImage)
            {
                resultImage.Save(resultImageFilename);
            }

            watch.Stop();
            LogProcessResult(watch.ElapsedMilliseconds);

            return resultImage;
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
        private Bitmap CreateResultBitmap(Bitmap imageA, Bitmap imageB, int insertX, int insertY)
        {
            var guidanceFieldProjection = CreateGuidanceFieldProjection(imageB);

            (var pixels, var neighbors) = GetPixelsWithNeighboards(imageA, imageB, insertX, insertY, guidanceFieldProjection);

            var solvedPixels = Solve(pixels, neighbors);

            var resultImagePixels = GetResultImageBorderPixels(imageA, imageB, insertX, insertY);

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
        /// Решение системы уравнений.
        /// </summary>
        /// <param name="pixels">Массив пикселей.</param>
        /// <param name="neighboards">Массив списков идентификаторов соседних пикселей.</param>
        /// <returns>Вычисленный массив пикселей.</returns>
        private Pixel[] Solve(Pixel[] pixels, List<int>[] neighboards)
        {
            int n = pixels.Length;
            double[,] x = new double[n, 3], nextX = new double[n, 3];
            int[,] pixelsRGB = new int[n, 3];
            for (int i = 0; i < n; i++)
            {
                pixelsRGB[i, 0] = pixels[i].R;
                pixelsRGB[i, 1] = pixels[i].G;
                pixelsRGB[i, 2] = pixels[i].B;
            }
            bool errorSuits = false;
            int it = 0;
            while (!errorSuits)
            {
                for (int k = 0; k < 3; k++)
                {
                    for (int i = 0; i < n; i++)
                    {
                        nextX[i, k] = pixelsRGB[i, k];
                        neighboards[i].ForEach(neighboard => nextX[i, k] += x[neighboard, k]);
                        nextX[i, k] /= 4;
                    }
                }
                var error = Error(x, nextX);
                errorSuits = error < AcceptError;
                for (int k = 0; k < 3; k++)
                {
                    for (int i = 0; i < n; i++)
                    {
                        x[i, k] = nextX[i, k];
                    }
                }
                LogSolveProgress(it++, error);
            }

            Pixel[] result = new Pixel[n];
            for (int i = 0; i < n; i++)
            {
                result[i] = new Pixel { R = (int)x[i, 0], G = (int)x[i, 1], B = (int)x[i, 2] };
            }
            return result;
        }

        #region Static

        /// <summary>
        /// Создает двумерный массив пикселей для результирующего изображения и заполняет границы пикселями базового изображения.
        /// </summary>
        /// <param name="imageA">Базовое изображение.</param>
        /// <param name="imageB">Накладываемое изображение.</param>
        /// <param name="insertX">Позиция x наложения.</param>
        /// <param name="insertY">Позиция y наложения.</param>
        /// <returns>Двумерный массив <see cref="Pixel"/> с заполненными граничными пикселями из базового изображения.</returns>
        private static Pixel[,] GetResultImageBorderPixels(Bitmap imageA, Bitmap imageB, int insertX, int insertY)
        {
            int insertHeight = imageB.Height, insertWidth = imageB.Width;
            var resultImagePixels = new Pixel[insertHeight, insertWidth];
            for (int j = 0; j < insertWidth; j++)
            {
                resultImagePixels[0, j] = new Pixel(imageA.GetPixel(insertX + j, insertY));
                resultImagePixels[insertHeight - 1, j] = new Pixel(imageA.GetPixel(insertX + j, insertY + insertHeight - 1));
            }
            for (int i = 1; i < insertHeight - 1; i++)
            {
                resultImagePixels[i, 0] = new Pixel(imageA.GetPixel(insertY, insertY + i));
                resultImagePixels[i, insertWidth - 1] = new Pixel(imageA.GetPixel(insertX + insertWidth - 1, insertY + i));
            }
            return resultImagePixels;
        }

        /// <summary>
        /// Создает двумерный массив проекций поля направлений для накладываемого изображения.
        /// </summary>
        /// <param name="imageB">Накладываемое изображение.</param>
        /// <returns>Двумерный массив <see cref="Pixel"/> проекций поля направлений.</returns>
        private static Pixel[,] CreateGuidanceFieldProjection(Bitmap imageB)
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
                        guidanceFieldProjection[i, j] += new Pixel(imageB.GetPixel(j, i)) - new Pixel(imageB.GetPixel(j, i - 1));
                    }
                    if (j > 0)
                    {
                        guidanceFieldProjection[i, j] += new Pixel(imageB.GetPixel(j, i)) - new Pixel(imageB.GetPixel(j - 1, i));
                    }
                    if (i < insertHeight - 1)
                    {
                        guidanceFieldProjection[i, j] += new Pixel(imageB.GetPixel(j, i)) - new Pixel(imageB.GetPixel(j, i + 1));
                    }
                    if (j < insertWidth - 1)
                    {
                        guidanceFieldProjection[i, j] += new Pixel(imageB.GetPixel(j, i)) - new Pixel(imageB.GetPixel(j + 1, i));
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
        private static (Pixel[] pixels, List<int>[] neighboards) GetPixelsWithNeighboards(Bitmap imageA, Bitmap imageB, int insertX, int insertY, Pixel[,] guidanceFieldProjection)
        {
            int insertHeight = imageB.Height, insertWidth = imageB.Width;
            var neighbors = new List<int>[(insertHeight - 2) * (insertWidth - 2)];
            for (int i = 0; i < (insertHeight - 2) * (insertWidth - 2); i++)
            {
                neighbors[i] = new List<int>();
            }
            var pixels = new Pixel[(insertHeight - 2) * (insertWidth - 2)];
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
                        pixels[index] += new Pixel(imageA.GetPixel(insertX + j + 1, insertY + i));
                    }
                    if (j > 0)
                    {
                        neighbors[index].Add(i * (insertWidth - 2) + j - 1);
                    }
                    else
                    {
                        pixels[index] += new Pixel(imageA.GetPixel(insertX + j, insertY + i + 1));
                    }
                    if (i + 1 < insertHeight - 2)
                    {
                        neighbors[index].Add((i + 1) * (insertWidth - 2) + j);
                    }
                    else
                    {
                        pixels[index] += new Pixel(imageA.GetPixel(insertX + j + 1, insertY + i + 2));
                    }
                    if (j + 1 < insertWidth - 2)
                    {
                        neighbors[index].Add(i * (insertWidth - 2) + j + 1);
                    }
                    else
                    {
                        pixels[index] += new Pixel(imageA.GetPixel(insertX + j + 2, insertY + i + 1));
                    }
                    pixels[index] += guidanceFieldProjection[i + 1, j + 1];
                }
            }

            return (pixels, neighbors);
        }

        /// <summary>
        /// Оценивает погрешность.
        /// </summary>
        /// <param name="x">Предыдущее решение.</param>
        /// <param name="nextX">Новое решение.</param>
        /// <returns>Оценка погрешности.</returns>
        private static double Error(double[,] x, double[,] nextX)
        {
            double error = 0;
            for (int k = 0; k < 3; k++)
            {
                for (int i = 0; i < x.GetLength(0); i++)
                {
                    error += Math.Pow(nextX[i, k] - x[i, k], 2);
                }
            }
            return Math.Sqrt(error);
        }

        #endregion

        #region Log

        /// <summary>
        /// Логирование результатов шага решения.
        /// </summary>
        /// <param name="iteration">Номер шага.</param>
        /// <param name="error">Оценка погрешности.</param>
        private void LogSolveProgress(int iteration, double error)
        {
            if (logProgress == null)
            {
                return;
            }

            logProgress($"Iteration: {iteration}; Error: {error}.");
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

            logProgress($"Blending finished in {elapsedMs} ms.");
        }

        #endregion

        #endregion
    }
}
