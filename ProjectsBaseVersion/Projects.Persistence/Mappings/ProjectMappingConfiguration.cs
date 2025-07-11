using Mapster;
using Projects.Domain.Entities;
using Projects.Domain.Entities.ProjectService.Domain.Entities;
using Projects.Domain.Enums;
using Projects.Domain.ValueObjects;
using Projects.Persistence.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Persistence.Mappings
{
	public class ProjectMappingConfiguration : IRegister
	{
		public void Register(TypeAdapterConfig config)
		{
			config.NewConfig<ProjectEntity, Project>()
				.ConstructUsing(src => MapToDomain(src));

			config.NewConfig<Project, ProjectEntity>()
				.Map(dest => dest.BudgetMin, src => src.Budget.Min)
				.Map(dest => dest.BudgetMax, src => src.Budget.Max)
				.Map(dest => dest.Currency, src => src.Budget.Currency)
				.Map(dest => dest.Milestones, src => src.Milestones.Adapt<List<ProjectMilestoneEntity>>())
				.Map(dest => dest.Attachments, src => src.Attachments.Adapt<List<ProjectAttachmentEntity>>())
				.Map(dest => dest.Status, src => (int)src.Status)
				.Map(dest => dest.ExpiresAt, src => src.ExpiresAt)
				.Map(dest => dest.Tags, src => src.Tags.ToList());
		}

		private static Project MapToDomain(ProjectEntity src)
		{
			var project = Project.CreateDraft(
				src.Title,
				src.Description,
				src.OwnerId,
				new Budget(src.BudgetMin, src.BudgetMax, src.Currency),
				src.Category,
				src.Tags
			);

			foreach (var milestone in src.Milestones?.Adapt<List<ProjectMilestone>>() ?? [])
				project.AddMilestone(milestone);

			foreach (var attachment in src.Attachments?.Adapt<List<ProjectAttachment>>() ?? [])
				project.AddAttachment(attachment);

			if (src.Status == (int)ProjectStatus.Completed)
				project.Complete();
			else if (src.Status == (int)ProjectStatus.Archived)
				project.Archive();
			else if (src.Status == (int)ProjectStatus.Active && src.ExpiresAt.HasValue)
				project.Publish(src.ExpiresAt.Value);

			return project;
		}
	}
}
