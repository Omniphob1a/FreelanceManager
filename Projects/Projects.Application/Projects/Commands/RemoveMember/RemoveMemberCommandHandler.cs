using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Projects.Application.DTOs;
using Projects.Application.Interfaces;
using Projects.Application.Outbox;
using Projects.Domain.Exceptions;
using Projects.Domain.Repositories;

namespace Projects.Application.Projects.Commands.RemoveMember;

public class RemoveMemberCommandHandler : IRequestHandler<RemoveMemberCommand, Result<Unit>>
{
	private readonly IProjectRepository _repository;
	private readonly IProjectQueryService _queryService;
	private readonly IUnitOfWork _unitOfWork;
	private readonly ILogger<RemoveMemberCommandHandler> _logger;
	private readonly IUserService _userService;
	private readonly IOutboxService _outboxService;

	public RemoveMemberCommandHandler(
		IProjectRepository repository,
		IProjectQueryService queryService,
		IUnitOfWork unitOfWork,
		ILogger<RemoveMemberCommandHandler> logger,
		IUserService userService,
		IOutboxService outboxService)
	{
		_repository = repository;
		_queryService = queryService;
		_unitOfWork = unitOfWork;
		_logger = logger;
		_userService = userService;
		_outboxService = outboxService;	
	}

	public async Task<Result<Unit>> Handle(RemoveMemberCommand request, CancellationToken ct)
	{
		_logger.LogDebug("Handling RemoveMemberCommand for ProjectId: {ProjectId}, Email: {UserId}",
			request.ProjectId, request.Email);

		try
		{
			var userDto = await _userService.GetUserByEmail(request.Email, ct);

			if (userDto is null)
			{
				_logger.LogWarning("User not found by email {Email}", request.Email);
				return Result.Fail<Unit>("User not found");
			}

			var project = await _queryService.GetByIdWithMembersAsync(request.ProjectId, ct);
			if (project is null)
			{
				_logger.LogWarning("Project {ProjectId} not found", request.ProjectId);
				return Result.Fail("Project not found.");
			}

			var existingMember = project.Members.FirstOrDefault(m => m.UserId == userDto.Id);
			if (existingMember is null)
			{
				_logger.LogWarning("User {UserId} is not a member of Project {ProjectId}", userDto.Id, project.Id);
				return Result.Fail("User is not a member of the project.");
			}

			project.RemoveMember(existingMember.Id);

			await _repository.UpdateAsync(project, ct);
			_unitOfWork.TrackEntity(project);

			var topic = "members";
			var key = $"{project.Id}:{userDto.Id}"; 
			await _outboxService.AddTombstone(topic, key, ct);

			await _unitOfWork.SaveChangesAsync(ct);

			_logger.LogInformation("Member {UserId} with Email {Email} removed from Project {ProjectId}",
				userDto.Id, request.Email, project.Id);

			return Result.Ok(Unit.Value);
		}
		catch (DomainException ex)
		{
			_logger.LogError(ex, "Domain error while removing member {Email} from Project {ProjectId}",
				request.Email, request.ProjectId);
			return Result.Fail(ex.Message);
		}
		catch (Exception ex)
		{
			_logger.LogCritical(ex, "Unexpected error while removing member {Email} from Project {ProjectId}",
				request.Email, request.ProjectId);
			return Result.Fail("Unexpected error occurred.");
		}
	}
}
