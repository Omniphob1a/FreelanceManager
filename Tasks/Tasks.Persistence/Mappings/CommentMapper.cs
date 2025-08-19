using Tasks.Domain.Aggregate.Entities;
using Tasks.Persistence.Models;

namespace Tasks.Persistence.Mappings
{
	public class CommentMapper
	{
		public CommentEntity ToEntity(Comment comment, Guid taskId)
		{
			return new CommentEntity
			{
				Id = comment.Id,
				AuthorId = comment.AuthorId,
				Text = comment.Text,
				CreatedAt = comment.CreatedAt,
				TaskId = taskId
			};
		}

		public Comment ToDomain(CommentEntity entity)
		{
			return Comment.Create(entity.AuthorId, entity.Text, entity.CreatedAt, entity.Id);
		}
	}
}
