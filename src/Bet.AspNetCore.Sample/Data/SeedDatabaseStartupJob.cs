using System;
using System.Threading;
using System.Threading.Tasks;

using CronScheduler.Extensions.StartupInitializer;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Bet.AspNetCore.Sample.Data
{
    public class SeedDatabaseStartupJob : IStartupJob
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<SeedDatabaseStartupJob> _logger;

        public SeedDatabaseStartupJob(
            ApplicationDbContext dbContext,
            UserManager<IdentityUser> userManager,
            ILogger<SeedDatabaseStartupJob> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("{job} started.", nameof(SeedDatabaseStartupJob));

            // await for docker container to come up.
            await Task.Delay(TimeSpan.FromSeconds(1));

            await _dbContext.Database.EnsureCreatedAsync(cancellationToken);

            await Task.Delay(TimeSpan.FromSeconds(5));

            var email = "demo@demo.com";
            var defaultUser = await _userManager.FindByNameAsync(email);

            if (defaultUser == null)
            {
                defaultUser = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                await _userManager.CreateAsync(defaultUser, "Demo2019!");
            }

            _logger.LogInformation("{job} ended.", nameof(SeedDatabaseStartupJob));
        }
    }
}
