namespace Tasks.Api.GraphQL.Types
{
	// Minimal UserType (если нужно)
	public class UserType : ObjectType<Tasks.Application.DTOs.PublicUserDto>
	{
		protected override void Configure(IObjectTypeDescriptor<Tasks.Application.DTOs.PublicUserDto> descriptor)
		{
			descriptor.Field(t => t.Id).Type<NonNullType<UuidType>>();
			descriptor.Field(t => t.Name).Type<StringType>();
			descriptor.Field(t => t.Login).Type<StringType>();
		}
	}
}
