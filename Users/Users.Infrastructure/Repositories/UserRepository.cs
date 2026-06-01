using FluentResults;
using Microsoft.EntityFrameworkCore;
using Users.Domain.Entities;
using Users.Domain.Interfaces.Repositories;
using Users.Domain.ValueObjects;
using Users.Infrastructure.Data;
using Users.Infrastructure.Models;
using MapsterMapper;
using Mapster;
using Microsoft.Extensions.Logging;

namespace Users.Infrastructure.Repositories
{
	public class UserRepository : IUserRepository
	{
		private readonly UsersDbContext _context;
		private readonly ILogger<UserRepository> _logger;
		private readonly IMapper _mapper;
		private readonly TypeAdapterConfig _config;

		public UserRepository(
			UsersDbContext context,
			ILogger<UserRepository> logger,
			IMapper mapper,
			TypeAdapterConfig config)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
			_config = config ?? throw new ArgumentNullException(nameof(config));
		}

		public async Task Add(User user, CancellationToken cancellationToken)
		{
			var userData = _mapper.Map<UserData>(user);
			await _context.Users.AddAsync(userData, cancellationToken);
		}

		public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken)
		{
			return await _context.Users
				.AsNoTracking()
				.AnyAsync(u => u.Email == email, cancellationToken);
		}

		public async Task<User?> GetByEmail(string email, CancellationToken cancellationToken)
		{
			var userData = await _context.Users
				.AsNoTracking()
				.Include(u => u.UserRoles)
				.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

			if (userData == null)
				return null;

			try
			{
				return _mapper.Map<User>(userData);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Mapping error for user with email {Email}", email);
				return null;
			}
		}

		public async Task<User?> GetById(Guid id, CancellationToken cancellationToken)
		{
			var userData = await _context.Users
				.Include(u => u.UserRoles)
				.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

			if (userData == null)
				return null;

			try
			{
				return _mapper.Map<User>(userData);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Mapping error for user with ID {UserId}", id);
				return null;
			}
		}
		public async Task<User> GetByLogin(string login, CancellationToken cancellationToken)
		{
			var userData = await _context.Users
				.Include(u => u.UserRoles)
				.FirstOrDefaultAsync(u => u.Login == login, cancellationToken);

			if (userData == null)
				return null;

			try
			{
				return _mapper.Map<User>(userData);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Mapping error for user with login {Login}", login);
				return null;
			}
		}

		public async Task<User?> GetByName(string name, CancellationToken cancellationToken)
		{
			var userData = await _context.Users
				.AsNoTracking()
				.Include(u => u.UserRoles)
				.FirstOrDefaultAsync(u => u.Name == name, cancellationToken);

			if (userData == null)
				return null;

			try
			{
				return _mapper.Map<User>(userData);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Mapping error for user with name {Name}", name);
				return null;
			}
		}

		public async Task Update(User user, CancellationToken cancellationToken)
		{
			if (user == null)
			{
				_logger.LogWarning("Trying to update null user");
				throw new ArgumentNullException(nameof(user));
			}

			try
			{
				_logger.LogDebug("Updating user with ID {UserId}", user.Id);

				var existing = await _context.Users
					.Include(u => u.UserRoles)
					.FirstOrDefaultAsync(u => u.Id == user.Id, cancellationToken);

				if (existing == null)
				{
					_logger.LogWarning("User with ID {UserId} not found for update", user.Id);
					throw new KeyNotFoundException($"User with ID {user.Id} not found.");
				}

				_mapper.Map(user, existing);

				_logger.LogInformation("User with ID {UserId} mapped into persistence entity (patched)", user.Id);

				// Диагностика: какие сущности помечены как изменённые перед SaveChanges
				var entries = _context.ChangeTracker.Entries()
					.Where(e => e.State != EntityState.Unchanged)
					.ToList();

				if (!entries.Any())
				{
					_logger.LogWarning("No tracked changes detected before SaveChanges for user {UserId}", user.Id);
				}
				else
				{
					foreach (var entry in entries)
					{
						var entityType = entry.Entity.GetType().Name;
						var state = entry.State;
						var idProperty = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
						var idValue = idProperty?.CurrentValue?.ToString() ?? "N/A";

						_logger.LogInformation("Tracked entity: {EntityType}, ID: {Id}, State: {State}",
							entityType, idValue, state);
					}
				}

			}
			catch (OperationCanceledException) { throw; }
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to update user with ID {UserId}", user.Id);
				throw;
			}
		}


		public async Task Delete(Guid id, CancellationToken cancellationToken)
		{
			var user = await _context.Users
				.Include(u => u.UserRoles)
				.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

			if (user != null)
			{
				_context.UserRoles.RemoveRange(user.UserRoles);
				_context.Users.Remove(user);
			}
		}

		public async Task<IEnumerable<User>> ListActive(CancellationToken cancellationToken)
		{
			var query = _context.Users
				.AsNoTracking()
				.Where(u => u.RevokedOn == null)
				.Include(u => u.UserRoles);

			var users = await query
				.ProjectToType<User>(_mapper.Config)
				.ToListAsync(cancellationToken);

			return users;
		}

		public async Task<IEnumerable<User>> ListByAge(int minAge, CancellationToken cancellationToken)
		{
			var cutoffDate = DateTime.UtcNow.AddYears(-minAge);

			var query = _context.Users
				.AsNoTracking()
				.Where(u => u.RevokedOn == null
							&& u.Birthday <= cutoffDate)
				.Include(u => u.UserRoles);

			var users = await query
				.ProjectToType<User>(_mapper.Config)
				.ToListAsync(cancellationToken);

			return users;
		}

		public async Task<List<string>> GetUserPermissions(Guid userId, CancellationToken ct)
		{
			return await _context.Users
				.Where(u => u.Id == userId)
				.SelectMany(u => u.UserRoles) 
				.Select(ur => ur.Role)        
				.SelectMany(r => r.RolePermissions) 
				.Select(rp => rp.Permission.Name)   
				.Distinct()                         
				.ToListAsync(ct);                   
		}

		public async Task<List<string>> GetUserRoles(Guid userId, CancellationToken ct)
		{
			return await _context.UserRoles
				.Where(ur => ur.UserId == userId)
				.Join(
					_context.Roles,
					userRole => userRole.RoleId,
					role => role.Id,
					(userRole, role) => role.Name 
				)
				.Distinct()
				.ToListAsync(ct);
		}
	}
}
