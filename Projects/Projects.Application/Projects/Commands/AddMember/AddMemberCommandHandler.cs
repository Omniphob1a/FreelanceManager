using FluentResults;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Projects.Application.DTOs;
using Projects.Application.Interfaces;
using Projects.Application.Outbox;
using Projects.Domain.Entities;
using Projects.Domain.Exceptions;
using Projects.Domain.Repositories;

namespace Projects.Application.Projects.Commands.AddMember;

public class AddMemberCommandHandler : IRequestHandler<AddMemberCommand, Result<ProjectMemberDto>>
{
	private readonly IProjectRepository _repository;
	private readonly IProjectQueryService _queryService;
	private readonly IUnitOfWork _unitOfWork;
	private readonly ILogger<AddMemberCommandHandler> _logger;
	private readonly IMapper _mapper;
	private readonly IUserReadRepository _userReadRepository;
	private readonly IOutboxService _outboxService;

	public AddMemberCommandHandler(
		IProjectRepository repository,
		IProjectQueryService queryService,
		IUnitOfWork unitOfWork,
		ILogger<AddMemberCommandHandler> logger,
		IMapper mapper,
		IUserReadRepository userReadRepository,
		IOutboxService outboxService)
	{
		_repository = repository;
		_queryService = queryService;
		_unitOfWork = unitOfWork;
		_logger = logger;
		_mapper = mapper;
		_userReadRepository = userReadRepository;
		_outboxService = outboxService;
	}

	public async Task<Result<ProjectMemberDto>> Handle(AddMemberCommand request, CancellationToken ct)
	{
		_logger.LogDebug("Handling AddMemberCommand for ProjectId: {ProjectId}, Login: {Login}",
			request.ProjectId, request.Login);

		try
		{
			var user = await _userReadRepository.GetByLoginAsync(request.Login, ct);

			if (user is null)
			{
				_logger.LogWarning("User not found by login {Login}", request.Login);
				return Result.Fail<ProjectMemberDto>("User not found");
			}

			var project = await _queryService.GetByIdWithMembersAsync(request.ProjectId, ct);
			if (project is null)
			{
				_logger.LogWarning("Project {ProjectId} not found", request.ProjectId);
				return Result.Fail<ProjectMemberDto>("Project not found.");
			}

			if (project.Members.Any(m => m.UserId == user.Id))
			{
				_logger.LogWarning("User {UserId} is already a member of Project {ProjectId}", user.Id, project.Id);
				return Result.Fail<ProjectMemberDto>("User is already a member of the project");
			}

			var member = new ProjectMember(user.Id, request.Role, request.ProjectId);
			project.AddMember(member);

			await _repository.UpdateAsync(project, ct);
			_unitOfWork.TrackEntity(project);

			var dto = _mapper.Map<ProjectMemberDto>(member);
			var topic = "members";
			var key = $"{dto.ProjectId}:{dto.UserId}";
			await _outboxService.Add(dto, topic, key, ct);

			await _unitOfWork.SaveChangesAsync(ct);

			_logger.LogInformation("Member {UserId} with role {Role} added to Project {ProjectId}",
				user.Id, request.Role, project.Id);

			return Result.Ok(dto);
		}
		catch (DomainException ex)
		{
			_logger.LogError(ex, "Domain error while adding member {UserLogin} to Project {ProjectId}",
				request.Login, request.ProjectId);
			return Result.Fail<ProjectMemberDto>(ex.Message);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Unexpected error while adding member {UserLogin} to Project {ProjectId}",
				request.Login, request.ProjectId);
			return Result.Fail<ProjectMemberDto>("Unexpected error occurred.");
		}
	}

}
