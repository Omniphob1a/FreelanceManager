﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Domain.Events
{
	public record ProjectCompletedDomainEvent(Guid ProjectId) : DomainEvent(ProjectId);
}
