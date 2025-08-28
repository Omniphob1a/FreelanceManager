using Amazon.Runtime.Internal.Util;
using Microsoft.Extensions.Logging;
using Projects.Application.DTOs;
using Projects.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Projects.Infrastructure.Services
{
	public class UserService : IUserService
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger<UserService> _logger;
		private readonly IAuthorizationService _authrizationService;

		public UserService(HttpClient httpClient, ILogger<UserService> logger, IAuthorizationService authrizationService)
		{
			_httpClient = httpClient;
			_logger = logger;
			_authrizationService = authrizationService;
		}

		public async Task<UserDto> GetUserByEmail(string email, CancellationToken cancellationToken)
		{
			var token = _authrizationService.GetAccessToken();

			if (!string.IsNullOrEmpty(token))
			{
				_httpClient.DefaultRequestHeaders.Authorization =
					new AuthenticationHeaderValue("Bearer", token);
			}

			_logger.LogInformation("Trying to get user by email {Email}", email);

			HttpResponseMessage response;

			try
			{
				response = await _httpClient.GetAsync($"users/by-email/{email}", cancellationToken);
			}
			catch (TaskCanceledException ex) when (cancellationToken.IsCancellationRequested)
			{
				_logger.LogWarning(ex, "Request to get user by email {Email} was canceled", email);
				return null;
			}
			catch (HttpRequestException ex)
			{
				_logger.LogError(ex, "Network error while trying to get user by email {Email}", email);
				throw; 
			}

			if (response.StatusCode == HttpStatusCode.NotFound)
			{
				_logger.LogWarning("User with email {Email} not found", email);
				return null;
			}

			if (!response.IsSuccessStatusCode)
			{
				_logger.LogError("Unexpected status code {StatusCode} when trying to get user with email {Email}",
					response.StatusCode, email);

				throw new HttpRequestException(
					$"Unexpected status code {response.StatusCode} when trying to get user with email {email}");
			}

			try
			{
				var userDto = await response.Content.ReadFromJsonAsync<UserDto>(cancellationToken: cancellationToken);

				if (userDto is null)
				{
					_logger.LogWarning("Response body was empty when retrieving user with email {Email}", email);
					return null;
				}

				_logger.LogInformation("User with email {Email} successfully retrieved", email);
				return userDto;
			}
			catch (NotSupportedException ex)
			{
				_logger.LogError(ex, "Unsupported content type when reading response for user {Email}", email);
				throw;
			}
			catch (JsonException ex)
			{
				_logger.LogError(ex, "Invalid JSON received when reading response for user {Email}", email);
				throw;
			}
		}
	}
}
