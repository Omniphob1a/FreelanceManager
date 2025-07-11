using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using Projects.Application.Common.Abstractions;
using Projects.Infrastructure.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Infrastructure.FileStorage
{
	public class S3FileStorage : IFileStorage
	{
		private readonly IAmazonS3 _s3;
		private readonly S3Options _options;

		public S3FileStorage(IAmazonS3 s3, IOptions<S3Options> options)
		{
			_s3 = s3;
			_options = options.Value;
		}

		public async Task DeleteAsync(string fileName, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(fileName))
				throw new ArgumentException("Имя файла не может быть пустым.", nameof(fileName));

			var request = new DeleteObjectRequest
			{
				BucketName = _options.BucketName, 
				Key = fileName             
			};

			try
			{
				await _s3.DeleteObjectAsync(request, cancellationToken);
			}
			catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
			{
			}
		}

		public async Task<Stream> DownloadAsync(string fileName, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(fileName))
				throw new ArgumentException("File name cannot be empt.", nameof(fileName));

			var request = new GetObjectRequest
			{
				BucketName = _options.BucketName,
				Key = fileName
			};

			using var response = await _s3.GetObjectAsync(request, cancellationToken);

			var memory = new MemoryStream();
			await response.ResponseStream.CopyToAsync(memory, cancellationToken);

			memory.Position = 0;
			return memory;            
		}

		public async Task<string> SaveAsync(Stream fileStream, string fileName, CancellationToken ct)
		{
			var key = $"attachments/{Guid.NewGuid()}_{fileName}";

			var request = new PutObjectRequest
			{
				BucketName = _options.BucketName,
				Key = key,
				InputStream = fileStream,
				ContentType = "application/octet-stream"
			};

			await _s3.PutObjectAsync(request, ct);

			return $"https://{_options.BucketName}.s3.{_options.Region}.amazonaws.com/{key}";
		}
	}

}
