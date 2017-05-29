using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Performance.Logger.API.Integration.Model
{
    public class Tracer
    {
        public Guid RunId { get; set; }
        public int ParallelProcess { get; set; }
        public int Iterations { get; set; }
        public string Specs { get; set; }
        public string Environment { get; set; }
        public string StartingAt { get; set; }
        public string CompletedAt { get; set; }
        public string ExecutingMachine { get; set; }
    }
}
