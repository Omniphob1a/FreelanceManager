// Application/Jobs/ArchiveOutOfDateProjectsJob.cs
using MediatR;
using Microsoft.Extensions.Logging;
using Projects.Application.Interfaces;
using Projects.Application.Projects.Commands.ArchiveProject;

namespace Projects.Application.Jobs
{
	public class ArchiveOutOfDateProjectsJob
	{
		private readonly IProjectQueryService _projectQueryService;
		private readonly IMediator _mediator;
		private readonly ILogger<ArchiveOutOfDateProjectsJob> _logger;

		public ArchiveOutOfDateProjectsJob(
			IProjectQueryService projectQueryService,
			IMediator mediator,
			ILogger<ArchiveOutOfDateProjectsJob> logger)
		{
			_projectQueryService = projectQueryService;
			_mediator = mediator;
			_logger = logger;
		}

		public async Task ExecuteAsync() 
		{
			try
			{
				_logger.LogInformation("Starting ArchiveOutOfDateProjectsJob...");
				var projects = await _projectQueryService.GetOutOfDateProjectsAsync(DateTime.UtcNow);

				if (projects?.Any() != true)
				{
					_logger.LogInformation("No projects to archive.");
					return;
				}

				foreach (var project in projects)
				{
					await _mediator.Send(new ArchiveProjectCommand(project.Id));
					_logger.LogInformation($"Archived project {project.Id}.");
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