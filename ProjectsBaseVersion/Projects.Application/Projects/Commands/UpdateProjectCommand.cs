using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Projects.Commands
{
	public class UpdateProjectCommand : IRequest<Result>
	{
		public Guid Id { get; set; } 
		public string Title { get; set; }
		public string Description { get; set; }
		public Guid OwnerId { get; set; }
		public decimal BudgetMin { get; set; }
		public decimal BudgetMax { get; set; }
		public string Currency { get; set; }
		public string Category { get; set; }
		public List<string> Tags { get; set; }
	}
}
