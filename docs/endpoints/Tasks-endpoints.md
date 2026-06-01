# Tasks Service Endpoints

Базовый префикс контроллера:

- `api/ProjectTasks`

На весь контроллер действует `[Authorize]`.

| Method | Path | Auth | Request body / query | Response |
|---|---|---|---|---|
| `POST` | `api/ProjectTasks` | authenticated | `CreateProjectTaskRequest` | `201 Created` with `Guid`, `400 BadRequest` |
| `GET` | `api/ProjectTasks` | authenticated | query: `TaskFilter`, `PaginationInfo` | `200 OK` with `PaginatedResult<TaskListItemDto>` |
| `GET` | `api/ProjectTasks/{taskId}` | authenticated | query: `includes=...` | `200 OK` with `ProjectTaskDto`, `404 NotFound`, `400 BadRequest` |
| `PUT` | `api/ProjectTasks/{taskId}` | authenticated | `UpdateProjectTaskRequest` | `204 NoContent`, `404 NotFound`, `400 BadRequest` |
| `DELETE` | `api/ProjectTasks/{taskId}` | authenticated | none | `204 NoContent`, `404 NotFound`, `400 BadRequest` |
| `PATCH` | `api/ProjectTasks/{taskId}/assign` | authenticated | `AssignTaskRequest` | `204 NoContent`, `404 NotFound`, `400 BadRequest` |
| `PATCH` | `api/ProjectTasks/{taskId}/unassign` | authenticated | `UnassignTaskRequest` | `204 NoContent`, `404 NotFound`, `400 BadRequest` |
| `PATCH` | `api/ProjectTasks/{taskId}/start` | authenticated | none | `204 NoContent`, `404 NotFound`, `400 BadRequest` |
| `PATCH` | `api/ProjectTasks/{taskId}/complete` | authenticated | none | `204 NoContent`, `404 NotFound`, `400 BadRequest` |
| `PATCH` | `api/ProjectTasks/{taskId}/cancel` | authenticated | `CancelTaskRequest` | `204 NoContent`, `404 NotFound`, `400 BadRequest` |
| `POST` | `api/ProjectTasks/{taskId}/time-entries` | authenticated | `AddTimeEntryRequest` | `200 OK` with created id, `404 NotFound`, `400 BadRequest` |
| `POST` | `api/ProjectTasks/{taskId}/comments` | authenticated | `AddCommentRequest` | `200 OK` with created id, `404 NotFound`, `400 BadRequest` |
| `GET` | `api/ProjectTasks/{taskId}/comments` | authenticated | none | `200 OK` with `List<CommentReadDto>`, `404 NotFound`, `400 BadRequest` |
| `GET` | `api/ProjectTasks/{projectId}/projectMembers` | authenticated | none | `200 OK` with `List<ProjectMemberReadDto>`, `404 NotFound`, `400 BadRequest` |

## Notes

- В `GET api/ProjectTasks/{taskId}` параметр `includes` парсится в `TaskIncludeOptions`.
- В `GET api/ProjectTasks` при `OnlyMyTasks=true` контроллер подставляет `CurrentUserId` из текущего пользователя.
