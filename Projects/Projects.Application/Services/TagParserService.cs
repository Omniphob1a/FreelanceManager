using FluentResults;
using Microsoft.Extensions.Logging;
using Projects.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Services
{
	public class TagParserService
	{
		private readonly ILogger<TagParserService> _logger;

		public TagParserService(ILogger<TagParserService> logger)
		{
			_logger = logger;
		}

		public Result<List<Tag>> ParseTags(IEnumerable<string> tagStrings)
		{
			var tags = new List<Tag>();

			foreach (var tagString in tagStrings.Distinct())
			{
				try
				{
					var tag = Tag.From(tagString);
					tags.Add(tag);
				}
				catch (Exception ex)
				{
					_logger.LogWarning(ex, "Invalid tag: {TagString}", tagString);
					return Result.Fail<List<Tag>>($"Invalid tag: {tagString}");
				}
			}

			return Result.Ok(tags);
		}
	}

}
