using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.Options;

namespace Bet.Extensions.Options
{
    /// <summary>
    /// An exception to provide with useful options validation information.
    /// </summary>
    [SuppressMessage("Readability", "RCS1194", Justification = "No need for the default constructors here.")]
    public class OptionsValidationException : Exception
    {
        private readonly IEnumerable<string> _failures;

        private string _message;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsValidationException"/> class.
        /// </summary>
        /// <param name="failure"></param>
        /// <param name="optionsType"></param>
        public OptionsValidationException(string failure, (Type type, string sectionName) optionsType)
        {
            if (string.IsNullOrWhiteSpace(failure))
            {
                throw new ArgumentNullException(nameof(failure));
            }

            OptionsType = optionsType != default ? optionsType : throw new ArgumentNullException(nameof(optionsType));

            _failures = new string[] { failure };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsValidationException"/> class.
        /// </summary>
        /// <param name="failures"></param>
        /// <param name="optionsType"></param>
        public OptionsValidationException(IEnumerable<string> failures, (Type type, string sectionName) optionsType)
        {
            _failures = failures ?? Array.Empty<string>().ToArray();

            OptionsType = optionsType != default ? optionsType : throw new ArgumentNullException(nameof(optionsType));
        }

        /// <inheritdoc/>
        public override string Message => _message ?? (_message = CreateExceptionMessage());

        /// <summary>
        /// Specify type of the <see cref="IOptions{TOptions}"/>.
        /// </summary>
        public (Type type, string sectionName) OptionsType { get; }

        private string CreateExceptionMessage()
        {
            if (_failures?.Count() > 0)
            {
                return $"{_failures.Count()} errors occurred for '{OptionsType.type.Name}' with Configuration '{OptionsType.sectionName}'. The errors are: {string.Join("; ", _failures)}";
            }

            return string.Empty;
        }
    }
}
