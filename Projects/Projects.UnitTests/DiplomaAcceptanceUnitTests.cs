using Projects.Domain.Entities;
using Projects.Domain.ValueObjects;

namespace Projects.UnitTests;

public class DiplomaAcceptanceUnitTests
{
	[Fact]
	public void CreateProject_CreatesDraftProject_WithExpectedData()
	{
		var ownerId = Guid.NewGuid();
		var project = CreateProject(ownerId);

		Assert.NotEqual(Guid.Empty, project.Id);
		Assert.Equal("Diploma project", project.Title);
		Assert.Equal(ownerId, project.OwnerId);
		Assert.Contains(project.Tags, tag => tag.Value == "backend");
	}

	[Fact]
	public void AddProjectMember_AddsMember_AndRejectsDuplicateUser()
	{
		var project = CreateProject(Guid.NewGuid());
		var userId = Guid.NewGuid();
		var member = new ProjectMember(userId, "developer", project.Id);

		project.AddMember(member);

		Assert.Contains(project.Members, item => item.UserId == userId);
		Assert.Throws<InvalidOperationException>(() =>
			project.AddMember(new ProjectMember(userId, "developer", project.Id)));
	}

	private static Project CreateProject(Guid ownerId)
	{
		return Project.CreateDraft(
			title: "Diploma project",
			description: "Project created by domain test",
			ownerId: ownerId,
			budget: new Budget(100, 200, CurrencyCode.From("USD")),
			category: Category.From("development"),
			tags: new[] { Tag.From("backend"), Tag.From("tests") });
	}
}
