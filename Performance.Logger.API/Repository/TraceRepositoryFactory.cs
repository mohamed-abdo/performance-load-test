using Performance.Logger.API.Integration.Model;
using Performance.Logger.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Performance.Logger.API.Repository
{
    public interface ITraceRepository
    {
        IEnumerable<TraceDetails> ReadData(string traceDirectory);
        Task<bool> CommitMasterData(Guid runId, string CompletedAt);
        Task<bool> SaveMaster(Models.PerformanceExecution masterData);
        Task<bool> SaveAPIPerformance(Models.APITrace APIPerformance);
        Task<bool> SaveAPITestCase(Models.APITestCase APITestCase);
        void ArchiveTraceFiles();
        Task<bool> APIErrorLogger(Guid runId, string error);
    }
    public static class TraceRepositoryFactory
    {
        public static ITraceRepository Create()
        {
            return new TraceRepositorty();
        }
    }
}
