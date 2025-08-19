using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasks.Persistence.Models
{
	public class TimeEntryEntity
	{
		public Guid Id { get; set; }
		public Guid TaskId { get; set; }
		public Guid UserId { get; set; }
		public DateTime StartedAt { get; set; }
		public DateTime EndedAt { get; set; }
		public string? Description { get; set; }
		public bool IsBillable { get; set; }
		public decimal? HourlyRateAmount { get; set; }
		public string? HourlyRateCurrency { get; set; }
		public decimal Hours { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}
