using System.Drawing;

namespace PoissonBlending.Lib
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

        public Color ToColor() => Color.FromArgb(
            R > byte.MaxValue ? byte.MaxValue : R < 0 ? 0 : R,
            G > byte.MaxValue ? byte.MaxValue : G < 0 ? 0 : G,
            B > byte.MaxValue ? byte.MaxValue : B < 0 ? 0 : B);

        public static Pixel operator *(int left, Pixel right) => new Pixel { R = left * right.R, G = left * right.G, B = left * right.B };

        public static Pixel operator *(Pixel left, int right) => right * left;

        public static Pixel operator /(Pixel left, int right) => new Pixel { R = left.R / right, G = left.G / right, B = left.B / right };

        public static Pixel operator +(Pixel left, Pixel right) => new Pixel { R = left.R + right.R, G = left.G + right.G, B = left.B + right.B };

        public static Pixel operator -(Pixel left, Pixel right) => new Pixel { R = left.R - right.R, G = left.G - right.G, B = left.B - right.B };

        public static Pixel operator -(Pixel value) => -1 * value;
    }
}
