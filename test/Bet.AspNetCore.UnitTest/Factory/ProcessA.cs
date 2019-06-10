using System;

namespace Bet.AspNetCore.UnitTest.Factory
{
    public class ProcessA : IProcess
    {
        public ProcessA()
        {
            Console.WriteLine($"Constructor called for: {nameof(ProcessA)}");
        }

        public void Run()
        {
            Console.WriteLine(nameof(ProcessA));
        }
    }
}
