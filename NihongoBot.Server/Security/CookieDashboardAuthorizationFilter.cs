using Hangfire.Dashboard;

namespace NihongoBot.Server.Security;

// Authorizes Hangfire Dashboard requests based on ASP.NET Core Identity cookie auth.
// Allows access only when the user is authenticated (you can extend to check roles/claims).
public sealed class CookieDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
	public bool Authorize(DashboardContext context)
	{
		HttpContext httpContext = context.GetHttpContext();
		return httpContext.User?.Identity?.IsAuthenticated == true;
	}
}
