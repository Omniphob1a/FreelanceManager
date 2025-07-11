using Mapster;
using Projects.Domain.Entities;
using Projects.Domain.Entities.ProjectService.Domain.Entities;
using Projects.Domain.Enums;
using Projects.Domain.ValueObjects;
using Projects.Persistence.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;

namespace Projects.Persistence.Mappings
{
	public class ProjectMappingConfiguration : IRegister
	{
		public void Register(TypeAdapterConfig config)
		{
			config.NewConfig<ProjectEntity, Project>()
				.ConstructUsing(src => MapToDomain(src, config))
				.Ignore(dest => dest.Category);

			config.NewConfig<Project, ProjectEntity>()
				.Map(dest => dest.Id, src => src.Id)
				.Map(dest => dest.BudgetMin, src => src.Budget.Min)
				.Map(dest => dest.BudgetMax, src => src.Budget.Max)
				.Map(dest => dest.CurrencyCode, src => src.Budget.CurrencyCode.ToString())
				.Map(dest => dest.Category, src => src.Category.ToString())
				.Map(dest => dest.Milestones, src => src.Milestones.Adapt<List<ProjectMilestoneEntity>>(config))
				.Map(dest => dest.Attachments, src => src.Attachments.Adapt<List<ProjectAttachmentEntity>>(config))
				.Map(dest => dest.Status, src => (int)src.Status)
				.Map(dest => dest.ExpiresAt, src => src.ExpiresAt)
				.Map(dest => dest.Tags, src => string.Join(",", src.Tags.Select(t => t.Value)));
		}

		private static Project MapToDomain(ProjectEntity src, TypeAdapterConfig config)
		{
			var budget = new Budget(src.BudgetMin, src.BudgetMax, CurrencyCode.From(src.CurrencyCode));
			var category = Category.From(src.Category);
			var tags = (src.Tags ?? "")
				.Split(',', StringSplitOptions.RemoveEmptyEntries)
				.Select(Tag.From)
				.ToList();

			var milestones = src.Milestones?.Adapt<List<ProjectMilestone>>(config) ?? new();
			var attachments = src.Attachments?.Adapt<List<ProjectAttachment>>(config) ?? new();
			var status = (ProjectStatus)src.Status;

			var project = Project.Restore(
				id: src.Id,
				title: src.Title,
				description: src.Description,
				ownerId: src.OwnerId,
				budget: budget,
				category: category,
				tags: tags,
				milestones: milestones,
				attachments: attachments,
				status: status,
				expiresAt: src.ExpiresAt
			);

			return project;
		}
	}
}
