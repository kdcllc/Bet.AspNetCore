using Bet.Extensions.Options;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bet.Extensions.UnitTest.Options
{
    internal class FakeOptions : IOptionsFormatter
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Format()
        {
            var options = new JObject
            {
                { nameof(Id), Id.ToString() },
                { nameof(Name), Name },
            };

            return options.ToString(Formatting.Indented);
        }
    }
}
