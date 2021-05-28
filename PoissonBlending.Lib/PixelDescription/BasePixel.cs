using System;
using System.Collections.Generic;
using System.Drawing;

namespace PoissonBlending.Lib.PixelDescription
{
    public abstract class BasePixel: IPixel
    {
        public abstract double Norm { get; }

        public abstract double this[string colorComponentName] { get; set; }

        public abstract BasePixel FromColor(Color color);

        public abstract Color ToColor();

        public abstract List<string> GetColorComponentsNames();

        public abstract BasePixel Multiply(int value);

        public abstract BasePixel Multiply(double value);

        public abstract BasePixel Add(IPixel value);

        public abstract BasePixel Minus(IPixel value);

        protected static Pixel GetPixel<Pixel>(IPixel value) where Pixel: BasePixel
        {
            if (value is not Pixel)
            {
                throw new ArgumentException($"Wrong argument type: {nameof(IPixel)}. Expected type {nameof(Pixel)}.");
            }

            return value as Pixel;
        }
    }
}
