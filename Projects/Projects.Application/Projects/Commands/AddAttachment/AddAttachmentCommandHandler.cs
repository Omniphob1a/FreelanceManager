using FluentResults;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Projects.Application.Common.Abstractions;
using Projects.Application.DTOs;
using Projects.Application.Interfaces;
using Projects.Domain.Entities;
using Projects.Domain.Exceptions;
using Projects.Domain.Repositories;

namespace Projects.Application.Projects.Commands.AddAttachment;

public class AddAttachmentCommandHandler : IRequestHandler<AddAttachmentCommand, Result<ProjectAttachmentDto>>
{
	private readonly IProjectQueryService _queryService;
	private readonly IProjectRepository _repository;
	private readonly IUnitOfWork _unitOfWork;
	private readonly IFileStorage _fileStorage;
	private readonly IMapper _mapper;
	private readonly ILogger<AddAttachmentCommandHandler> _logger;

	public AddAttachmentCommandHandler(
		IProjectQueryService queryService,
		IProjectRepository repository,
		IUnitOfWork unitOfWork,
		IFileStorage fileStorage,
		IMapper mapper,
		ILogger<AddAttachmentCommandHandler> logger)
	{
		_queryService = queryService;
		_repository = repository;
		_unitOfWork = unitOfWork;
		_fileStorage = fileStorage;
		_mapper = mapper;
		_logger = logger;
	}

	public async Task<Result<ProjectAttachmentDto>> Handle(AddAttachmentCommand request, CancellationToken ct)
	{
		_logger.LogDebug("Starting AddAttachmentCommand for ProjectId: {ProjectId}", request.ProjectId);

		try
		{
			var project = await _queryService.GetByIdWithAttachmentsAsync(request.ProjectId, ct);
			if (project is null)
			{
				_logger.LogWarning("Project with Id {ProjectId} not found", request.ProjectId);
				return Result.Fail<ProjectAttachmentDto>("Project not found.");
			}

			var file = request.File;
			var extension = Path.GetExtension(file.FileName);
			var uniqueFileName = $"{Guid.NewGuid()}{extension}";
			var objectKey = $"projects/{project.Id}/{uniqueFileName}";

			await using var stream = file.OpenReadStream();
			var fileUrl = await _fileStorage.SaveAsync(stream, objectKey, ct);

			_logger.LogInformation("File uploaded for Project {ProjectId}: {FileName} -> {Url}", project.Id, file.FileName, fileUrl);

			var attachment = new ProjectAttachment(file.FileName.ToString(), fileUrl, project.Id);
			project.AddAttachment(attachment);
			_logger.LogInformation("Attachment properties before saving: {name}, {ulr}", attachment.FileName, attachment.Url);

			await _repository.UpdateAsync(project, ct);
			_unitOfWork.TrackEntity(project);
			await _unitOfWork.SaveChangesAsync(ct);

			_logger.LogInformation("Attachment {AttachmentId} added to Project {ProjectId}", attachment.Id, project.Id);

			var dto = _mapper.Map<ProjectAttachmentDto>(attachment);
			return Result.Ok(dto);
		}
		catch (DomainException ex)
		{
			_logger.LogError(ex, "Domain exception while adding attachment to Project {ProjectId}", request.ProjectId);
			return Result.Fail<ProjectAttachmentDto>(ex.Message);
		}
		catch (Exception ex)
		{
			_logger.LogCritical(ex, "Unhandled exception while adding attachment to Project {ProjectId}", request.ProjectId);
			return Result.Fail<ProjectAttachmentDto>("Unexpected error occurred while adding attachment.");
		}
	}
}
