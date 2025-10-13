using Tasks.Api.GraphQL.DataLoaders;
using Tasks.Application.DTOs;

namespace Tasks.Api.GraphQL.Types
{
	public class ProjectTaskType : ObjectType<ProjectTaskDto>
	{
		protected override void Configure(IObjectTypeDescriptor<ProjectTaskDto> descriptor)
		{
			descriptor.Field(t => t.Id).Type<NonNullType<UuidType>>();
			descriptor.Field(t => t.Title).Type<NonNullType<StringType>>();
			descriptor.Field(t => t.Description).Type<StringType>();
			descriptor.Field(t => t.Status).Type<NonNullType<StringType>>();
			descriptor.Field(t => t.Priority).Type<NonNullType<IntType>>();
			descriptor.Field(t => t.TimeEstimatedTicks).Type<IntType>().Name("timeEstimatedMinutes");
			descriptor.Field(t => t.TimeSpentTicks).Type<IntType>().Name("timeSpentMinutes");

			descriptor.Field("assignee")
				.Type<UserType>() 
				.Resolve(async (ctx, ct) =>
				{
					var parent = ctx.Parent<ProjectTaskDto>();
					if (parent.AssigneeId == null) return null;
					var loader = ctx.Service<UserByIdDataLoader>();
					return await loader.LoadAsync(parent.AssigneeId.Value, ct);
				});
		}
	}
}
