using Performance.Logger.API.Integration.Model;
using Performance.Logger.API.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Performance.Logger.API.Helper;
using System.Collections;

namespace Performance.Logger.API.Repository
{

    public class TraceRepositorty : ITraceRepository
    {
        private static string tracePath = ConfigurationManager.AppSettings["TracePath"];
        private const string archiveFolder = "Archives";
        private const int saveThreeshold = 1000;
        private static readonly IDecorator _docerator = DecoratorFactory.CreateDecorator();

        public Task<bool> APIErrorLogger(Guid runId, string error)
        {
            return _docerator.Decorate<bool>(() =>
            {
                bool ret = false;
                using (var dbContext = new PerformanceDbContext())
                {
                    var master = dbContext.PerformanceExecutions.FirstOrDefault(m => m.RunId == runId);
                    if (string.IsNullOrEmpty(error) || master == null)
                        return false;
                    var errorLog = new APIErrorLog()
                    {
                        RunId = runId,
                        PerformanceExecution = master,
                        Error = error,
                    };
                    dbContext.APIErrorLogs.Add(errorLog);
                    ret = dbContext.SaveChanges() > 0;
                }
                return ret;
            });
        }

        public Task<bool> SaveAPIPerformance(APITrace APIPerformance)
        {
            return _docerator.Decorate<bool>(() =>
            {
                bool ret = false;
                using (var dbContext = new PerformanceDbContext())
                {
                    var master = dbContext.PerformanceExecutions.FirstOrDefault(m => m.RunId == APIPerformance.RunId);
                    if (APIPerformance == null || master == null)
                        return false;
                    APIPerformance.PerformanceExecution = master;
                    dbContext.APITraces.Add(APIPerformance);
                    ret = dbContext.SaveChanges() > 0;
                }
                return ret;
            });
        }

        public Task<bool> SaveMaster(Models.PerformanceExecution masterData)
        {
            return _docerator.Decorate<bool>(() =>
            {
                bool ret = false;
                using (var dbContext = new PerformanceDbContext())
                {
                    if (masterData == null)
                        return false;
                    dbContext.PerformanceExecutions.Add(masterData);
                    ret = dbContext.SaveChanges() > 0;
                }
                return ret;
            });
        }

        public void ArchiveTraceFiles()
        {
            if (!Directory.Exists($"{tracePath}\\{archiveFolder}\\"))
                Directory.CreateDirectory($"{tracePath}\\{archiveFolder}\\");
            foreach (var filePath in Directory.GetFiles(tracePath, "*.log"))
            {
                File.Move(filePath, $"{tracePath}\\{archiveFolder}\\{DateTime.Now.ToFileTime()}.log");
            }
        }

        public Task<bool> CommitMasterData(Guid runId, string CompletedAt)
        {
            return _docerator.Decorate<bool>(() =>
            {
                bool ret = false;
                using (var dbContext = new PerformanceDbContext())
                {
                    var masterData = dbContext.PerformanceExecutions.FirstOrDefault(r => r.RunId == runId);
                    if (masterData == null)
                        return false;
                    masterData.CompletedAt = Helper.Utilities.ParseDateTimestamp(CompletedAt);
                    masterData.DurationInMS = ((masterData.CompletedAt) - masterData.StartedAt)?.TotalMilliseconds ?? 0;
                    var traceData = ReadDataInHashTable(tracePath);
                    int idx = 0;
                    foreach (ICollection<TraceDetails> traceDic in traceData.Values)
                    {
                        foreach (var item in traceDic)
                        {
                            item.RunId = masterData.RunId;
                            item.PerformanceExecution = masterData;
                            dbContext.TraceDetails.Add(item);
                            // save when reach threeshold to keep saving in progress, avoid saving timeout for large number of items.
                            if (++idx >= saveThreeshold)
                            {
                                dbContext.SaveChanges();
                                idx = 0;
                            }
                        }
                    }
                    ret = dbContext.SaveChanges() > 0;
                }
                return ret;
            });
        }
        public Task<bool> LazyCommitMasterData(Guid runId, string CompletedAt)
        {
            return _docerator.Decorate<bool>(() =>
            {
                bool ret = false;
                using (var dbContext = new PerformanceDbContext())
                {
                    var masterData = dbContext.PerformanceExecutions.FirstOrDefault(r => r.RunId == runId);
                    if (masterData == null)
                        return false;
                    masterData.CompletedAt = Helper.Utilities.ParseDateTimestamp(CompletedAt);
                    masterData.DurationInMS = ((masterData.CompletedAt) - masterData.StartedAt)?.TotalMilliseconds ?? 0;
                    var enumartor = TransformData(ReadData(tracePath)).GetEnumerator();
                    int idx = 0;
                    while (enumartor.MoveNext())
                    {
                        var item = enumartor.Current as TraceDetails;
                        item.RunId = masterData.RunId;
                        item.PerformanceExecution = masterData;
                        dbContext.TraceDetails.Add(item);
                        // save when reach threeshold to keep saving in progress, avoid saving timeout for large number of items.
                        if (idx++ > saveThreeshold)
                        {
                            dbContext.SaveChanges();
                            idx = 0;
                        }
                    }
                    ret = dbContext.SaveChanges() > 0;
                }
                return ret;
            });
        }
        public IEnumerable<Models.TraceDetails> ReadData(string traceDirectory)
        {
            foreach (var filePath in Directory.GetFiles(traceDirectory, "*.log", SearchOption.TopDirectoryOnly))
            {
                using (var sr = File.OpenText(filePath))
                {
                    while (sr.Peek() >= 0)
                    {
                        var line = sr.ReadLine();
                        var fields = line.Split('|');
                        yield return new Models.TraceDetails()
                        {
                            Operation = fields[4]?.Trim(),
                            CorrelationId = fields[5]?.Trim(),
                            StartedAt = Helper.Utilities.ParseDateTimestamp(fields[7]?.Trim())
                        };
                    }
                    sr.Close();
                }
            }
        }
        public Hashtable ReadDataInHashTable(string traceDirectory)
        {
            IDictionary tuple = new Dictionary<string, List<TraceDetails>>();
            var hashTable = new Hashtable(tuple);

            foreach (var filePath in Directory.GetFiles(traceDirectory, "*.log", SearchOption.TopDirectoryOnly))
            {
                using (var sr = File.OpenText(filePath))
                {
                    while (sr.Peek() >= 0)
                    {
                        var line = sr.ReadLine();
                        var fields = line.Split('|');
                        var Operation = fields[4]?.Trim();
                        var CorrelationId = fields[5]?.Trim();
                        var StartedAt = Helper.Utilities.ParseDateTimestamp(fields[7]?.Trim());
                        if (hashTable.Contains(CorrelationId))
                        {
                            var CorrelatedList = (List<TraceDetails>)hashTable[CorrelationId];
                            var operationObj = CorrelatedList.FirstOrDefault(o => o.Operation == Operation);
                            if (operationObj == null)
                            {
                                CorrelatedList.Add(
                                    new Models.TraceDetails()
                                    {
                                        Operation = fields[4]?.Trim(),
                                        CorrelationId = fields[5]?.Trim(),
                                        StartedAt = Helper.Utilities.ParseDateTimestamp(fields[7]?.Trim())
                                    });
                            }
                            else
                            {
                                DateTime? firstItem = null;
                                DateTime? secondItem = null;
                                double duration = 0;
                                if (StartedAt > operationObj.StartedAt)
                                {
                                    firstItem = operationObj.StartedAt;
                                    secondItem = StartedAt;
                                    duration = (secondItem - firstItem)?.TotalMilliseconds ?? 0;
                                }
                                else
                                {
                                    firstItem = StartedAt;
                                    secondItem = operationObj.StartedAt;
                                    duration = (secondItem - firstItem)?.TotalMilliseconds ?? 0;
                                }

                                operationObj.StartedAt = firstItem;
                                operationObj.CompletedAt = secondItem;
                                operationObj.DurationInMS = duration;
                            }
                        }
                        else
                        {
                            var traceDetailsList = new List<TraceDetails>{ new Models.TraceDetails()
                                {
                                    Operation = fields[4]?.Trim(),
                                    CorrelationId = fields[5]?.Trim(),
                                    StartedAt = Helper.Utilities.ParseDateTimestamp(fields[7]?.Trim())
                                }
                            };
                            hashTable.Add(CorrelationId, traceDetailsList);
                        }
                    }
                    sr.Close();
                }
            }
            return hashTable;
        }
        private IEnumerable<Models.TraceDetails> TransformData(IEnumerable<Models.TraceDetails> rawData)
        {
            var correlatedGroups = rawData.GroupBy(r => r.CorrelationId);
            foreach (var group in correlatedGroups)
            {
                var operationGroup = group.GroupBy(r => r.Operation);
                foreach (var item in operationGroup)
                {
                    if (item.Count(r => r.StartedAt != null) == 2)
                    {
                        var orderedItems = item.OrderBy(o => o.StartedAt);
                        var firstItem = orderedItems.First();
                        var secondItem = orderedItems.Last();
                        var duration = (secondItem.StartedAt - firstItem.StartedAt)?.TotalMilliseconds ?? 0;
                        yield return new Models.TraceDetails()
                        {
                            Operation = firstItem.Operation,
                            CorrelationId = firstItem.CorrelationId,
                            StartedAt = firstItem.StartedAt,
                            CompletedAt = secondItem.StartedAt,
                            DurationInMS = duration
                        };
                        continue;
                    }
                }
            }
        }

        public Task<bool> SaveAPITestCase(Models.APITestCase APITestCase)
        {
            return _docerator.Decorate<bool>(() =>
            {
                bool ret = false;
                using (var dbContext = new PerformanceDbContext())
                { 
                    dbContext.APITestCases.Add(APITestCase);
                    ret = dbContext.SaveChanges() > 0;
                }
                return ret;
            });
        }
    }

}

