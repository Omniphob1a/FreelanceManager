using Projects.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Domain.Events
{
	public sealed record TagsDeletedDomainEvent(Guid ProjectId, Tag Tag) : DomainEvent(ProjectId);
}
