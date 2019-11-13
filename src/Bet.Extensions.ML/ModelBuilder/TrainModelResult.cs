using System;
using Microsoft.ML;

namespace Bet.Extensions.ML.ModelBuilder
{
    public class TrainModelResult
    {
        public TrainModelResult(ITransformer model, long elapsedMilliseconds = default)
        {
            Model = model;
            ElapsedMilliseconds = elapsedMilliseconds;
        }

        public ITransformer Model { get; set; }

        public long ElapsedMilliseconds { get; set; }
    }
}
