using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Performance.Logger.API.Reports
{
    public interface ITraceReports
    {
        dynamic GetDetailsReport(Guid runId);
    }
    public static class TraceReportsFactory
    {
        public static ITraceReports Create()
        {
            return new TracesReport();
        }
    }
}
