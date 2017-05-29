using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Performance.Logger.API.Models;
using Performance.Logger.API.Integration.Model;
using System.Data.Entity;
using Performance.Logger.API.Repository;
using Performance.Logger.API.Reports;
using System.Reflection;

namespace Performance.Logger.API.Services
{
    public class TracerService
    {
        private readonly NLog.ILogger _logger;
        private static readonly string currentAssemblyName = Assembly.GetExecutingAssembly().FullName;
        private static ITraceRepository repository = TraceRepositoryFactory.Create();
        private static ITraceReports reports = TraceReportsFactory.Create();
        public TracerService()
        {
            _logger = NLog.LogManager.GetLogger(currentAssemblyName);
        }
        private Models.PerformanceExecution CreateMasterData(Tracer tracer)
        {

            return new Models.PerformanceExecution()
            {
                RunId = tracer.RunId,
                ParallelProcess = tracer.ParallelProcess,
                Iterations = tracer.Iterations,
                Specs = tracer.Specs,
                Environment = tracer.Environment,
                StartedAt = Helper.Utilities.ParseDateTimestamp(tracer.StartingAt),
                CompletedAt = Helper.Utilities.ParseDateTimestamp(tracer.CompletedAt),
                ExecutingMachine = tracer.ExecutingMachine,
            };
        }

        public bool InitializeMaster(Tracer tracer)
        {
            //archiive unprocessed logs.
            try
            {
                repository.ArchiveTraceFiles();
                var masterData = CreateMasterData(tracer);
                return repository.SaveMaster(masterData).Result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return false;
            }

        }
        public dynamic TracingReport(Guid runId, string completedAt)
        {
            try
            {
                var success = repository.CommitMasterData(runId, completedAt).Result;
                if (success)
                    repository.ArchiveTraceFiles();
                return reports.GetDetailsReport(runId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return false;
            }

        }
        public bool APIErrorLogger(Guid runId, string error)
        {
            try
            {
                return repository.APIErrorLogger(runId, error).Result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return false;
            }

        }
        public dynamic GetReport(Guid runId)
        {
            try
            {
                var report = reports.GetDetailsReport(runId);
                if (report == null)
                    throw new Exception("report is null, please find more details on the log.");
                return report;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return null;
            }

        }
        public bool APIPerformace(APITrace APIPerformance)
        {
            try
            {
                return repository.SaveAPIPerformance(APIPerformance).Result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return false;
            }
        }
        public bool APITestCase(Models.APITestCase APITestCase)
        {
            try
            {
                return repository.SaveAPITestCase(APITestCase).Result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return false;
            }
        }
    }
}
