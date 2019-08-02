# Bet.Extensions.ML.Spam

This project is self contained ML.NET project to be used within other DotNetCore applications.

## Usage

```
    // 1. register
    services.AddSpamDetectionModelBuilder();
    
    services.AddTimedHostedService<ModelBuilderHostedService>(options =>
    {
        options.Interval = TimeSpan.FromMinutes(30);
        options.FailMode = FailMode.LogAndRetry;
        options.RetryInterval = TimeSpan.FromSeconds(30);
    });

    // 2. build model

    public class ModelBuilderHostedService : TimedHostedService
    {
        private readonly IEnumerable<IModelBuilderService> _modelBuilders;

        public ModelBuilderHostedService(
            IEnumerable<IModelBuilderService> modelBuilders,
            IOptionsMonitor<TimedHostedServiceOptions> options,
            IEnumerable<ITimedHostedLifeCycleHook> lifeCycleHooks,
            ILogger<ITimedHostedService> logger) : base(options, lifeCycleHooks, logger)
        {
            TaskToExecuteAsync = (token) => RunModelGenertorsAsync(token);
            _modelBuilders = modelBuilders ?? throw new ArgumentNullException(nameof(modelBuilders));
        }

        public async Task RunModelGenertorsAsync(CancellationToken cancellationToken)
        {
            foreach (var modelBuilder in _modelBuilders)
            {
                try
                {
                    await modelBuilder.TrainModelAsync(cancellationToken);

                    await modelBuilder.ClassifyTestAsync(cancellationToken);

                    await modelBuilder.SaveModelAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    Logger.LogError("{modelBuilder} failed with exception: {message}", modelBuilder.GetType(), ex.Message);
                }
            }
        }
    }
    
    // 3. predict

    [Route("api/[controller]")]
    [ApiController]
    public class PredictionController : ControllerBase
    {
        private readonly IModelPredictionEngine<SpamInput, SpamPrediction> _spamModel;

        public PredictionController(
            IModelPredictionEngine<SpamInput, SpamPrediction> spamModel)
        {
            _spamModel = spamModel ?? throw new ArgumentNullException(nameof(spamModel));
        }

        // GET /api/prediction/spam?text=Hello World
        [HttpGet]
        [Route("spam")]
        public ActionResult<SpamPrediction> PredictSpam([FromQuery]string text)
        {
            return _spamModel.Predict(new SpamInput { Message = text });
        }
    }   

```
