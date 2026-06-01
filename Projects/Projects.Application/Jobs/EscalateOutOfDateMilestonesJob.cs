// Application/Jobs/ArchiveOutOfDateProjectsJob.cs
using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;
using Projects.Application.Interfaces;
using Projects.Application.Projects.Commands.EscalateMilestone;

namespace Projects.Application.Jobs
{
	public class EscalateOutOfDateMilestonesJob
	{
		private readonly IProjectQueryService _projectQueryService;
		private readonly IMediator _mediator;
		private readonly ILogger<EscalateOutOfDateMilestonesJob> _logger;

		public EscalateOutOfDateMilestonesJob(
			IProjectQueryService projectQueryService,
			IMediator mediator,
			ILogger<EscalateOutOfDateMilestonesJob> logger)
		{
			_projectQueryService = projectQueryService;
			_mediator = mediator;
			_logger = logger;
		}

		[DisableConcurrentExecution(timeoutInSeconds: 1800)]
		public async Task ExecuteAsync()
		{
			try
			{
				_logger.LogInformation("Starting EscalateOutOfDateMilestonesJob...");
				var projects = await _projectQueryService.GetProjectsWithOutOfDateMilestonesAsync(
					DateTime.UtcNow,
					take: 25,
					CancellationToken.None);

				if (projects?.Any() != true)
				{
					_logger.LogInformation("No overdue milestones found.");
					return;
				}

				foreach (var project in projects)
				{
					await _mediator.Send(new EscalateMilestoneCommand(project.Id));
					_logger.LogInformation($"Milestones escalated in project {project.Id}.");
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to archive projects.");
				throw;
			}
		}
	}
}
