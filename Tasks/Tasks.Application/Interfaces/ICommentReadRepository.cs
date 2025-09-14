using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Application.DTOs;

namespace Tasks.Application.Interfaces
{
	public interface ICommentReadRepository
	{
		Task<List<CommentReadDto>> GetCommentsByTaskIdAsync(Guid taskId, CancellationToken ct);
	}
}
