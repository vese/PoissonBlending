using System;
using System.Collections.Generic;
using System.Drawing;

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
            double[,] x = new double[n, 3], nextX = new double[n, 3];
            int[,] brgb = new int[n, 3];
            for (int i = 0; i < n; i++)
            {
                brgb[i, 0] = b[i].R;
                brgb[i, 1] = b[i].G;
                brgb[i, 2] = b[i].B;
            }
            bool errorSuits = false;

            while (!errorSuits)
            {
                for (int k = 0; k < 3; k++)
                {
                    for (int i = 0; i < n; i++)
                    {
                        nextX[i, k] = brgb[i, k];
                        neighboards[i].ForEach(neighboard => nextX[i, k] += x[neighboard, k]);
                        nextX[i, k] /= 4;
                    }
                }
                errorSuits = Error(x, nextX) < 1;
                for (int k = 0; k < 3; k++)
                {
                    for (int i = 0; i < n; i++)
                    {
                        x[i, k] = nextX[i, k];
                    }
                }
            }

            Pixel[] result = new Pixel[n];
            for (int i = 0; i < n; i++)
            {
                result[i] = new Pixel { R = (int)x[i, 0], G = (int)x[i, 1], B = (int)x[i, 2] };
            }
            return result;
        }

        static double Error(double[,] x, double[,] nextX)
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

        static void Main(string[] args)
        {
            //jpg png
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
                            b[index] += guidanceFieldProjection[i + 1, j + 1];
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
