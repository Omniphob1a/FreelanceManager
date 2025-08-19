using System;

namespace Tasks.Domain.Aggregate.Entities
{
	public class Comment
	{
		public Guid Id { get; private set; }
		public Guid TaskId { get; private set; }
		public Guid AuthorId { get; private set; }
		public string Text { get; private set; }
		public DateTime CreatedAt { get; private set; }

		private Comment(Guid id, Guid authorId, string text, DateTime createdAt, Guid taskId)
		{
			Id = id;
			AuthorId = authorId;
			Text = text;
			CreatedAt = createdAt;
			TaskId = taskId;
		}

		public static Comment Create(Guid authorId, string text, DateTime createdAtUtc, Guid taskId)
		{
			if (authorId == Guid.Empty)
				throw new ArgumentException("Author ID is required.");

			if (string.IsNullOrWhiteSpace(text))
				throw new ArgumentException("Text cannot be empty.");

			return new Comment(Guid.NewGuid(), authorId, text, createdAtUtc, taskId);
		}
	}
}
