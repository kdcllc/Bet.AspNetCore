using Microsoft.ML.Transforms;
using System;

namespace Bet.Hosting.Sample
{
    [CustomMappingFactoryAttribute(nameof(LabelTransfomer.Transform))]
    public class LabelTransfomer : CustomMappingFactory<LabelInput, LabelOutput>
    {
        public static void Transform(LabelInput input, LabelOutput output)
        {
            output.Label = string.Equals(input.Label, "spam", System.StringComparison.InvariantCultureIgnoreCase) ? true : false;
        }

        public override Action<LabelInput, LabelOutput> GetMapping()
        {
            return Transform;
        }
    }

    public class LabelInput
    {
        public string Label { get; set; }
    }

    public class LabelOutput
    {
        public bool Label { get; set; }
    }
}
