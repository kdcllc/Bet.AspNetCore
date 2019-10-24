using System;
using System.Text;

using Xunit.Abstractions;

namespace Bet.Extensions.Testing.Logging
{
    public class XunitTestOutputHelper : ITestOutputHelper
    {
        private StringBuilder _output = new StringBuilder();

        public bool Throw { get; set; }

        public string Output => _output.ToString();

        public void WriteLine(string message)
        {
            if (Throw)
            {
                throw new Exception("Bam!");
            }

            _output.AppendLine(message);
        }

        public void WriteLine(string format, params object[] args)
        {
            if (Throw)
            {
                throw new Exception("Bam!");
            }

            _output.AppendLine(string.Format(format, args));
        }
    }
}
