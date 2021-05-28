using System.Collections.Generic;
using System.Drawing;

namespace PoissonBlending.Lib.PixelDescription
{
    public interface IPixel
    {
        double Norm { get; }

        double this[string colorComponentName] { get; set; }

        BasePixel FromColor(Color color);

        Color ToColor();

        List<string> GetColorComponentsNames();

        BasePixel Multiply(int value);

        BasePixel Multiply(double value);

        BasePixel Add(IPixel value);

        BasePixel Minus(IPixel value);
    }
}
