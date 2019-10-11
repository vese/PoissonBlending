﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Poisson_Blending
{
    public class Pixel
    {
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }

        public Pixel()
        {
            R = G = B = 0;
        }

        public Pixel(Color color)
        {
            R = color.R;
            G = color.G;
            B = color.B;
        }

        public Color ToColor() => Color.FromArgb(R > byte.MaxValue ? byte.MaxValue : R < 0 ? 0 : R, G > byte.MaxValue ? byte.MaxValue : G < 0 ? 0 : G, B > byte.MaxValue ? byte.MaxValue : B < 0 ? 0 : B);

        public static Pixel operator *(int left, Pixel right) => new Pixel { R = left * right.R, G = left * right.G, B = left * right.B };

        public static Pixel operator *(Pixel left, int right) => right * left;

        public static Pixel operator /(Pixel left, int right) => new Pixel { R = left.R / right, G = left.G / right, B = left.B / right };

        public static Pixel operator +(Pixel left, Pixel right) => new Pixel { R = left.R + right.R, G = left.G + right.G, B = left.B + right.B };

        public static Pixel operator -(Pixel left, Pixel right) => new Pixel { R = left.R - right.R, G = left.G - right.G, B = left.B - right.B };

        public static Pixel operator -(Pixel value) => -1 * value;
    }

    class Program
    {
        static Pixel[] Solve(List<int>[] neighboards, Pixel[] b)
        {
            int n = b.Length;
            Pixel[] X = new Pixel[n];
            var RX = Solve(neighboards, b.Select(item => (double)item.R).ToArray());
            var GX = Solve(neighboards, b.Select(item => (double)item.G).ToArray());
            var BX = Solve(neighboards, b.Select(item => (double)item.B).ToArray());
            for (int i = 0; i < n; i++)
            {
                X[i] = new Pixel() { R = (int)RX[i], G = (int)GX[i], B = (int)BX[i] };
            }
            return X;
        }

        static double[] Solve(List<int>[] neighboards, double[] b)
        {
            int n = b.Length;
            double[] x = new double[n], nextX = new double[n];

            bool errorSuits = false;

            while (!errorSuits)
            {
                for (int i = 0; i < n; i++)
                {
                    nextX[i] = b[i];
                    neighboards[i].ForEach(neighboard => nextX[i] += x[neighboard]);
                    nextX[i] /= 4;
                }
                errorSuits = SuitsError(x, nextX, 0.1);
                for (int i = 0; i < n; i++)
                {
                    x[i] = nextX[i];
                }
            }

            return x;
        }

        static bool SuitsError(double[] x, double[] nextX, double epsilon)
        {
            for (int i = 0; i < x.Length; i++)
            {
                if (Math.Pow(nextX[i] - x[i], 2) > epsilon)
                {
                    return false;
                }
            }
            return true;
        }

        static void Main(string[] args)
        {
            using (Bitmap imageA = new Bitmap("A4.jpg"))
            {
                int insertX = 350, insertY = 300;
                using (Bitmap imageB = new Bitmap("B5.jpg"))
                {
                    int insertHeight = imageB.Height, insertWidth = imageB.Width;
                    //int insertHeight = 20, insertWidth = 20;

                    Pixel[,] newImage = new Pixel[insertHeight, insertWidth];
                    for (int j = 0; j < insertWidth; j++)
                    {
                        newImage[0, j] = new Pixel(imageA.GetPixel(insertY + j, insertX));
                        newImage[insertHeight - 1, j] = new Pixel(imageA.GetPixel(insertY + j, insertX + insertHeight - 1));
                    }
                    for (int i = 1; i < insertHeight - 1; i++)
                    {
                        newImage[i, 0] = new Pixel(imageA.GetPixel(insertX, insertX + i));
                        newImage[i, insertWidth - 1] = new Pixel(imageA.GetPixel(insertY + insertWidth - 1, insertX + i));
                    }

                    Pixel[,] guidanceFieldProjection = new Pixel[insertHeight, insertWidth];
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

                    List<int>[] neighbords = new List<int>[(insertHeight - 2) * (insertWidth - 2)];
                    for (int i = 0; i < (insertHeight - 2) * (insertWidth - 2); i++)
                    {
                        neighbords[i] = new List<int>();
                    }
                    Pixel[] b = new Pixel[(insertHeight - 2) * (insertWidth - 2)];
                    for (int i = 0; i < (insertHeight - 2) * (insertWidth - 2); i++)
                    {
                        b[i] = new Pixel();
                    }
                    for (int i = 0; i < insertHeight - 2; i++)
                    {
                        for (int j = 0; j < insertWidth - 2; j++)
                        {
                            int index = i * (insertWidth - 2) + j;
                            if (i > 0)
                            {
                                neighbords[index].Add((i - 1) * (insertWidth - 2) + j);
                            }
                            else
                            {
                                b[index] += new Pixel(imageA.GetPixel(insertY + j + 1, insertX + i));
                            }
                            if (j > 0)
                            {
                                neighbords[index].Add(i * (insertWidth - 2) + j - 1);
                            }
                            else
                            {
                                b[index] += new Pixel(imageA.GetPixel(insertY + j, insertX + i + 1));
                            }
                            if (i + 1 < insertHeight - 2)
                            {
                                neighbords[index].Add((i + 1) * (insertWidth - 2) + j);
                            }
                            else
                            {
                                b[index] += new Pixel(imageA.GetPixel(insertY + j + 1, insertX + i + 2));
                            }
                            if (j + 1 < insertWidth - 2)
                            {
                                neighbords[index].Add(i * (insertWidth - 2) + j + 1);
                            }
                            else
                            {
                                b[index] += new Pixel(imageA.GetPixel(insertY + j + 2, insertX + i + 1));
                            }
                            b[index] += guidanceFieldProjection[i, j + 1] + guidanceFieldProjection[i + 2, j + 1]
                                + guidanceFieldProjection[i + 1, j] + guidanceFieldProjection[i + 1, j + 2];
                        }
                    }
                    var X = Solve(neighbords, b);
                    for (int i = 0; i < insertHeight - 2; i++)
                    {
                        for (int j = 0; j < insertWidth - 2; j++)
                        {
                            newImage[i + 1, j + 1] = X[i * (insertWidth - 2) + j];
                        }
                    }
                    for (int i = 0; i < insertHeight; i++)
                    {
                        for (int j = 0; j < insertWidth; j++)
                        {
                            imageA.SetPixel(insertY + j, insertX + i, newImage[i, j].ToColor());
                        }
                    }
                    imageA.Save("newA.jpg");









                    //for (int i = 0; i < insertHeight; i++)
                    //{
                    //    for (int j = 0; j < insertWidth; j++)
                    //    {
                    //        Color color = imageB.GetPixel(i, j);
                    //        Console.WriteLine($"R{color.R} G{color.G} B{color.B}");
                    //    }
                    //}
                }
            }


            Console.WriteLine("Hello World!");
        }
    }
}
