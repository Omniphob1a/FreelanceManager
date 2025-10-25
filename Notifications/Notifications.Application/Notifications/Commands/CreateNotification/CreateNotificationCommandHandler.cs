using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Notifications.Application.Interfaces;
using Notifications.Domain.Aggregates.Notification.Enums;
using Notifications.Domain.Aggregates.Root;
using Notifications.Domain.Exceptions;
using Notifications.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Application.Interfaces;

namespace Notifications.Application.Notifications.Commands.CreateNotification
{
	public class CreateNotificationCommandHandler : IRequestHandler<CreateNotificationCommand, Result<Guid>>
	{
		private readonly ILogger<CreateNotificationCommandHandler> _logger;
		private readonly INotificationRepository _notificationRepository;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IUserReadRepository _userReadRepository;

		public CreateNotificationCommandHandler(
			ILogger<CreateNotificationCommandHandler> logger,
			INotificationRepository notificationRepository,
			IUnitOfWork unitOfWork,
			IUserReadRepository userReadRepository)
		{
			_logger = logger;
			_notificationRepository = notificationRepository;
			_unitOfWork = unitOfWork;
			_userReadRepository = userReadRepository;
		}

		public async Task<Result<Guid>> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Creating new notification. EventId: {EventId}, UserId: {UserId}",
				request.EventId, request.UserId);

			if (request.EventId == Guid.Empty)
			{
				_logger.LogWarning("Invalid request: EventId is empty");
				return Result.Fail<Guid>("EventId is required");
			}

			if (request.UserId == Guid.Empty)
			{
				_logger.LogWarning("Invalid request: UserId is empty");
				return Result.Fail<Guid>("UserId is required");
			}

			var userExists = await _userReadRepository.ExistsAsync(request.UserId, cancellationToken);
			if (!userExists)
			{
				_logger.LogWarning("User {UserId} not found", request.UserId);
				return Result.Fail<Guid>("User not found.");
			}

			Notification notification;
			try
			{
				notification = Notification.Create(
					request.EventId,
					request.UserId,
					(NotificationChannel)request.Channel,
					request.TemplateKey,
					request.Payload
				);
			}
			catch (DomainException ex)
			{
				_logger.LogWarning(ex, "Domain error while creating notification in from event {EventId}", request.EventId);
				return Result.Fail<Guid>(ex.Message);
			}

			try
			{
				await _notificationRepository.AddAsync(notification, cancellationToken);
				_unitOfWork.TrackEntity(notification);
				await _unitOfWork.SaveChangesAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unexpected error while creating notification in from event {EventId}", request.EventId);
				throw;
			}

			_logger.LogInformation("Notification created successfully with ID: {NotificationId} from event {EventId}", notification.Id, request.EventId);
			return Result.Ok(notification.Id);

		}
	}
}
