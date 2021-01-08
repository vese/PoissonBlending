﻿using Skybrud.Colors;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace PoissonBlending.Lib.PixelDescription
{
    public class CmyPixel : BasePixel
    {
        private static readonly List<string> ColorComponentsNames = new() { nameof(C), nameof(M), nameof(Y) };

        public double C { get; set; }
        public double M { get; set; }
        public double Y { get; set; }

        public CmyPixel()
        {
            C = M = Y = 0;
        }

        public CmyPixel(Color color)
        {
            var cmyColor = new CmyColor(color.R, color.G, color.B).ToCmy();
            C = cmyColor.C;
            M = cmyColor.M;
            Y = cmyColor.Y;
        }

        public override double this[string colorComponentName]
        {
            get => colorComponentName switch
            {
                nameof(C) => C,
                nameof(M) => M,
                nameof(Y) => Y,
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
                    default:
                        throw new ArgumentException($"Unknown color component name {colorComponentName}");
                }
            }
        }

        public override CmyPixel FromColor(Color color)
        {
            var cmyColor = new RgbColor(color.R, color.G, color.B).ToCmy();
            C = cmyColor.C;
            M = cmyColor.M;
            Y = cmyColor.Y;
            return this;
        }

        public override Color ToColor()
        {
            var rgbColor = new CmyColor(GetColorComponentValue(C), GetColorComponentValue(M), GetColorComponentValue(Y)).ToRgb();
            return Color.FromArgb(rgbColor.R, rgbColor.G, rgbColor.B);
        }

        public override List<string> GetColorComponentsNames() => ColorComponentsNames;

        public override CmyPixel Multiply(int value)
        {
            foreach (var colorComponentsName in ColorComponentsNames)
            {
                this[colorComponentsName] *= value;
            }
            return this;
        }

        public override CmyPixel Add(BasePixel value)
        {
            var pixelValue = GetCmyPixel(value);
            foreach (var colorComponentsName in ColorComponentsNames)
            {
                this[colorComponentsName] += pixelValue[colorComponentsName];
            }
            return this;
        }

        public override CmyPixel Minus(BasePixel value)
        {
            var pixelValue = GetCmyPixel(value);
            foreach (var colorComponentsName in ColorComponentsNames)
            {
                this[colorComponentsName] -= pixelValue[colorComponentsName];
            }
            return this;
        }

        private static double GetColorComponentValue(double value) => value > 1 ? 1 : value < 0 ? 0 : value;

        private static CmyPixel GetCmyPixel(BasePixel value)
        {
            if (value is not CmyPixel)
            {
                throw new ArgumentException($"Wrong argument type: {nameof(BasePixel)}. Expected type {nameof(CmyPixel)}.");
            }

            return value as CmyPixel;
        }
    }
}