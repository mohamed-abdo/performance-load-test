using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Performance.Logger.API.Models
{
    public class TraceDetails
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public long Id { get; set; }
        [Required]
        public string Operation { get; set; }
        [Required]
        public string CorrelationId { get; set; }

        public DateTime? StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public double DurationInMS { get; set; }

        public Guid RunId { get; set; }
        [Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedOn { get; set; }
        [ForeignKey(nameof(RunId))]
        public virtual PerformanceExecution PerformanceExecution { get; set; }
    }
}