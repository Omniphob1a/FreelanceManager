using Projects.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Domain.Events
{
	public record AttachmentRemovedDomainEvent(Guid ProjectId, ProjectAttachment Attachment)	: DomainEvent(ProjectId);
}
