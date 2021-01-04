using System;
using System.Collections.Generic;
using System.Drawing;

namespace PoissonBlending.Lib.PixelDescription
{
    public class RgbPixel: BasePixel
    {
        public static new readonly List<string> ColorComponentsNames = new() { nameof(R), nameof(G), nameof(B) };

        public static new readonly int ColorComponentsCount = ColorComponentsNames.Count;

        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }

        public RgbPixel()
        {
            R = G = B = 0;
        }

        public RgbPixel(Color color)
        {
            R = color.R;
            G = color.G;
            B = color.B;
        }

        public override int this[string colorComponentName]
        {
            get => colorComponentName switch
            {
                nameof(R) => R,
                nameof(G) => G,
                nameof(B) => B,
                _ => throw new ArgumentException($"Unknown color component name {colorComponentName}")
            };
            set
            {
                switch (colorComponentName)
                {
                    case nameof(R):
                        R = value;
                        break;
                    case nameof(G):
                        G = value;
                        break;
                    case nameof(B):
                        B = value;
                        break;
                    default:
                        throw new ArgumentException($"Unknown color component name {colorComponentName}");
                }
            }
        }

        public override RgbPixel FromColor(Color color)
        {
            R = color.R;
            G = color.G;
            B = color.B;
            return this;
        }

        public override Color ToColor() => Color.FromArgb(
            R > byte.MaxValue ? byte.MaxValue : R < 0 ? 0 : R,
            G > byte.MaxValue ? byte.MaxValue : G < 0 ? 0 : G,
            B > byte.MaxValue ? byte.MaxValue : B < 0 ? 0 : B);

        public override RgbPixel Multiply(int value)
        {
            R = value * R;
            G = value * G;
            B = value * B;
            return this;
        }

        public override RgbPixel Add(BasePixel value)
        {
            if (value is not RgbPixel)
            {
                throw new ArgumentException($"Wrong argument type: {nameof(BasePixel)}. Expected type {nameof(RgbPixel)}.");
            }

            var pixelValue = value as RgbPixel;
            R += pixelValue.R;
            G += pixelValue.G;
            B += pixelValue.B;
            return this;
        }

        public override RgbPixel Minus(BasePixel value)
        {
            if (value is not RgbPixel)
            {
                throw new ArgumentException($"Wrong argument type: {nameof(BasePixel)}. Expected type {nameof(RgbPixel)}.");
            }

            var pixelValue = value as RgbPixel;
            R -= pixelValue.R;
            G -= pixelValue.G;
            B -= pixelValue.B;
            return this;
        }
    }
}
