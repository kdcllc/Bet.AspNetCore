using System;

namespace Bet.Extensions.UnitTest.DIFactory
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
