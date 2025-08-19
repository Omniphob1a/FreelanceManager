// Application/Jobs/ArchiveOutOfDateProjectsJob.cs
using MediatR;
using Microsoft.Extensions.Logging;
using Projects.Application.Interfaces;
using Projects.Application.Projects.Commands.ArchiveProject;
using Projects.Application.Projects.Commands.EscalateMilestone;
using Projects.Application.Projects.Commands.RescheduleAllMilestones;
using Projects.Application.Projects.Commands.RescheduleMilestone;

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

		public async Task ExecuteAsync()
		{
			try
			{
				_logger.LogInformation("Starting EscalateOutOfDateMilestonesJob...");
				var projects = await _projectQueryService.GetAllAsync();

				if (projects?.Any() != true)
				{
					_logger.LogInformation("No projects found.");
					return;
				}

				foreach (var project in projects)
				{
					await _mediator.Send(new EscalateMilestoneCommand(project.Id));
					await _mediator.Send(new RescheduleAllMilestonesCommand(project.Id, TimeSpan.FromDays(3)));
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