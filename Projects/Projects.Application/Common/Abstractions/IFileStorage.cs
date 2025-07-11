using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Common.Abstractions
{
	public interface IFileStorage
	{
		Task<string> SaveAsync(Stream fileStream, string fileName, CancellationToken ct);
		Task DeleteAsync(string fileName, CancellationToken ct );
		Task<Stream> DownloadAsync(string fileName, CancellationToken ct);
	}
}
