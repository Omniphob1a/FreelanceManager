using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasks.Application.DTOs
{
	public class CommentReadDto
	{
		public Guid Id { get; set; }
		public Guid TaskId { get; set; }
		public string Text { get; set; } = default!;
		public DateTime CreatedAt { get; set; }
		public PublicUserDto? Author { get; set; } = default!;
	}
}
