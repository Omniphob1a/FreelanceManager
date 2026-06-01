# Projects Service Endpoints

Базовый префикс контроллера:

- `api/Projects`

На весь контроллер действует `[Authorize]`.

| Method | Path | Auth | Request body / query | Response |
|---|---|---|---|---|
| `POST` | `api/Projects` | authenticated | `CreateProjectRequest` | `201 Created` with `Guid`, `400 BadRequest` |
| `GET` | `api/Projects` | authenticated | query: `ProjectFilter` | `200 OK` with `PaginatedResult<ProjectDto>` |
| `GET` | `api/Projects/{projectId}` | authenticated | route: `Guid projectId` | `200 OK` with full project dto, `404 NotFound` |
| `DELETE` | `api/Projects/{projectId}` | authenticated | none | `204 NoContent`, `404 NotFound`, `400 BadRequest` |
| `PUT` | `api/Projects/{projectId}` | authenticated | `UpdateProjectRequest` | `204 NoContent`, `404 NotFound`, `400 BadRequest` |
| `PATCH` | `api/Projects/{projectId}/archive` | authenticated | none | `204 NoContent`, `404 NotFound`, `400 BadRequest` |
| `PATCH` | `api/Projects/{projectId}/publish` | authenticated | `PublishProjectRequest` | `204 NoContent`, `404 NotFound`, `400 BadRequest` |
| `PATCH` | `api/Projects/{projectId}/complete` | authenticated | none | `204 NoContent`, `404 NotFound`, `400 BadRequest` |
| `PATCH` | `api/Projects/{projectId}/add-attachment` | authenticated | `multipart/form-data`, model `AddAttachmentRequest` | `200 OK`, `400 BadRequest` |
| `PATCH` | `api/Projects/{projectId}/delete-attachment` | authenticated | `DeleteAttachmentRequest` | `200 OK`, `404 NotFound`, `400 BadRequest` |
| `PATCH` | `api/Projects/{projectId}/add-milestone` | authenticated | `AddMilestoneRequest` | `200 OK`, `400 BadRequest` |
| `PATCH` | `api/Projects/{projectId}/add-member` | authenticated | `AddMemberRequest` | `200 OK`, `400 BadRequest` |
| `PATCH` | `api/Projects/{projectId}/remove-member` | authenticated | `RemoveMemberRequest` | `200 OK`, `404 NotFound`, `400 BadRequest` |
| `PATCH` | `api/Projects/{projectId}/delete-milestone` | authenticated | body currently typed as `DeleteAttachmentRequest` | `200 OK`, `404 NotFound`, `400 BadRequest` |
| `PATCH` | `api/Projects/{projectId}/complete-milestone` | authenticated | `CompleteMilestoneRequest` | `200 OK`, `404 NotFound`, `400 BadRequest` |
| `PATCH` | `api/Projects/{projectId}/reschedule-milestone` | authenticated | `RescheduleMilestoneRequest` | `200 OK`, `404 NotFound`, `400 BadRequest` |
| `PATCH` | `api/Projects/{projectId}/add-tags` | authenticated | `AddTagsRequest` | `200 OK`, `400 BadRequest` |
| `PATCH` | `api/Projects/{projectId}/delete-tags` | authenticated | `DeleteTagsRequest` | `200 OK`, `404 NotFound`, `400 BadRequest` |
| `GET` | `api/Projects/{projectId}/members` | authenticated | none | `200 OK` with `List<ProjectMemberReadDto>`, `404 NotFound`, `400 BadRequest` |

## Notes

- В `GET api/Projects` контроллер сам подставляет `OwnerId = current user`, если фильтр не передан.
- Для вложений используется `FromForm`, то есть файл надо отправлять как multipart request.
- `delete-milestone` сейчас принимает `DeleteAttachmentRequest`, это особенность текущего кода контроллера.
