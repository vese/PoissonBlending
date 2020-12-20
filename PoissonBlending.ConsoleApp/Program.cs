using PoissonBlending.Lib;
using System;

namespace PoissonBlending.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var solver = new PoissonBlendingSolver((string message) => Console.WriteLine(message));
            solver.Impose("A.jpg", "B.jpg", 300, 70);
            solver.ImposeWithoutBlending("A.jpg", "B.jpg", 300, 70, true, "resultWithoutBlending.jpg");
        }
    }
}
