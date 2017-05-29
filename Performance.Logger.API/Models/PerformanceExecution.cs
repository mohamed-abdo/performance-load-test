using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Performance.Logger.API.Models
{
    public class PerformanceExecution
    {
        [Key]
        public Guid RunId { get; set; }
        [Required]
        public int ParallelProcess { get; set; }
        [Required]
        public int Iterations { get; set; }
        [Required]
        public string Specs { get; set; }
        public string Environment { get; set; }
        public string ExecutingMachine { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public double DurationInMS { get; set; }
        [Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedOn { get; set; }
        public virtual IEnumerable<TraceDetails> TraceDetails { get; set; }
    }
}
