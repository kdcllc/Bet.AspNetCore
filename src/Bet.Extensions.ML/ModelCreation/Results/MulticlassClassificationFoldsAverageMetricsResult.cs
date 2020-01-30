using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.ML.Data;

using static Microsoft.ML.TrainCatalogBase;

namespace Bet.Extensions.ML.ModelCreation.Results
{
    public class MulticlassClassificationFoldsAverageMetricsResult : MetricsResult
    {
        private readonly IReadOnlyList<CrossValidationResult<MulticlassClassificationMetrics>> _crossValResults;

        public MulticlassClassificationFoldsAverageMetricsResult(
            string algorithmName,
            IReadOnlyList<CrossValidationResult<MulticlassClassificationMetrics>> crossValResults)
        {
            AlgorithmName = algorithmName;
            _crossValResults = crossValResults ?? throw new ArgumentNullException(nameof(crossValResults));

            CalculateMetrics();
        }

        public double MicroAccuracyAverage { get; set; }

        public double MicroAccuraciesStdDeviation { get; set; }

        public double MicroAccuraciesConfidenceInterval95 { get; set; }

        public double MacroAccuracyAverage { get; set; }

        public double MacroAccuraciesStdDeviation { get; set; }

        public double MacroAccuraciesConfidenceInterval95 { get; set; }

        public double LogLossAverage { get; set; }

        public double LogLossStdDeviation { get; set; }

        public double LogLossConfidenceInterval95 { get; set; }

        public double LogLossReductionAverage { get; set; }

        public double LogLossReductionStdDeviation { get; set; }

        public double LogLossReductionConfidenceInterval95 { get; set; }

        public string AlgorithmName { get; }

        public static double CalculateStandardDeviation(IEnumerable<double> values)
        {
            var average = values.Average();
            var sumOfSquaresOfDifferences = values.Select(val => (val - average) * (val - average)).Sum();
            return Math.Sqrt(sumOfSquaresOfDifferences / (values.Count() - 1));
        }

        public static double CalculateConfidenceInterval95(IEnumerable<double> values)
        {
            return 1.96 * CalculateStandardDeviation(values) / Math.Sqrt(values.Count() - 1);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("*************************************************************************************************************");
            sb.Append("*       Metrics for ").Append(AlgorithmName).AppendLine(" Multi-class Classification model                   ");
            sb.Append("*       Time Elapsed for: ").Append(ElapsedMilliseconds / 1000).Append(" seconds.                            ");
            sb.AppendLine("*------------------------------------------------------------------------------------------------------------");
            sb.Append("*       Average MicroAccuracy:    ").AppendFormat("{0:0.###}", MicroAccuracyAverage).Append("  - Standard deviation: (").AppendFormat("{0:#.###}", MicroAccuraciesStdDeviation).Append(")  - Confidence Interval 95%: (").AppendFormat("{0:#.###}", MicroAccuraciesConfidenceInterval95).AppendLine(")");
            sb.Append("*       Average MacroAccuracy:    ").AppendFormat("{0:0.###}", MacroAccuracyAverage).Append("  - Standard deviation: (").AppendFormat("{0:#.###}", MacroAccuraciesStdDeviation).Append(")  - Confidence Interval 95%: (").AppendFormat("{0:#.###}", MacroAccuraciesConfidenceInterval95).AppendLine(")");
            sb.Append("*       Average LogLoss:          ").AppendFormat("{0:#.###}", LogLossAverage).Append("  - Standard deviation: (").AppendFormat("{0:#.###}", LogLossStdDeviation).Append(")  - Confidence Interval 95%: (").AppendFormat("{0:#.###}", LogLossConfidenceInterval95).AppendLine(")");
            sb.Append("*       Average LogLossReduction: ").AppendFormat("{0:#.###}", LogLossReductionAverage).Append("  - Standard deviation: (").AppendFormat("{0:#.###}", LogLossReductionStdDeviation).Append(")  - Confidence Interval 95%: (").AppendFormat("{0:#.###}", LogLossReductionConfidenceInterval95).AppendLine(")");
            sb.AppendLine("*************************************************************************************************************");
            return sb.ToString();
        }

        private void CalculateMetrics()
        {
            var metricsInMultipleFolds = _crossValResults.Select(r => r.Metrics);

            var microAccuracyValues = metricsInMultipleFolds.Select(m => m.MicroAccuracy);
            MicroAccuracyAverage = microAccuracyValues.Average();
            MicroAccuraciesStdDeviation = CalculateStandardDeviation(microAccuracyValues);
            MicroAccuraciesConfidenceInterval95 = CalculateConfidenceInterval95(microAccuracyValues);

            var macroAccuracyValues = metricsInMultipleFolds.Select(m => m.MacroAccuracy);
            MacroAccuracyAverage = macroAccuracyValues.Average();
            MacroAccuraciesStdDeviation = CalculateStandardDeviation(macroAccuracyValues);
            MacroAccuraciesConfidenceInterval95 = CalculateConfidenceInterval95(macroAccuracyValues);

            var logLossValues = metricsInMultipleFolds.Select(m => m.LogLoss);
            LogLossAverage = logLossValues.Average();
            LogLossStdDeviation = CalculateStandardDeviation(logLossValues);
            LogLossConfidenceInterval95 = CalculateConfidenceInterval95(logLossValues);

            var logLossReductionValues = metricsInMultipleFolds.Select(m => m.LogLossReduction);
            LogLossReductionAverage = logLossReductionValues.Average();
            LogLossReductionStdDeviation = CalculateStandardDeviation(logLossReductionValues);
            LogLossReductionConfidenceInterval95 = CalculateConfidenceInterval95(logLossReductionValues);
        }
    }
}
