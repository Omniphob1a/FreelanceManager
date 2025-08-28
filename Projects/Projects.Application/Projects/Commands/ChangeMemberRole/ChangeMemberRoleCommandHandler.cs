using FluentResults;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Projects.Application.DTOs;
using Projects.Application.Interfaces;
using Projects.Domain.Exceptions;
using Projects.Domain.Repositories;

namespace Projects.Application.Projects.Commands.ChangeMemberRole;

public class ChangeMemberRoleCommandHandler : IRequestHandler<ChangeMemberRoleCommand, Result<ProjectMemberDto>>
{
	private readonly IProjectRepository _repository;
	private readonly IProjectQueryService _queryService;
	private readonly IUnitOfWork _unitOfWork;
	private readonly ILogger<ChangeMemberRoleCommandHandler> _logger;
	private readonly IMapper _mapper;

	public ChangeMemberRoleCommandHandler(
		IProjectRepository repository,
		IProjectQueryService queryService,
		IUnitOfWork unitOfWork,
		ILogger<ChangeMemberRoleCommandHandler> logger,
		IMapper mapper)
	{
		_repository = repository;
		_queryService = queryService;
		_unitOfWork = unitOfWork;
		_logger = logger;
		_mapper = mapper;
	}

	public async Task<Result<ProjectMemberDto>> Handle(ChangeMemberRoleCommand request, CancellationToken ct)
	{
		_logger.LogDebug("Handling ChangeMemberRoleCommand for ProjectId: {ProjectId}, UserId: {UserId}, Role: {Role}",
			request.ProjectId, request.UserId, request.NewRole);

		try
		{
			var project = await _queryService.GetByIdWithMembersAsync(request.ProjectId, ct);
			if (project is null)
			{
				_logger.LogWarning("Project {ProjectId} not found", request.ProjectId);
				return Result.Fail<ProjectMemberDto>("Project not found.");
			}

			project.ChangeMemberRole(request.UserId, request.NewRole);

			await _repository.UpdateAsync(project, ct);
			_unitOfWork.TrackEntity(project);
			await _unitOfWork.SaveChangesAsync(ct);

			var updatedMember = project.Members.First(m => m.UserId == request.UserId);
			var dto = _mapper.Map<ProjectMemberDto>(updatedMember);

			_logger.LogInformation("Role of Member {UserId} in Project {ProjectId} changed to {Role}",
				request.UserId, project.Id, request.NewRole);

			return Result.Ok(dto);
		}
		catch (DomainException ex)
		{
			_logger.LogError(ex, "Domain error while changing role for Member {UserId} in Project {ProjectId}",
				request.UserId, request.ProjectId);
			return Result.Fail<ProjectMemberDto>(ex.Message);
		}
		catch (Exception ex)
		{
			_logger.LogCritical(ex, "Unexpected error while changing role for Member {UserId} in Project {ProjectId}",
				request.UserId, request.ProjectId);
			return Result.Fail<ProjectMemberDto>("Unexpected error occurred.");
		}
	}
}
