using PoissonBlending.Lib.PixelDescription;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PoissonBlending.Lib
{
    public class Mask<Pixel> where Pixel : IPixel, new()
    {
        public bool[,] BorderMask { get; set; }

        public bool[,] FullMask { get; set; }

        public bool[,] BorderlessMask { get; set; }

        public int OffsetX { get; set; }

        public int OffsetY { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public (int x, int y)[] PixelsMap { get; set; }

        // Соседи из A + градиент - известные значения для пикселя
        public PixelArray<Pixel> Pixels { get; set; }

        // Неизветсные соседние пиксели
        public List<int>[] PixelsNeighbors { get; set; }

        public Mask(Point[] selectedAreaPoints, int imageWidth, int imageHeight)
        {
            if (selectedAreaPoints != null && selectedAreaPoints.Length >= 3)
            {
                OffsetX = selectedAreaPoints.Min(p => p.X);
                OffsetY = selectedAreaPoints.Min(p => p.Y);
                Width = selectedAreaPoints.Max(p => p.X) - OffsetX + 1;
                Height = selectedAreaPoints.Max(p => p.Y) - OffsetY + 1;
                BorderMask = new bool[Height, Width];
                FullMask = new bool[Height, Width];
                BorderlessMask = new bool[Height, Width];

                var startBorderMask = new bool[Height, Width];
                var endBorderMask = new bool[Height, Width];

                for (var i = 0; i < selectedAreaPoints.Length; i++)
                {
                    var startPoint = i == 0 ? selectedAreaPoints[^1] : selectedAreaPoints[i - 1];
                    var endPoint = selectedAreaPoints[i];
                    var points = GetBresenhamLine(startPoint, endPoint);
                    var isStartBorder = endPoint.Y < startPoint.Y;
                    points.ForEach(point =>
                    {
                        FullMask[point.Y - OffsetY, point.X - OffsetX] = BorderMask[point.Y - OffsetY, point.X - OffsetX] = true;
                        if (isStartBorder)
                        {
                            startBorderMask[point.Y - OffsetY, point.X - OffsetX] = true;
                        }
                        else
                        {
                            endBorderMask[point.Y - OffsetY, point.X - OffsetX] = true;
                        }
                    });
                }

                var inSelectedArea = false;
                var pointsInSelectedArea = new List<int>();
                for (var i = 0; i < Height; i++)
                {
                    for (var j = 0; j < Width; j++)
                    {
                        if (startBorderMask[i, j] && !endBorderMask[i, j])
                        {
                            inSelectedArea = true;
                            pointsInSelectedArea.Clear();
                        }
                        else if (endBorderMask[i, j] && inSelectedArea && !startBorderMask[i, j])
                        {
                            inSelectedArea = false;
                            pointsInSelectedArea.ForEach(point => BorderlessMask[i, point] = FullMask[i, point] = true);
                        }
                        else if (inSelectedArea)
                        {
                            pointsInSelectedArea.Add(j);
                        }
                    }
                    inSelectedArea = false;
                }
            }
            else
            {
                OffsetX = 0;
                OffsetY = 0;
                Width = imageWidth;
                Height = imageHeight;
                BorderMask = new bool[Height, Width];
                FullMask = new bool[Height, Width];
                BorderlessMask = new bool[Height, Width];
                for (var i = 0; i < Height; i++)
                {
                    for (var j = 0; j < Width; j++)
                    {
                        FullMask[i, j] = true;
                        if (i == 0 || i == Height - 1 || j == 0 || j == Width - 1)
                        {
                            BorderMask[i, j] = true;
                        }
                        else
                        {
                            BorderlessMask[i, j] = true;
                        }
                    }
                }
            }

            var pixelsMap = new List<(int x, int y)>();
            for (var i = 0; i < Height; i++)
            {
                for (var j = 0; j < Width; j++)
                {
                    if (BorderlessMask[i, j])
                    {
                        pixelsMap.Add((j, i));
                    }
                }
            }
            PixelsMap = pixelsMap.ToArray();
            Pixels = new(PixelsMap.Length);
            PixelsNeighbors = new List<int>[PixelsMap.Length];
            for (var i = 0; i < PixelsMap.Length; i++)
            {
                (var x, var y) = PixelsMap[i];
                PixelsNeighbors[i] = GetNeighbors(x, y).Where(p => p.y < BorderlessMask.GetLength(0) && BorderlessMask[p.y, p.x])
                    .Select(p => Array.IndexOf(PixelsMap, p)).ToList();
            }
        }

        public static List<(int x, int y)> GetNeighbors(int x, int y) => new() { (x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1) };

        private static List<Point> GetBresenhamLine(Point p0, Point p1)
        {
            int x0 = p0.X;
            int y0 = p0.Y;
            int x1 = p1.X;
            int y1 = p1.Y;
            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);

            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;

            int err = dx - dy;

            var points = new List<Point>
            {
                new Point(x0, y0)
            };

            while (x0 != x1 || y0 != y1)
            {
                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }

                points.Add(new Point(x0, y0));
            }

            return points;
        }
    }
}
