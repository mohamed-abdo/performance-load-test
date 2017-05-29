using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Performance.Logger.API.Integration.Model
{
   public class APITestCase
    {
        public Guid callId { get; set; }
        public string api { get; set; }
        public string url { get; set; }
        public string failedTestCase { get; set; }
    }
}
