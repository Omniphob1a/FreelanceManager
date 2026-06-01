# Users Service Endpoints

Базовый префикс контроллеров:

- `api/Auth`
- `api/Users`
- `api/Roles`

## AuthController

| Method | Path | Auth | Request body | Response |
|---|---|---|---|---|
| `POST` | `api/Auth/register` | anonymous | `RegisterUserCommand` | `200 OK` with registration result, `400 BadRequest` |
| `POST` | `api/Auth/login` | anonymous | `AuthenticateUserQuery` | `200 OK` with `AuthenticationResult`, `401 Unauthorized` |

## UsersController

| Method | Path | Auth | Request body / query | Response |
|---|---|---|---|---|
| `POST` | `api/Users` | `Admin` + permission `ManageUsers` | `RegisterUserCommand` | `201 Created`, `400 BadRequest` |
| `GET` | `api/Users` | `Admin` | none | `200 OK` with active users |
| `GET` | `api/Users/{id}` | `User` | route: `Guid id` | `200 OK` with `PublicUserDto`, `404 NotFound` |
| `GET` | `api/Users/by-login/{login}` | `Admin` | route: `string login` | `200 OK` with `UserDto`, `404 NotFound` |
| `GET` | `api/Users/by-email/{email}` | `User` | route: `string email` | `200 OK` with `UserDto`, `404 NotFound` |
| `GET` | `api/Users/age/{minAge}` | `Admin` | route: `int minAge` | `200 OK` |
| `PUT` | `api/Users/{userId}` | authenticated user expected | `UpdateUserCommand` | `204 NoContent`, `400 BadRequest` |
| `PUT` | `api/Users/{userId}/password` | authenticated user expected | `ChangeUserPasswordCommand` | `204 NoContent`, `400 BadRequest` |
| `PUT` | `api/Users/{userId}/login` | authenticated user expected | `ChangeUserLoginCommand` | `204 NoContent`, `400 BadRequest` |
| `DELETE` | `api/Users/{userId}?hard={bool}` | `Admin` + permission `DeleteUser` | query: `hard` | `204 NoContent`, `400 BadRequest` |
| `PATCH` | `api/Users/{userId}/restore` | `Admin` | none | `204 NoContent`, `400 BadRequest` |
| `POST` | `api/Users/get-my-profile` | authenticated | none | `200 OK` with current user profile, `401 Unauthorized` |

## RolesController

На весь контроллер действует `Authorize(Roles = "Admin")`.

| Method | Path | Auth | Request body | Response |
|---|---|---|---|---|
| `GET` | `api/Roles` | `Admin` | none | `200 OK` with `IEnumerable<RoleDto>` |
| `GET` | `api/Roles/{id}` | `Admin` | none | `200 OK` with `RoleDto`, `404 NotFound` |
| `POST` | `api/Roles` | `Admin` | `CreateRoleRequest` | `201 Created` with `RoleDto`, `400 BadRequest` |
| `DELETE` | `api/Roles/{id}` | `Admin` | none | `204 NoContent`, `404 NotFound` |
| `POST` | `api/Roles/{id}/permissions/{permissionId}` | `Admin` | none | `200 OK` with `RoleDto`, `404 NotFound`, `400 BadRequest` |
| `DELETE` | `api/Roles/{id}/permissions/{permissionId}` | `Admin` | none | `200 OK` with `RoleDto`, `404 NotFound`, `400 BadRequest` |

## Notes

- `AuthController.Login` дополнительно записывает токен в cookie `secretCookie`.
- В `UsersController` часть методов не помечена явным `[Authorize]`, но использует `User.Identity?.Name`; фактически их следует считать endpoint-ами для авторизованного пользователя.
