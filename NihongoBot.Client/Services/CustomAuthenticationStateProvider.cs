using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;

namespace NihongoBot.Client.Services;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
	private readonly HttpClient _httpClient;
	private ClaimsPrincipal _cachedUser = new(new ClaimsIdentity());

	public CustomAuthenticationStateProvider(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	public override async Task<AuthenticationState> GetAuthenticationStateAsync()
	{
		var user = await GetUserAsync();
		return new AuthenticationState(user);
	}

	private async Task<ClaimsPrincipal> GetUserAsync()
	{
		try
		{
			var response = await _httpClient.GetAsync("/api/auth/user");
			if (response.IsSuccessStatusCode)
			{
				var content = await response.Content.ReadAsStringAsync();
				var userInfo = JsonSerializer.Deserialize<JsonElement>(content);
				
				var claims = new List<Claim>
				{
					new Claim(ClaimTypes.Name, userInfo.GetProperty("username").GetString() ?? ""),
					new Claim(ClaimTypes.Email, userInfo.GetProperty("email").GetString() ?? ""),
					new Claim(ClaimTypes.NameIdentifier, userInfo.GetProperty("id").GetString() ?? ""),
					new Claim(ClaimTypes.Role, "Admin") // All authenticated users are admins in this context
				};

				var identity = new ClaimsIdentity(claims, "Server authentication");
				_cachedUser = new ClaimsPrincipal(identity);
				return _cachedUser;
			}
		}
		catch (Exception)
		{
			// Authentication failed
		}

		_cachedUser = new ClaimsPrincipal(new ClaimsIdentity());
		return _cachedUser;
	}

	public async Task LoginAsync(string username, string password, bool rememberMe)
	{
		var loginRequest = new
		{
			Username = username,
			Password = password,
			RememberMe = rememberMe
		};

		var response = await _httpClient.PostAsJsonAsync("/api/auth/login", loginRequest);
		if (response.IsSuccessStatusCode)
		{
			var user = await GetUserAsync();
			NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
		}
		else
		{
			throw new UnauthorizedAccessException("Login failed");
		}
	}

	public async Task LogoutAsync()
	{
		await _httpClient.PostAsync("/api/auth/logout", null);
		_cachedUser = new ClaimsPrincipal(new ClaimsIdentity());
		NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_cachedUser)));
	}
}