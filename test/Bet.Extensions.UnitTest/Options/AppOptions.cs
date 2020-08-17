using System.Collections.Generic;

namespace Bet.Extensions.UnitTest.Options
{
    public class AppOptions
    {
        public AppOptions()
        {
            Environments = new Environments();
        }

        public Environments Environments { get; set; }
    }
}
