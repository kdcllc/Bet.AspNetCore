using System.Reflection;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Bet.AnalyticsEngine
{
    public class AnalyticsRequestContext : DbContext
    {
        private AnalyticsEngineOptions _options;

        public AnalyticsRequestContext(IOptionsSnapshot<AnalyticsEngineOptions> options)
        {
            _options = options.Value;
        }

        internal DbSet<WebRequest> WebRequest { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(_options.Database, options =>
            {
                options.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
            });

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WebRequest>().ToTable("WebRequest");
            modelBuilder.Entity<WebRequest>(e => e.HasKey(k => k.Id));

            base.OnModelCreating(modelBuilder);
        }
    }
}
