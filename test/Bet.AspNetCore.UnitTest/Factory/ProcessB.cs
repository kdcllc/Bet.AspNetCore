using System;

namespace Bet.AspNetCore.UnitTest.Factory
{
    public class ProcessB : IProcess
    {
        public ProcessB()
        {
            Console.WriteLine($"Constructor called for: {nameof(ProcessB)}");
        }

        public void Run()
        {
            Console.WriteLine(nameof(ProcessB));
        }
    }
}
