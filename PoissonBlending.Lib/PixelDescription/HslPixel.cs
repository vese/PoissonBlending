using Skybrud.Colors;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace PoissonBlending.Lib.PixelDescription
{
    public class HslPixel: BasePixel
    {
        public static readonly List<string> ColorComponentsNames = new() { nameof(H), nameof(S), nameof(L) };

        public double H { get; set; }
        public double S { get; set; }
        public double L { get; set; }

        public HslPixel()
        {
            H = S = L = 0;
        }

        public HslPixel(Color color)
        {
            var hslColor = new RgbColor(color.R, color.G, color.B).ToHsl();
            H = hslColor.Hue;
            S = hslColor.Saturation;
            L = hslColor.Lightness;
        }

        public override double this[string colorComponentName]
        {
            get => colorComponentName switch
            {
                nameof(H) => H,
                nameof(S) => S,
                nameof(L) => L,
                _ => throw new ArgumentException($"Unknown color component name {colorComponentName}")
            };
            set
            {
                switch (colorComponentName)
                {
                    case nameof(H):
                        H = value;
                        break;
                    case nameof(S):
                        S = value;
                        break;
                    case nameof(L):
                        L = value;
                        break;
                    default:
                        throw new ArgumentException($"Unknown color component name {colorComponentName}");
                }
            }
        }

        public override HslPixel FromColor(Color color)
        {
            var hslColor = new RgbColor(color.R, color.G, color.B).ToHsl();
            H = hslColor.Hue;
            S = hslColor.Saturation;
            L = hslColor.Lightness;
            return this;
        }

        public override Color ToColor()
        {
            var rgbColor = new HslColor(GetColorComponentValue(H), GetColorComponentValue(S), GetColorComponentValue(L)).ToRgb();
            return Color.FromArgb(rgbColor.R, rgbColor.G, rgbColor.B);
        }

        public override List<string> GetColorComponentsNames() => ColorComponentsNames;

        public override HslPixel Multiply(int value)
        {
            foreach(var colorComponentsName in ColorComponentsNames)
            {
                this[colorComponentsName] *= value;
            }
            return this;
        }

        public override HslPixel Add(BasePixel value)
        {
            var pixelValue = GetHslPixel(value);
            foreach (var colorComponentsName in ColorComponentsNames)
            {
                this[colorComponentsName] += pixelValue[colorComponentsName];
            }
            return this;
        }

        public override HslPixel Minus(BasePixel value)
        {
            var pixelValue = GetHslPixel(value);
            foreach (var colorComponentsName in ColorComponentsNames)
            {
                this[colorComponentsName] -= pixelValue[colorComponentsName];
            }
            return this;
        }

        private static double GetColorComponentValue(double value) => value > 1 ? 1 : value < 0 ? 0 : value;

        private static HslPixel GetHslPixel(BasePixel value)
        {
            if (value is not HslPixel)
            {
                throw new ArgumentException($"Wrong argument type: {nameof(BasePixel)}. Expected type {nameof(HslPixel)}.");
            }

            return value as HslPixel;
        }
    }
}
