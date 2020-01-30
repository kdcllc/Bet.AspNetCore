using System.Text;

using Microsoft.ML.Data;

namespace Bet.Extensions.ML.ModelCreation.Results
{
    public class BinaryClassificationMetricsResult : MetricsResult
    {
        private readonly CalibratedBinaryClassificationMetrics _metrics;
        private readonly string _modelName;

        public BinaryClassificationMetricsResult(
            string modelName,
            CalibratedBinaryClassificationMetrics metrics)
        {
            _metrics = metrics;
            _modelName = modelName;
        }

        public double Accuracy => _metrics.Accuracy;

        public double AreaUnderRocCurve => _metrics.AreaUnderRocCurve;

        public double AreaUnderPrecisionRecallCurve => _metrics.AreaUnderPrecisionRecallCurve;

        public double F1Score => _metrics.F1Score;

        public double LogLoss => _metrics.LogLoss;

        public double LogLossReduction => _metrics.LogLossReduction;

        public double PositivePrecision => _metrics.PositivePrecision;

        public double PositiveRecall => _metrics.PositiveRecall;

        public double NegativePrecision => _metrics.NegativePrecision;

        public double NegativeRecall => _metrics.NegativeRecall;

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append("************************************************************");
            sb.Append("*       Metrics for ").Append(_modelName).Append(" binary classification model      ");
            sb.Append("*-----------------------------------------------------------");
            sb.Append("*       Time Elapsed for: ").Append(ElapsedMilliseconds / 1000).Append(" seconds.   ");
            sb.Append("*       Accuracy: ").AppendFormat("{0:P2}", Accuracy);
            sb.Append("*       Area Under Curve:      ").AppendFormat("{0:P2}", AreaUnderRocCurve);
            sb.Append("*       Area under Precision recall Curve:  ").AppendFormat("{0:P2}", AreaUnderPrecisionRecallCurve);
            sb.Append("*       F1Score:  ").AppendFormat("{0:P2}", F1Score);
            sb.Append("*       LogLoss:  ").AppendFormat("{0:#.##}", LogLoss);
            sb.Append("*       LogLossReduction:  ").AppendFormat("{0:#.##}", LogLossReduction);
            sb.Append("*       PositivePrecision:  ").AppendFormat("{0:#.##}", PositivePrecision);
            sb.Append("*       PositiveRecall:  ").AppendFormat("{0:#.##}", PositiveRecall);
            sb.Append("*       NegativePrecision:  ").AppendFormat("{0:#.##}", NegativePrecision);
            sb.Append("*       NegativeRecall:  ").AppendFormat("{0:P2}", NegativeRecall);
            sb.Append("************************************************************");

            return sb.ToString();
        }
    }
}
