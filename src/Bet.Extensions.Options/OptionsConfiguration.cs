using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Bet.Extensions.Options
{
    /// <summary>
    /// Use to configure the options binding from the configurations.
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    public class OptionsConfiguration<TOptions> : IConfigureOptions<TOptions> where TOptions : class
    {
        private readonly IConfiguration _configuration;
        private readonly string _sectionName;
        private readonly Action<IConfiguration, string, TOptions> _configure;

        public OptionsConfiguration(
            IConfiguration configuration,
            string sectionName,
            Action<IConfiguration, string, TOptions> configure)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _configure = configure ?? throw new ArgumentNullException(nameof(configure));

            _sectionName = sectionName;
        }

        public void Configure(TOptions options)
        {
            _configure(_configuration, _sectionName, options);
        }
    }
}
