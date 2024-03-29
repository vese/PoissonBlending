﻿using Skybrud.Colors;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace PoissonBlending.Lib.PixelDescription
{
    public class CmykPixel : BasePixel
    {
        private static readonly List<string> ColorComponentsNames = new() { nameof(C), nameof(M), nameof(Y), nameof(K) };

        public double C { get; set; }
        public double M { get; set; }
        public double Y { get; set; }
        public double K { get; set; }

        public override double Norm => Math.Sqrt(Math.Pow(C, 2) + Math.Pow(M, 2) + Math.Pow(Y, 2) + Math.Pow(K, 2));

        public CmykPixel()
        {
            C = M = Y = K = 0;
        }

        public CmykPixel(Color color)
        {
            var cmyColor = new CmyColor(color.R, color.G, color.B).ToCmyk();
            C = cmyColor.C;
            M = cmyColor.M;
            Y = cmyColor.Y;
            K = cmyColor.K;
        }

        public override double this[string colorComponentName]
        {
            get => colorComponentName switch
            {
                nameof(C) => C,
                nameof(M) => M,
                nameof(Y) => Y,
                nameof(K) => K,
                _ => throw new ArgumentException($"Unknown color component name {colorComponentName}")
            };
            set
            {
                switch (colorComponentName)
                {
                    case nameof(C):
                        C = value;
                        break;
                    case nameof(M):
                        M = value;
                        break;
                    case nameof(Y):
                        Y = value;
                        break;
                    case nameof(K):
                        K = value;
                        break;
                    default:
                        throw new ArgumentException($"Unknown color component name {colorComponentName}");
                }
            }
        }

        public override CmykPixel FromColor(Color color)
        {
            var cmyColor = new RgbColor(color.R, color.G, color.B).ToCmyk();
            C = cmyColor.C;
            M = cmyColor.M;
            Y = cmyColor.Y;
            K = cmyColor.K;
            return this;
        }

        public override Color ToColor()
        {
            var rgbColor = new CmykColor(GetColorComponentValue(C), GetColorComponentValue(M), GetColorComponentValue(Y), GetColorComponentValue(K)).ToRgb();
            return Color.FromArgb(rgbColor.R, rgbColor.G, rgbColor.B);
        }

        public override List<string> GetColorComponentsNames() => ColorComponentsNames;

        public override CmykPixel Multiply(int value)
        {
            foreach (var colorComponentsName in ColorComponentsNames)
            {
                this[colorComponentsName] *= value;
            }
            return this;
        }

        public override CmykPixel Multiply(double value)
        {
            foreach (var colorComponentsName in ColorComponentsNames)
            {
                this[colorComponentsName] *= value;
            }
            return this;
        }

        public override CmykPixel Add(IPixel value)
        {
            var pixelValue = GetPixel<CmykPixel>(value);
            foreach (var colorComponentsName in ColorComponentsNames)
            {
                this[colorComponentsName] += pixelValue[colorComponentsName];
            }
            return this;
        }

        public override CmykPixel Minus(IPixel value)
        {
            var pixelValue = GetPixel<CmykPixel>(value);
            foreach (var colorComponentsName in ColorComponentsNames)
            {
                this[colorComponentsName] -= pixelValue[colorComponentsName];
            }
            return this;
        }

        private static double GetColorComponentValue(double value) => value > 1 ? 1 : value < 0 ? 0 : value;
    }
}
