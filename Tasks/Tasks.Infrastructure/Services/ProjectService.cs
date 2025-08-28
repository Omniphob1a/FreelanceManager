using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Tasks.Application.DTOs;
using Tasks.Application.Interfaces;

namespace Tasks.Infrastructure.Services
{
	public class ProjectService : IProjectService
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger<ProjectService> _logger;
		private readonly IAuthorizationService _authService;

		public ProjectService(HttpClient httpClient, ILogger<ProjectService> logger, IAuthorizationService authService)
		{
			_httpClient = httpClient;
			_logger = logger;
			_logger.LogInformation("ProjectService BaseAddress: {BaseAddress}", _httpClient.BaseAddress);
			_authService = authService;
		}

		public async Task<bool> ExistsAsync(Guid projectId, CancellationToken cancellationToken)
		{
			var token = _authService.GetAccessToken();

			if (!string.IsNullOrEmpty(token))
			{
				_httpClient.DefaultRequestHeaders.Authorization =
					new AuthenticationHeaderValue("Bearer", token);
			}

			_logger.LogInformation("Checking if project {ProjectId} exists", projectId);

			var response = await _httpClient.GetAsync($"projects/{projectId}", cancellationToken);

			if (response.StatusCode == HttpStatusCode.NotFound)
			{
				_logger.LogWarning("Project {ProjectId} not found", projectId);
				return false;
			}

			if (response.IsSuccessStatusCode)
			{
				_logger.LogInformation("Project {ProjectId} exists", projectId);
				return true;
			}

			_logger.LogError("Unexpected status code {StatusCode} when checking project {ProjectId}",
				response.StatusCode, projectId);

			throw new HttpRequestException(
				$"Unexpected status code {response.StatusCode} when checking project {projectId}");
		}

		public async Task<ProjectDto?> GetProjectByIdAsync(Guid projectId, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Trying to get project {ProjectId}", projectId);

			var response = await _httpClient.GetAsync($"projects/{projectId}", cancellationToken);

			if (response.StatusCode == HttpStatusCode.NotFound)
				return null; 

			response.EnsureSuccessStatusCode(); 

			var project = await response.Content.ReadFromJsonAsync<ProjectDto>(
				cancellationToken: cancellationToken);

			return project;
		}
	}
}
