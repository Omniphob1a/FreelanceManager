using Tasks.Domain.Aggregate.Entities;
using Tasks.Domain.Aggregate.Enums;
using Tasks.Domain.Aggregate.Enums.Tasks.Domain.Aggregate.Enums;
using Tasks.Domain.Aggregate.Root;

namespace Tasks.UnitTests;

public class TaskDomainTests
{
	[Fact]
	public void StartTask_MovesTaskToInProgress_OnlyFromTodo()
	{
		var task = ProjectTask.CreateDraft(
			projectId: Guid.NewGuid(),
			title: "Implement registration",
			description: "Fix registration flow",
			reporterId: Guid.NewGuid(),
			priority: TaskPriority.High);

		task.MarkInProgress();

		Assert.Equal(ProjectTaskStatus.InProgress, task.Status);
		Assert.Throws<InvalidOperationException>(() => task.MarkInProgress());
	}

	[Fact]
	public void AddTaskComment_AddsCommentToSelectedTask()
	{
		var task = ProjectTask.CreateDraft(
			projectId: Guid.NewGuid(),
			title: "Review UI",
			description: "Check localized modal",
			reporterId: Guid.NewGuid());
		var comment = Comment.Create(Guid.NewGuid(), "Looks valid", DateTime.UtcNow, task.Id);

		task.AddComment(comment);

		Assert.Contains(task.Comments, item => item.Id == comment.Id && item.TaskId == task.Id);
	}
}
