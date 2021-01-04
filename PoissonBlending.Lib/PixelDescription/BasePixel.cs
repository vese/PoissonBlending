using System.Collections.Generic;
using System.Drawing;

namespace PoissonBlending.Lib.PixelDescription
{
    public abstract class BasePixel
    {
        public static readonly List<string> ColorComponentsNames;

        public static readonly int ColorComponentsCount;

        public abstract int this[string colorComponentName] { get; set; }

        public abstract BasePixel FromColor(Color color);

        public abstract Color ToColor();

        public abstract BasePixel Multiply(int value);

        public abstract BasePixel Add(BasePixel value);

        public abstract BasePixel Minus(BasePixel value);
    }
}
