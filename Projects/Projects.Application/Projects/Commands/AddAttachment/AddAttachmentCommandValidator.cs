using FluentValidation;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Projects.Commands.AddAttachment
{
	public class AddAttachmentCommandValidator : AbstractValidator<AddAttachmentCommand>
	{
		private static readonly string[] Allowed = { ".pdf", ".docx", ".png", ".jpg" };
		private const long MaxBytes = 10_000_000;     // 10 MB

		public AddAttachmentCommandValidator()
		{
			RuleFor(x => x.File)
				.NotNull().WithMessage("File is necessary")
				.Must(f => f!.Length > 0).WithMessage("File is empty")
				.Must(f => f!.Length <= MaxBytes).WithMessage("File is too big")
				.Must(HasAllowedExtension).WithMessage("Wrong file type");
		}

		private static bool HasAllowedExtension(IFormFile file)
		{
			var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
			return Allowed.Contains(ext);
		}
	}
}
