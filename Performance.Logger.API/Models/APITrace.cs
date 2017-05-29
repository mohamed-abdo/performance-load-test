using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Performance.Logger.API.Models
{
    public class APITrace
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public Guid RunId { get; set; }
        public string CorrelationId { get; set; }
        public string Method { get; set; }
        public string Url { get; set; }
        public string Status { get; set; }
        public double DurationInMS { get; set; }
        public string Argument { get; set; }
        public string Body { get; set; }
        public string Response { get; set; }
        public Guid CallId { get; set; }
        [Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedOn { get; set; }
        [ForeignKey(nameof(RunId))]
        public virtual PerformanceExecution PerformanceExecution { get; set; }

    }
}
