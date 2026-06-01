package fml.vp;

import com.vp.plugin.ApplicationManager;
import com.vp.plugin.DiagramManager;
import com.vp.plugin.ModelConvertionManager;
import com.vp.plugin.ProjectManager;
import com.vp.plugin.VPPlugin;
import com.vp.plugin.VPPluginCommandLineSupport;
import com.vp.plugin.VPPluginInfo;
import com.vp.plugin.diagram.IClassDiagramUIModel;
import com.vp.plugin.diagram.IConnectorUIModel;
import com.vp.plugin.diagram.IDiagramElement;
import com.vp.plugin.diagram.IDiagramUIModel;
import com.vp.plugin.diagram.IShapeUIModel;
import com.vp.plugin.diagram.connector.IAssociationUIModel;
import com.vp.plugin.diagram.shape.IClassUIModel;
import com.vp.plugin.model.IAssociation;
import com.vp.plugin.model.IAssociationEnd;
import com.vp.plugin.model.IAttribute;
import com.vp.plugin.model.IClass;
import com.vp.plugin.model.IDependency;
import com.vp.plugin.model.IGeneralization;
import com.vp.plugin.model.IModelElement;
import com.vp.plugin.model.IOperation;
import com.vp.plugin.model.IPackage;
import com.vp.plugin.model.IRealization;
import com.vp.plugin.model.factory.IModelElementFactory;

import java.awt.Color;
import java.awt.Point;
import java.io.File;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

public class ClassDiagramPlugin implements VPPlugin, VPPluginCommandLineSupport {
    private static final IModelElementFactory FACTORY = IModelElementFactory.instance();

    private DiagramManager diagramManager;
    private final Map<String, Box> boxes = new HashMap<>();
    private final List<IDiagramUIModel> createdDiagrams = new ArrayList<>();

    @Override
    public void loaded(VPPluginInfo info) {
    }

    @Override
    public void unloaded() {
    }

    @Override
    public void invoke(String[] args) {
        try {
            String outputPath = args.length > 0
                    ? args[0]
                    : "C:\\Projects\\FreelanceManagerLabs\\FreelanceManager\\docs\\diagrams\\freelance-manager-class-diagrams.vpp";
            String exportPath = args.length > 1
                    ? args[1]
                    : "C:\\Projects\\FreelanceManagerLabs\\FreelanceManager\\docs\\diagrams\\visual-paradigm-export";

            ApplicationManager app = ApplicationManager.instance();
            ProjectManager projectManager = app.getProjectManager();
            diagramManager = app.getDiagramManager();

            projectManager.newProject();
            projectManager.getProject().setName("Freelance Manager - диаграммы классов");

            createUsersDiagram();
            createProjectsDiagram();
            createTasksDiagram();

            File output = new File(outputPath);
            output.getParentFile().mkdirs();
            projectManager.saveProjectAs(output);

            exportDiagrams(app.getModelConvertionManager(), new File(exportPath));
        } catch (Throwable t) {
            t.printStackTrace();
            throw new RuntimeException(t);
        }
    }

    private void exportDiagrams(ModelConvertionManager conversionManager, File exportDir) {
        exportDir.mkdirs();
        for (IDiagramUIModel diagram : createdDiagrams) {
            String baseName = diagram.getName()
                    .replace(" - диаграмма классов", "")
                    .replaceAll("[^A-Za-zА-Яа-я0-9_-]+", "_");
            File png = new File(exportDir, baseName + ".png");
            File svg = new File(exportDir, baseName + ".svg");
            conversionManager.exportDiagramAsImage(diagram, png, ModelConvertionManager.IMAGE_TYPE_PNG);
            conversionManager.exportDiagramAsImage(diagram, svg, ModelConvertionManager.IMAGE_TYPE_SVG);
        }
    }

    private void createUsersDiagram() {
        boxes.clear();
        IClassDiagramUIModel diagram = createDiagram("Users - диаграмма классов");

        Layer api = layer(diagram, "u_api", "API слой", 40, 40, 300, 880, new Color(232, 244, 255));
        Layer app = layer(diagram, "u_app", "Application слой", 420, 40, 520, 880, new Color(255, 248, 225));
        Layer domain = layer(diagram, "u_domain", "Domain слой", 1020, 40, 520, 880, new Color(232, 248, 232));
        Layer persistence = layer(diagram, "u_persistence", "Persistence слой", 1620, 40, 360, 880, new Color(242, 235, 248));
        Layer infra = layer(diagram, "u_infra", "Infrastructure слой", 2060, 40, 330, 880, new Color(252, 235, 235));

        clazz(api, "u_auth", "AuthController", "контроллер", null,
                arr("Register", "Login"), 80, 130, 220, 110);
        clazz(api, "u_users", "UsersController", "контроллер", null,
                arr("GetById", "Update", "Delete", "Restore"), 80, 360, 220, 130);
        clazz(api, "u_roles", "RolesController", "контроллер", null,
                arr("CreateRole", "ListRoles", "AddPermission"), 80, 620, 220, 120);

        clazz(app, "u_mediator", "IMediator", "интерфейс", null,
                arr("Send"), 560, 120, 240, 90);
        clazz(app, "u_register_cmd", "RegisterUserCommand", "команда",
                arr("Login: string", "Password: string", "Name: string", "Email: string"),
                null, 460, 285, 210, 140);
        clazz(app, "u_register_handler", "RegisterUserCommandHandler", "обработчик",
                null, arr("Handle"), 700, 285, 200, 100);
        clazz(app, "u_auth_query", "AuthenticateUserQuery", "запрос",
                arr("Login: string", "Password: string"), null, 460, 500, 210, 110);
        clazz(app, "u_auth_handler", "AuthenticateUserQueryHandler", "обработчик",
                null, arr("Handle"), 700, 500, 200, 100);
        clazz(app, "u_role_cmd", "CreateRoleCommand", "команда",
                arr("Name: string"), null, 460, 700, 210, 100);
        clazz(app, "u_role_handler", "CreateRoleCommandHandler", "обработчик",
                null, arr("Handle"), 700, 700, 200, 100);

        clazz(domain, "u_user", "User", "сущность",
                arr("Id: Guid", "Login: string", "PasswordHash: string", "Name: string", "Birthday: DateTime", "Email: Email"),
                arr("Register", "UpdateProfile", "ChangePassword", "Delete", "Restore", "AddRole"), 1060, 100, 260, 260);
        clazz(domain, "u_role", "Role", "сущность",
                arr("Id: Guid", "Name: string", "PermissionIds: ICollection<Guid>"),
                arr("AddPermission", "RemovePermission"), 1060, 470, 240, 160);
        clazz(domain, "u_permission", "Permission", "value object",
                arr("Name: string"), null, 1060, 730, 240, 95);
        clazz(domain, "u_email", "Email", "value object",
                arr("Value: string"), null, 1370, 120, 130, 90);
        clazz(domain, "u_events", "DomainEvent", "события",
                arr("UserRegisteredDomainEvent", "UserDeletedDomainEvent", "UserRoleAddedDomainEvent", "UserProfileUpdatedDomainEvent"),
                null, 1360, 400, 150, 170);
        clazz(domain, "u_user_repo_i", "IUserRepository", "интерфейс",
                null, arr("Add", "GetById", "GetByLogin", "Update", "Delete"), 1340, 650, 180, 150);
        clazz(domain, "u_role_repo_i", "IRoleRepository", "интерфейс",
                null, arr("Add", "GetById", "ListAll", "Update"), 1340, 815, 180, 125);

        clazz(persistence, "u_db", "UsersDbContext", "EF Core",
                arr("Users", "Roles", "Permissions", "OutboxMessages"), null, 1670, 120, 250, 130);
        clazz(persistence, "u_user_repo", "UserRepository", "репозиторий",
                null, null, 1670, 360, 250, 80);
        clazz(persistence, "u_role_repo", "RoleRepository", "репозиторий",
                null, null, 1670, 560, 250, 80);
        clazz(persistence, "u_outbox_msg", "OutboxMessage", "модель БД",
                arr("EventType: string", "Topic: string", "Payload: string", "Processed: bool"), null, 1670, 740, 250, 130);

        clazz(infra, "u_jwt_i", "IJwtTokenGenerator", "интерфейс",
                null, arr("GenerateToken"), 2100, 120, 220, 90);
        clazz(infra, "u_jwt", "JwtTokenGenerator", "сервис",
                null, null, 2100, 260, 220, 80);
        clazz(infra, "u_outbox_i", "IOutboxService", "интерфейс",
                null, arr("Add", "AddTombstone"), 2100, 420, 220, 100);
        clazz(infra, "u_outbox", "OutboxService", "сервис",
                null, null, 2100, 570, 220, 80);
        clazz(infra, "u_publisher", "OutboxPublisherHostedService", "фоновый сервис",
                null, null, 2100, 720, 220, 80);

        dependency("u_auth", "u_mediator");
        dependency("u_users", "u_mediator");
        dependency("u_roles", "u_mediator");
        dependency("u_register_cmd", "u_register_handler");
        dependency("u_auth_query", "u_auth_handler");
        dependency("u_role_cmd", "u_role_handler");
        dependency("u_register_handler", "u_user_repo_i");
        dependency("u_auth_handler", "u_user_repo_i");
        dependency("u_role_handler", "u_role_repo_i");

        composition("u_user", "u_email", "1", "1");
        association("u_user", "u_role", "0..*", "0..*");
        association("u_role", "u_permission", "0..*", "0..*");
        dependency("u_user", "u_events");

        realization("u_user_repo", "u_user_repo_i");
        realization("u_role_repo", "u_role_repo_i");
        dependency("u_user_repo", "u_db");
        dependency("u_role_repo", "u_db");

        realization("u_jwt", "u_jwt_i");
        realization("u_outbox", "u_outbox_i");
        dependency("u_outbox", "u_outbox_msg");
        dependency("u_publisher", "u_outbox_msg");
    }

    private void createProjectsDiagram() {
        boxes.clear();
        IClassDiagramUIModel diagram = createDiagram("Projects - диаграмма классов");

        Layer api = layer(diagram, "p_api", "API слой", 40, 40, 300, 1020, new Color(232, 244, 255));
        Layer app = layer(diagram, "p_app", "Application слой", 420, 40, 590, 1020, new Color(255, 248, 225));
        Layer domain = layer(diagram, "p_domain", "Domain слой", 1090, 40, 560, 1020, new Color(232, 248, 232));
        Layer persistence = layer(diagram, "p_persistence", "Persistence слой", 1730, 40, 370, 1020, new Color(242, 235, 248));
        Layer infra = layer(diagram, "p_infra", "Infrastructure слой", 2180, 40, 340, 1020, new Color(252, 235, 235));

        clazz(api, "p_ctrl", "ProjectsController", "контроллер", null,
                arr("CreateProject", "GetProjects", "GetProjectById", "UpdateProject", "PublishProject", "ArchiveProject", "AddMember", "AddMilestone", "AddAttachment"),
                75, 130, 230, 210);

        clazz(app, "p_mediator", "IMediator", "интерфейс", null, arr("Send"), 585, 105, 240, 85);
        clazz(app, "p_create_cmd", "CreateProjectCommand", "команда",
                arr("Title: string", "Description: string", "OwnerId: Guid", "Budget: Budget"), null, 465, 250, 230, 140);
        clazz(app, "p_create_handler", "CreateProjectCommandHandler", "обработчик", null, arr("Handle"), 735, 250, 230, 95);
        clazz(app, "p_publish_cmd", "PublishProjectCommand", "команда",
                arr("ProjectId: Guid", "ExpiresAt: DateTime"), null, 465, 450, 230, 115);
        clazz(app, "p_publish_handler", "PublishProjectCommandHandler", "обработчик", null, arr("Handle"), 735, 450, 230, 95);
        clazz(app, "p_member_cmd", "AddMemberCommand", "команда",
                arr("ProjectId: Guid", "UserId: Guid", "Role: string"), null, 465, 620, 230, 115);
        clazz(app, "p_member_handler", "AddMemberCommandHandler", "обработчик", null, arr("Handle"), 735, 620, 230, 95);
        clazz(app, "p_filter_query", "GetProjectsByFilterQuery", "запрос",
                arr("Status: ProjectStatus?", "Tag: string?", "Page: int"), null, 465, 790, 230, 120);
        clazz(app, "p_query_handler", "GetProjectsByFilterQueryHandler", "обработчик запроса", null, arr("Handle"), 735, 790, 230, 95);
        clazz(app, "p_file_i", "IFileStorage", "интерфейс", null, arr("SaveAsync", "DeleteAsync"), 2200, 265, 190, 105);
        clazz(app, "p_cache_i", "ICacheService", "интерфейс", null, arr("GetAsync", "SetAsync", "RemoveAsync"), 2200, 100, 190, 120);
        clazz(app, "p_job_i", "IBackgroundJobManager", "интерфейс", null, arr("Schedule", "AddOrUpdateRecurring"), 2200, 430, 190, 105);

        clazz(domain, "p_project", "Project", "агрегат",
                arr("Id: Guid", "Title: string", "Description: string", "OwnerId: Guid", "Status: ProjectStatus", "ExpiresAt: DateTime?"),
                arr("CreateDraft", "Publish", "Archive", "Complete", "AddMember", "AddMilestone", "AddAttachment"),
                1130, 100, 270, 260);
        clazz(domain, "p_member", "ProjectMember", "сущность",
                arr("Id: Guid", "UserId: Guid", "Role: string", "ProjectId: Guid", "AddedAt: DateTime"), null, 1450, 100, 160, 145);
        clazz(domain, "p_milestone", "ProjectMilestone", "сущность",
                arr("Id: Guid", "Title: string", "DueDate: DateTime", "IsCompleted: bool", "IsEscalated: bool"),
                arr("MarkCompleted", "MarkEscalated", "Reschedule"), 1450, 320, 160, 190);
        clazz(domain, "p_attachment", "ProjectAttachment", "сущность",
                arr("Id: Guid", "FileName: string", "Url: string", "ProjectId: Guid"), null, 1450, 590, 160, 125);
        clazz(domain, "p_budget", "Budget", "value object", arr("Min: decimal", "Max: decimal", "Currency: string"), null, 1130, 470, 150, 115);
        clazz(domain, "p_tag", "Tag", "value object", arr("Name: string"), null, 1300, 470, 120, 85);
        clazz(domain, "p_status", "ProjectStatus", "enum", arr("Draft", "Active", "Completed", "Archived"), null, 1130, 650, 160, 120);
        clazz(domain, "p_events", "DomainEvent", "события",
                arr("ProjectCreatedDomainEvent", "ProjectPublishedDomainEvent", "ProjectMemberAddedDomainEvent", "MilestoneAddedDomainEvent", "AttachmentAddedDomainEvent"),
                null, 1300, 760, 190, 170);
        clazz(domain, "p_repo_i", "IProjectRepository", "интерфейс", null,
                arr("AddAsync", "UpdateAsync", "DeleteAsync", "ExistsAsync"), 1500, 805, 120, 130);

        clazz(persistence, "p_db", "ProjectsDbContext", "EF Core",
                arr("Projects", "UserReadModels", "IncomingEvents", "OutboxMessages"), null, 1785, 110, 250, 130);
        clazz(persistence, "p_repo", "ProjectRepository", "репозиторий", null, null, 1785, 320, 250, 80);
        clazz(persistence, "p_qs", "ProjectQueryService", "запросы", null, null, 1785, 480, 250, 80);
        clazz(persistence, "p_user_read", "UserReadRepository", "read repository", null, null, 1785, 640, 250, 80);
        clazz(persistence, "p_user_model", "UserReadModel", "read model", arr("Id: Guid", "Login: string", "Name: string"), null, 1785, 780, 250, 110);
        clazz(persistence, "p_outbox_msg", "OutboxMessage", "модель БД", arr("Topic: string", "Payload: string", "Processed: bool"), null, 1785, 930, 250, 110);

        clazz(infra, "p_cache", "RedisCacheService", "Redis", null, null, 2470, 120, 220, 80);
        clazz(infra, "p_s3", "S3FileStorage", "S3", null, null, 2470, 285, 220, 80);
        clazz(infra, "p_job", "BackgroundJobManager", "Hangfire", null, null, 2470, 445, 220, 80);
        clazz(infra, "p_outbox", "OutboxService", "сервис", null, null, 2240, 620, 220, 80);
        clazz(infra, "p_publisher", "OutboxPublisherHostedService", "фоновый сервис", null, null, 2240, 770, 220, 80);
        clazz(infra, "p_kafka", "ConfluentKafkaProducer", "Kafka", null, null, 2470, 770, 220, 80);

        dependency("p_ctrl", "p_mediator");
        dependency("p_create_cmd", "p_create_handler");
        dependency("p_publish_cmd", "p_publish_handler");
        dependency("p_member_cmd", "p_member_handler");
        dependency("p_filter_query", "p_query_handler");
        dependency("p_create_handler", "p_repo_i");
        dependency("p_publish_handler", "p_repo_i");
        dependency("p_member_handler", "p_repo_i");
        dependency("p_query_handler", "p_qs");

        composition("p_project", "p_member", "1", "0..*");
        composition("p_project", "p_milestone", "1", "0..*");
        composition("p_project", "p_attachment", "1", "0..*");
        composition("p_project", "p_budget", "1", "1");
        association("p_project", "p_tag", "1", "0..*");
        association("p_project", "p_status", "1", "1");
        dependency("p_project", "p_events");

        realization("p_repo", "p_repo_i");
        dependency("p_repo", "p_db");
        dependency("p_qs", "p_db");
        dependency("p_user_read", "p_db");
        realization("p_cache", "p_cache_i");
        realization("p_s3", "p_file_i");
        realization("p_job", "p_job_i");
        dependency("p_outbox", "p_outbox_msg");
        dependency("p_publisher", "p_outbox_msg");
        dependency("p_publisher", "p_kafka");
    }

    private void createTasksDiagram() {
        boxes.clear();
        IClassDiagramUIModel diagram = createDiagram("Tasks - диаграмма классов");

        Layer api = layer(diagram, "t_api", "API слой", 40, 40, 300, 1040, new Color(232, 244, 255));
        Layer app = layer(diagram, "t_app", "Application слой", 420, 40, 590, 1040, new Color(255, 248, 225));
        Layer domain = layer(diagram, "t_domain", "Domain слой", 1090, 40, 560, 1040, new Color(232, 248, 232));
        Layer persistence = layer(diagram, "t_persistence", "Persistence слой", 1730, 40, 370, 1040, new Color(242, 235, 248));
        Layer infra = layer(diagram, "t_infra", "Infrastructure слой", 2180, 40, 340, 1040, new Color(252, 235, 235));

        clazz(api, "t_ctrl", "ProjectTasksController", "контроллер", null,
                arr("CreateTask", "GetTasks", "GetTaskById", "UpdateTask", "AssignTask", "StartTask", "CompleteTask", "AddComment", "AddTimeEntry"),
                75, 130, 230, 210);

        clazz(app, "t_mediator", "IMediator", "интерфейс", null, arr("Send"), 585, 105, 240, 85);
        clazz(app, "t_create_cmd", "CreateProjectTaskCommand", "команда",
                arr("ProjectId: Guid", "Title: string", "ReporterId: Guid"), null, 465, 225, 230, 110);
        clazz(app, "t_create_handler", "CreateProjectTaskCommandHandler", "обработчик", null, arr("Handle"), 735, 225, 230, 90);
        clazz(app, "t_assign_cmd", "AssignProjectTaskCommand", "команда",
                arr("TaskId: Guid", "AssigneeId: Guid"), null, 465, 370, 230, 100);
        clazz(app, "t_assign_handler", "AssignProjectTaskCommandHandler", "обработчик", null, arr("Handle"), 735, 370, 230, 90);
        clazz(app, "t_comment_cmd", "AddCommentCommand", "команда",
                arr("TaskId: Guid", "AuthorId: Guid", "Text: string"), null, 465, 515, 230, 105);
        clazz(app, "t_comment_handler", "AddCommentCommandHandler", "обработчик", null, arr("Handle"), 735, 515, 230, 90);
        clazz(app, "t_time_cmd", "LogTimeCommand", "команда",
                arr("TaskId: Guid", "UserId: Guid", "Period: TimeRange"), null, 465, 660, 230, 105);
        clazz(app, "t_time_handler", "LogTimeCommandHandler", "обработчик", null, arr("Handle"), 735, 660, 230, 90);
        clazz(app, "t_tasks_query", "GetProjectTasksQuery", "запрос",
                arr("ProjectId: Guid", "Status: ProjectTaskStatus?", "Page: int"), null, 465, 805, 230, 115);
        clazz(app, "t_query_handler", "GetProjectTasksQueryHandler", "обработчик запроса", null, arr("Handle"), 735, 805, 230, 90);
        clazz(app, "t_cache_i", "ICacheService", "интерфейс", null, arr("GetAsync", "SetAsync", "RemoveAsync"), 2200, 100, 190, 120);
        clazz(app, "t_event_repo_i", "IIncomingEventRepository", "интерфейс", null, arr("GetPendingAsync", "MarkProcessedAsync"), 2470, 675, 210, 105);
        clazz(app, "t_auth_i", "IAuthorizationService", "интерфейс", null, arr("GetAccessToken"), 985, 225, 170, 85);

        clazz(domain, "t_task", "ProjectTask", "агрегат",
                arr("Id: Guid", "ProjectId: Guid", "Title: string", "AssigneeId: Guid?", "ReporterId: Guid", "Status: ProjectTaskStatus", "Priority: TaskPriority"),
                arr("CreateDraft", "Assign", "MarkInProgress", "MarkCompleted", "Cancel", "AddComment", "AddTimeEntry"),
                1130, 100, 270, 280);
        clazz(domain, "t_comment", "Comment", "сущность",
                arr("Id: Guid", "TaskId: Guid", "AuthorId: Guid", "Text: string", "CreatedAt: DateTime"), null, 1450, 115, 160, 145);
        clazz(domain, "t_time", "TimeEntry", "сущность",
                arr("Id: Guid", "UserId: Guid", "TaskId: Guid", "Period: TimeRange", "Duration: TimeSpan"), null, 1450, 335, 160, 145);
        clazz(domain, "t_range", "TimeRange", "value object",
                arr("Start: DateTime", "End: DateTime", "Duration: TimeSpan"), null, 1450, 565, 160, 115);
        clazz(domain, "t_money", "Money", "value object",
                arr("Amount: decimal", "Currency: string"), null, 1450, 760, 160, 100);
        clazz(domain, "t_status", "ProjectTaskStatus", "enum",
                arr("ToDo", "InProgress", "Completed", "Cancelled"), null, 1130, 560, 160, 120);
        clazz(domain, "t_priority", "TaskPriority", "enum",
                arr("Low", "Medium", "High"), null, 1310, 560, 120, 105);
        clazz(domain, "t_events", "DomainEvent", "события",
                arr("TaskCreatedDomainEvent", "TaskAssignedDomainEvent", "TaskCompletedDomainEvent", "CommentAddedDomainEvent", "TimeEntryAddedDomainEvent"), null, 1130, 760, 240, 170);
        clazz(domain, "t_repo_i", "IProjectTaskRepository", "интерфейс", null,
                arr("GetByIdAsync", "AddAsync", "UpdateAsync", "DeleteAsync"), 1400, 910, 200, 130);

        clazz(persistence, "t_db", "ProjectTasksDbContext", "EF Core",
                arr("Tasks", "Comments", "TimeEntries", "ReadModels", "IncomingEvents", "OutboxMessages"), null, 1785, 100, 250, 150);
        clazz(persistence, "t_repo", "ProjectTaskRepository", "репозиторий", null, null, 1785, 325, 250, 80);
        clazz(persistence, "t_qs", "ProjectTaskQueryService", "запросы", null, null, 1785, 480, 250, 80);
        clazz(persistence, "t_comment_repo", "CommentReadRepository", "read repository", null, null, 1785, 635, 250, 80);
        clazz(persistence, "t_member_model", "MemberReadModel", "read model", arr("UserId: Guid", "ProjectId: Guid", "Role: string"), null, 1785, 780, 250, 110);
        clazz(persistence, "t_project_model", "ProjectReadModel", "read model", arr("Id: Guid", "Title: string", "OwnerId: Guid"), null, 1785, 910, 250, 110);

        clazz(infra, "t_cache", "RedisCacheService", "Redis", null, null, 2470, 120, 220, 80);
        clazz(infra, "t_users_consumer", "UsersConsumerHostedService", "Kafka consumer", null, null, 2240, 260, 220, 80);
        clazz(infra, "t_projects_consumer", "ProjectsConsumerHostedService", "Kafka consumer", null, null, 2240, 410, 220, 80);
        clazz(infra, "t_members_consumer", "MembersConsumerHostedService", "Kafka consumer", null, null, 2240, 560, 220, 80);
        clazz(infra, "t_processor", "IncomingEventsProcessorHostedService", "фоновый сервис", null, null, 2240, 720, 220, 80);
        clazz(infra, "t_publisher", "OutboxPublisherHostedService", "фоновый сервис", null, null, 2240, 900, 220, 80);

        dependency("t_ctrl", "t_mediator");
        dependency("t_create_cmd", "t_create_handler");
        dependency("t_assign_cmd", "t_assign_handler");
        dependency("t_comment_cmd", "t_comment_handler");
        dependency("t_time_cmd", "t_time_handler");
        dependency("t_tasks_query", "t_query_handler");
        dependency("t_create_handler", "t_repo_i");
        dependency("t_assign_handler", "t_repo_i");
        dependency("t_comment_handler", "t_repo_i");
        dependency("t_time_handler", "t_repo_i");
        dependency("t_query_handler", "t_qs");
        dependency("t_create_handler", "t_auth_i");

        composition("t_task", "t_comment", "1", "0..*");
        composition("t_task", "t_time", "1", "0..*");
        composition("t_time", "t_range", "1", "1");
        association("t_time", "t_money", "0..1", "1");
        association("t_task", "t_status", "1", "1");
        association("t_task", "t_priority", "1", "1");
        dependency("t_task", "t_events");

        realization("t_repo", "t_repo_i");
        dependency("t_repo", "t_db");
        dependency("t_qs", "t_db");
        dependency("t_comment_repo", "t_db");
        realization("t_cache", "t_cache_i");
        dependency("t_users_consumer", "t_event_repo_i");
        dependency("t_projects_consumer", "t_event_repo_i");
        dependency("t_members_consumer", "t_event_repo_i");
        dependency("t_processor", "t_event_repo_i");
        dependency("t_publisher", "t_db");
    }

    private IClassDiagramUIModel createDiagram(String name) {
        IClassDiagramUIModel diagram = (IClassDiagramUIModel) diagramManager.createDiagram(DiagramManager.DIAGRAM_TYPE_CLASS_DIAGRAM);
        diagram.setName(name);
        diagram.setBounds(0, 0, 2850, 1150);
        diagram.setShowConnectorName(IDiagramUIModel.SHOW_CONNECTOR_NAME_NO);
        diagram.setPaintConnectorThroughLabel(IDiagramUIModel.PAINT_CONNECTOR_THROUGH_LABEL_NO);
        diagram.setConnectorLineJumps(1);
        diagram.setConnectorLineJumpsSize(12);
        diagram.setConnectionPointStyle(IShapeUIModel.CONNECTION_POINT_TYPE_CENTER);
        diagram.setClassFitSizeWhenShowHideMember(false);
        createdDiagrams.add(diagram);
        return diagram;
    }

    private Layer layer(IClassDiagramUIModel diagram, String id, String name, int x, int y, int w, int h, Color color) {
        IPackage model = FACTORY.createPackage();
        model.setName(name);
        Layer layer = new Layer(id, model, diagram, color);
        layerLabel(layer, name.replace(" слой", ""), x + 20, y + 20, Math.min(w - 40, 260));
        return layer;
    }

    private void layerLabel(Layer layer, String name, int x, int y, int w) {
        IClass model = FACTORY.createClass();
        model.setName(name);
        model.addStereotype("слой");
        layer.model.addChild(model);

        IClassUIModel shape = (IClassUIModel) diagramManager.createDiagramElement(layer.diagram, model);
        shape.setBounds(x, y, w, 54);
        shape.setBackground(new Color(245, 245, 245));
        shape.setRequestResetCaption(true);
        shape.setConnectionPointType(IShapeUIModel.CONNECTION_POINT_TYPE_CENTER);
    }

    private void clazz(Layer layer, String id, String name, String stereotype, String[] attrs, String[] ops,
                       int x, int y, int w, int h) {
        IClass model = FACTORY.createClass();
        model.setName(name);
        if (stereotype != null && !stereotype.isBlank()) {
            model.addStereotype(stereotype);
        }
        if (attrs != null) {
            for (String attrSpec : attrs) {
                IAttribute attr = FACTORY.createAttribute();
                String[] parts = attrSpec.split(":", 2);
                attr.setName(parts[0].trim());
                attr.setVisibility(IAttribute.VISIBILITY_PUBLIC);
                if (parts.length > 1) {
                    attr.setType(parts[1].trim());
                }
                model.addAttribute(attr);
            }
        }
        if (ops != null) {
            for (String opName : ops) {
                IOperation op = FACTORY.createOperation();
                op.setName(opName.replace("()", ""));
                op.setVisibility(IOperation.VISIBILITY_PUBLIC);
                model.addOperation(op);
            }
        }
        layer.model.addChild(model);

        IClassUIModel shape = (IClassUIModel) diagramManager.createDiagramElement(
                layer.diagram, model);
        shape.setBounds(x, y, w, h);
        shape.setBackground(layer.color);
        shape.setRequestResetCaption(true);
        shape.setConnectionPointType(IShapeUIModel.CONNECTION_POINT_TYPE_CENTER);
        boxes.put(id, new Box(model, shape));
    }

    private void dependency(String fromId, String toId) {
        Box from = boxes.get(fromId);
        Box to = boxes.get(toId);
        IDependency model = FACTORY.createDependency();
        model.setFrom(from.model);
        model.setTo(to.model);
        connector(model, from, to);
    }

    private void realization(String fromId, String toId) {
        Box from = boxes.get(fromId);
        Box to = boxes.get(toId);
        IRealization model = FACTORY.createRealization();
        model.setFrom(from.model);
        model.setTo(to.model);
        connector(model, from, to);
    }

    private void generalization(String fromId, String toId) {
        Box from = boxes.get(fromId);
        Box to = boxes.get(toId);
        IGeneralization model = FACTORY.createGeneralization();
        model.setFrom(from.model);
        model.setTo(to.model);
        connector(model, from, to);
    }

    private void association(String fromId, String toId, String fromMult, String toMult) {
        association(fromId, toId, fromMult, toMult, null);
    }

    private void composition(String fromId, String toId, String fromMult, String toMult) {
        association(fromId, toId, fromMult, toMult, IAssociationEnd.AGGREGATION_KIND_COMPOSITED);
    }

    private void association(String fromId, String toId, String fromMult, String toMult, String aggregationKind) {
        Box from = boxes.get(fromId);
        Box to = boxes.get(toId);
        IAssociation model = FACTORY.createAssociation();
        model.setFrom(from.model);
        model.setTo(to.model);
        IAssociationEnd fromEnd = (IAssociationEnd) model.getFromEnd();
        IAssociationEnd toEnd = (IAssociationEnd) model.getToEnd();
        fromEnd.setMultiplicity(fromMult);
        toEnd.setMultiplicity(toMult);
        if (aggregationKind != null) {
            fromEnd.setAggregationKind(aggregationKind);
        }
        IAssociationUIModel connector = (IAssociationUIModel) diagramManager.createConnector(
                from.shape.getDiagramUIModel(), model, from.shape, to.shape, null);
        styleConnector(connector);
    }

    private void connector(IModelElement model, Box from, Box to) {
        IDiagramElement connector = diagramManager.createConnector(
                from.shape.getDiagramUIModel(), model, from.shape, to.shape, (Point[]) null);
        styleConnector(connector);
    }

    private void styleConnector(IDiagramElement connector) {
        if (connector instanceof IConnectorUIModel) {
            IConnectorUIModel c = (IConnectorUIModel) connector;
            c.setConnectorStyle(IConnectorUIModel.CS_RECTI_LINEAR);
            c.setConnectorLineJumps(1);
        }
        connector.setRequestResetCaption(true);
    }

    private static String[] arr(String... values) {
        return values;
    }

    private static final class Layer {
        final String id;
        final IPackage model;
        final IClassDiagramUIModel diagram;
        final Color color;

        Layer(String id, IPackage model, IClassDiagramUIModel diagram, Color color) {
            this.id = id;
            this.model = model;
            this.diagram = diagram;
            this.color = color;
        }
    }

    private static final class Box {
        final IClass model;
        final IClassUIModel shape;

        Box(IClass model, IClassUIModel shape) {
            this.model = model;
            this.shape = shape;
        }
    }
}
