using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Performance.Logger.API.Integration.Model
{
    public class APIPerformance
    {
        public Guid runId { get; set; }
        public string method { get; set; }
        public string url { get; set; }
        public string correlationId { get; set; }
        public string statusCode { get; set; }
        public long responseTime{ get; set; }
        public string argument { get; set; }
        public string body { get; set; }
        public string response { get; set; }
        public Guid callId { get; set; }
    }
}
