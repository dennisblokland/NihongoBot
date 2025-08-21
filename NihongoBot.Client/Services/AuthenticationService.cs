using System.Net;
using System.Text.Json;

namespace NihongoBot.Client.Services;

public class AuthenticationService
{
	private readonly HttpClient _httpClient;

	public AuthenticationService(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	public async Task<bool> IsAuthenticatedAsync()
	{
		try
		{
			var response = await _httpClient.GetAsync("/api/auth/user");
			return response.IsSuccessStatusCode;
		}
		catch
		{
			return false;
		}
	}

	public async Task<string?> GetCurrentUserAsync()
	{
		try
		{
			var response = await _httpClient.GetAsync("/api/auth/user");
			if (response.IsSuccessStatusCode)
			{
				var content = await response.Content.ReadAsStringAsync();
				var user = JsonSerializer.Deserialize<JsonElement>(content);
				return user.GetProperty("username").GetString();
			}
		}
		catch
		{
			// Fall through to return null
		}
		return null;
	}
}