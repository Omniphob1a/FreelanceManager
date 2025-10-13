using Tasks.Application.DTOs;
using Tasks.Application.Interfaces;
using Tasks.Persistence.Models.ReadModels;

namespace Tasks.Api.GraphQL.DataLoaders
{
	public class UserByIdDataLoader : BatchDataLoader<Guid, PublicUserDto>
	{
		private readonly IUserReadRepository _repo;

		public UserByIdDataLoader(IBatchScheduler batchScheduler, IUserReadRepository repo, DataLoaderOptions? options = null)
			: base(batchScheduler, options)
		{
			_repo = repo ?? throw new ArgumentNullException(nameof(repo));
		}

		protected override async Task<IReadOnlyDictionary<Guid, PublicUserDto>> LoadBatchAsync(
			IReadOnlyList<Guid> keys,
			CancellationToken cancellationToken)
		{
			// Предполагается, что GetByIdsAsync возвращает List<PublicUserDto> или Dictionary<Guid,PublicUserDto>
			var users = await _repo.GetByIdsAsync(keys, cancellationToken);

			// Если repo вернул Dictionary<Guid,PublicUserDto>, приведём к нужному виду:
			if (users is IDictionary<Guid, PublicUserDto> dict)
			{
				return new Dictionary<Guid, PublicUserDto>(dict);
			}

			// Если возвращает IEnumerable<PublicUserDto> или List<PublicUserDto>:
			return users.ToDictionary(u => u.Id);
		}
	}
}
