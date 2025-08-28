using System;

namespace Projects.Domain.Entities
{
	public class ProjectMember
	{
		public Guid Id { get; }
		public Guid UserId { get; }
		public string Role { get; }
		public Guid ProjectId { get; }
		public DateTime AddedAt { get; }

		public ProjectMember(Guid userId, string role, Guid projectId)
		{
			Id = Guid.NewGuid();
			UserId = userId == Guid.Empty
				? throw new ArgumentException("UserId is required.", nameof(userId))
				: userId;
			Role = string.IsNullOrWhiteSpace(role)
				? throw new ArgumentNullException(nameof(role))
				: role;
			ProjectId = projectId == Guid.Empty
				? throw new ArgumentException("ProjectId is required.", nameof(projectId))
				: projectId;
			AddedAt = DateTime.UtcNow;
		}

		private ProjectMember(Guid id, Guid userId, string role, Guid projectId, DateTime addedAt)
		{
			Id = id;
			UserId = userId;
			Role = role;
			ProjectId = projectId;
			AddedAt = addedAt;
		}

		public static ProjectMember Load(Guid id, Guid userId, string role, Guid projectId, DateTime addedAt)
		{
			return new ProjectMember(id, userId, role, projectId, addedAt);
		}
	}
}
