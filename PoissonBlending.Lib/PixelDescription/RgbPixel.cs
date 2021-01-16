using System;
using System.Collections.Generic;
using System.Drawing;

namespace PoissonBlending.Lib.PixelDescription
{
    public class RgbPixel: BasePixel
    {
        private static readonly List<string> ColorComponentsNames = new() { nameof(R), nameof(G), nameof(B) };

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

        public override double this[string colorComponentName]
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
                        R = (int)value;
                        break;
                    case nameof(G):
                        G = (int)value;
                        break;
                    case nameof(B):
                        B = (int)value;
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

        public override Color ToColor() => Color.FromArgb(GetColorComponentValue(R), GetColorComponentValue(G), GetColorComponentValue(B));

        public override List<string> GetColorComponentsNames() => ColorComponentsNames;

        public override RgbPixel Multiply(int value)
        {
            foreach (var colorComponentsName in ColorComponentsNames)
            {
                this[colorComponentsName] *= value;
            }
            return this;
        }

        public override RgbPixel Add(IPixel value)
        {
            var pixelValue = GetRgbPixel(value);
            foreach (var colorComponentsName in ColorComponentsNames)
            {
                this[colorComponentsName] += pixelValue[colorComponentsName];
            }
            return this;
        }

        public override RgbPixel Minus(IPixel value)
        {
            var pixelValue = GetRgbPixel(value);
            foreach (var colorComponentsName in ColorComponentsNames)
            {
                this[colorComponentsName] -= pixelValue[colorComponentsName];
            }
            return this;
        }

        private static int GetColorComponentValue(int value) => value > byte.MaxValue ? byte.MaxValue : value < 0 ? 0 : value;

        private static RgbPixel GetRgbPixel(IPixel value)
        {
            if (value is not RgbPixel)
            {
                throw new ArgumentException($"Wrong argument type: {nameof(IPixel)}. Expected type {nameof(RgbPixel)}.");
            }

            return value as RgbPixel;
        }
    }
}
