using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Performance.Logger.API.Models
{
    public class PerformanceDbContext : DbContext
    {
        public PerformanceDbContext() : base("PerformanceContext")
        {
            Database.SetInitializer(new CreateDatabaseIfNotExists<PerformanceDbContext>());
#if DEBUG
            Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
#endif
            Configuration.ProxyCreationEnabled = false;
            Configuration.LazyLoadingEnabled = false;
        }
        public DbSet<PerformanceExecution> PerformanceExecutions { get; set; }
        public DbSet<TraceDetails> TraceDetails { get; set; }
        public DbSet<APITrace> APITraces { get; set; }
        public DbSet<APIErrorLog> APIErrorLogs { get; set; }
        public DbSet<APITestCase> APITestCases { get; set; }
    }
}
