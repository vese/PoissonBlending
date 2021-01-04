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

        public PixelArray(Dictionary<string, int[]> colorComponentsValues)
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

        public Dictionary<string, int[]> GetColorComponentsValues()
        {
            var colorComponentsValues = new Dictionary<string, int[]>();
            foreach (var colorComponentName in BasePixel.ColorComponentsNames)
            {
                colorComponentsValues.Add(colorComponentName, new int[Length]);
            }

            for (var i = 0; i < Length; i++)
            {
                foreach (var colorComponentName in BasePixel.ColorComponentsNames)
                {
                    colorComponentsValues[colorComponentName][i] = Pixels[i][colorComponentName];
                }
            }

            return colorComponentsValues;
        }
    }
}
