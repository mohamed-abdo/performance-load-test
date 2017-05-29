using Performance.Logger.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Performance.Logger.API.Helper;

namespace Performance.Logger.API.Reports
{

    public class TracesReport : ITraceReports
    {
        private PerformanceDbContext dbContext = new PerformanceDbContext();
        public dynamic GetDetailsReport(Guid runId)
        {
            var details = from query in dbContext.TraceDetails
                          where query.RunId == runId
                          group query by new
                          {
                              query.Operation
                          } into operationGroup
                          select new
                          {
                              operationGroup.Key.Operation,
                              duration = (double?)operationGroup.Average(p => p.DurationInMS)
                          };
            var APIDetails = from query in dbContext.APITraces
                             where query.RunId == runId
                             group query by new
                             {
                                 query.Argument,
                             } into operationGroup
                             select new
                             {
                                 operationGroup.Key.Argument,
                                 duration = (double?)operationGroup.Average(p => p.DurationInMS)
                             };
            var ApiTestCases = from query in dbContext.APITraces
                               join tCasesQuery in dbContext.APITestCases
                               on query.CallId equals tCasesQuery.APITraceCallId
                               where query.RunId == runId
                               group tCasesQuery by new
                               {
                                   query.Argument,
                                   tCasesQuery.API
                               } into operationGroup
                               select new
                               {
                                   API = operationGroup.Key.Argument,
                                   CallingCase = operationGroup.Key.API,
                                   failedCount = operationGroup.Count()
                               };

            var APIStatus = from query in dbContext.APITraces
                            where query.RunId == runId
                            group query by new
                            {
                                query.Status,
                            } into operationGroup
                            select new
                            {
                                operationGroup.Key.Status,
                                count = operationGroup.Count()
                            };
            var APIErrorLogCount = dbContext.APIErrorLogs.Count(e => e.RunId == runId);


            var masterData = dbContext.PerformanceExecutions.FirstOrDefault(m => m.RunId == runId);
            return new
            {
                RunId = masterData.RunId,
                StartingAt = masterData.StartedAt?.ToString(Utilities.dateFormat),
                CompletedAt = masterData.CompletedAt?.ToString(Utilities.dateFormat),
                DurationInMS = masterData.DurationInMS,
                Specs = masterData.Specs,
                Environment = masterData.Environment,
                ParallelProcess = masterData.ParallelProcess,
                Iterations = masterData.Iterations,
                APIErrorLogCount = APIErrorLogCount,
                APIStatusSummary = APIStatus,
                APIEndToEndSummary = APIDetails,
                ApiTestCases = ApiTestCases,
                TracingDetails = details
            };
        }
    }
}
