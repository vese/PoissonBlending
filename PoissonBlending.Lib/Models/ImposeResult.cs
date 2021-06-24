using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace PoissonBlending.Lib.Models
{
    public class ImposeResult
    {
        public Bitmap Image { get; set; }

        public long ImposeTimeMilliseconds { get; set; }

        public Dictionary<string, long> ColorComponentSolveTimes { get; set; }

        public (double A, double B) ChangeResult { get; set; }

        public Dictionary<string, double> AverageBorderPixelA { get; set; }
        public Dictionary<string, double> AverageBorderPixelC { get; set; }
        public Dictionary<string, double> AverageInternalPixelB { get; set; }
        public Dictionary<string, double> AverageInternalPixelC { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append($"\nTime: {ImposeTimeMilliseconds}");
            foreach (var colorComponent in ColorComponentSolveTimes.Keys)
            {
                builder.Append($"\nTime for {colorComponent}: {ColorComponentSolveTimes[colorComponent]}");
            }
            builder.Append($"\nChange result: {ChangeResult.A * 100:0.##}% ; {ChangeResult.B * 100:0.##}%");
            builder.Append($"\nAverage border pixel A: {string.Join(", ", AverageBorderPixelA.Select(p => $"{p.Key}: {p.Value:0.##}"))}");
            builder.Append($"\nAverage border pixel C: {string.Join(", ", AverageBorderPixelC.Select(p => $"{p.Key}: {p.Value:0.##}"))}");
            builder.Append($"\nAverage internal pixel B: {string.Join(", ", AverageInternalPixelB.Select(p => $"{p.Key}: {p.Value:0.##}"))}");
            builder.Append($"\nAverage internal pixel C: {string.Join(", ", AverageInternalPixelC.Select(p => $"{p.Key}: {p.Value:0.##}"))}");
            builder.Append($"\n\n");
            return builder.ToString();
        }
    }
}
