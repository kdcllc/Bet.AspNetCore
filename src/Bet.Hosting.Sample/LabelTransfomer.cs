using Microsoft.ML;
using System.ComponentModel.Composition;

namespace Bet.Hosting.Sample
{
    public class LabelTransfomer
    {
        [Export(nameof(LabelTransfomer))]
        public ITransformer Transformer => ML.Transforms.CustomMappingTransformer<LabelInput, LabelOutput>(Transform,nameof(LabelTransfomer));

        [Import]
        public MLContext ML { get; set; }

        public static void Transform(LabelInput input, LabelOutput output)
        {
            output.Label = string.Equals(input.Label, "spam", System.StringComparison.InvariantCultureIgnoreCase) ? true : false;
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
