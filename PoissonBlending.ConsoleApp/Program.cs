using PoissonBlending.Lib;
using System;

namespace PoissonBlending.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var solver = new PoissonBlendingSolver((string message) => Console.WriteLine(message));
            solver.Blend("A.jpg", "C.jpg", 1200, 200);
        }
    }
}
