using Users.Domain.Entities;

namespace Users.Domain.ValueObjects;

public class Permission
{
    public string Name { get; set; } = string.Empty;

	public Permission(string name)
	{
		if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name), "Permission cannot be empty");
		Name = name;
	}

	public static readonly Permission ManageUsers = new Permission("ManageUsers");
	public static readonly Permission DeletePosts = new Permission("DeletePosts");

	//Тестовые
	public static readonly Permission CreateUser = new Permission("CreateUser");
	public static readonly Permission DeleteUser = new Permission("DeleteUser");
	public static readonly Permission UpdateUser = new Permission("UpdateUser");
	public static readonly Permission ViewUser = new Permission("ViewUser");

	public override bool Equals(Object obj)
	{
		return obj is Permission other &&
			   StringComparer.Ordinal.Equals(Name, other.Name);
	}

	public override int GetHashCode() => Name.GetHashCode();

}
