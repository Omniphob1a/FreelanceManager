using FluentResults;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Projects.Application.Common.Abstractions;
using Projects.Application.Interfaces;
using Projects.Domain.Exceptions;
using Projects.Domain.Repositories;

namespace Projects.Application.Projects.Commands.DeleteAttachment;

public class DeleteAttachmentCommandHandler : IRequestHandler<DeleteAttachmentCommand, Result>
{
	private readonly IProjectRepository _projectRepository;
	private readonly IProjectQueryService _queryService;
	private readonly IUnitOfWork _unitOfWork;
	private readonly IFileStorage _fileStorage;
	private readonly ILogger<DeleteAttachmentCommandHandler> _logger;
	private readonly IMapper _mapper;

	public DeleteAttachmentCommandHandler(
		IProjectRepository projectRepository,
		IProjectQueryService queryService,
		IUnitOfWork unitOfWork,
		IFileStorage fileStorage,
		ILogger<DeleteAttachmentCommandHandler> logger,
		IMapper mapper)
	{
		_projectRepository = projectRepository;
		_queryService = queryService;
		_unitOfWork = unitOfWork;
		_fileStorage = fileStorage;
		_logger = logger;
		_mapper = mapper;
	}

	public async Task<Result> Handle(DeleteAttachmentCommand request, CancellationToken ct)
	{
		_logger.LogInformation("Handling DeleteAttachmentCommand for ProjectId {ProjectId}", request.ProjectId);

		var project = await _queryService.GetByIdWithAttachmentsAsync(request.ProjectId, ct);
		if (project is null)
		{
			_logger.LogWarning("Project {ProjectId} not found", request.ProjectId);
			return Result.Fail("Project not found.");
		}

		var attachment = project.Attachments.FirstOrDefault(a => a.Id == request.AttachmentId);
		if (attachment is null)
		{
			_logger.LogWarning("Attachment {AttachmentId} not found", request.AttachmentId);
			return Result.Fail("Attachment not found.");
		}

		try
		{
			await _fileStorage.DeleteAsync(attachment.FileName, ct);
			_logger.LogInformation("Attachment deleted from storage. Url: {Url}", attachment.Url);


			project.DeleteAttachment(attachment);

			await _projectRepository.UpdateAsync(project, ct);
			_unitOfWork.TrackEntity(project);
			await _unitOfWork.SaveChangesAsync(ct);
			_logger.LogInformation("Attachment {AttachmentId} removed from Project {ProjectId}",
				attachment.Id, project.Id);

			return Result.Ok();
		}
		catch (DomainException ex)
		{
			_logger.LogWarning(ex, "Domain exception while removing attachment {AttachmentId}", request.AttachmentId);
			return Result.Fail(ex.Message);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Unexpected error while removing attachment {AttachmentId}", request.AttachmentId);
			return Result.Fail("Unexpected error.");
		}
	}
}
