using System.Collections.Generic;

namespace PoissonBlending.Lib.PixelDescription
{
    public class PixelArray<Pixel> where Pixel : BasePixel, new()
    {
        public Pixel[] Pixels { get; set; }

        public int Length => Pixels.Length;

        public Pixel this[int i]
        {
            get => Pixels[i];
            set => Pixels[i] = value;
        }

        public PixelArray(int length)
        {
            Pixels = new Pixel[length];
        }

        public PixelArray(Dictionary<string, double[]> colorComponentsValues)
        {
            foreach (var colorComponentValues in colorComponentsValues)
            {
                if (Pixels == null)
                {
                    Pixels = new Pixel[colorComponentValues.Value.Length];
                }
                for (int i = 0; i < Length; i++)
                {
                    if (Pixels[i] == null)
                    {
                        Pixels[i] = new Pixel();
                    }
                    Pixels[i][colorComponentValues.Key] = colorComponentValues.Value[i];
                }
            }
        }

        public Dictionary<string, double[]> GetColorComponentsValues()
        {
            var colorComponentsNames = new Pixel().GetColorComponentsNames();
            var colorComponentsValues = new Dictionary<string, double[]>();
            foreach (var colorComponentName in colorComponentsNames)
            {
                colorComponentsValues.Add(colorComponentName, new double[Length]);
            }

            for (var i = 0; i < Length; i++)
            {
                foreach (var colorComponentName in colorComponentsNames)
                {
                    colorComponentsValues[colorComponentName][i] = Pixels[i][colorComponentName];
                }
            }

            return colorComponentsValues;
        }
    }
}
