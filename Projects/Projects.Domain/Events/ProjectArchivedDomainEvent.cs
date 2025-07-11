using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Domain.Events
{
	public record ProjectArchivedDomainEvent(Guid ProjectId)	: DomainEvent(ProjectId);
}
