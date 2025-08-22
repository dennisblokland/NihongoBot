using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using NihongoBot.Client;
using NihongoBot.Client.Services;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");

// Configure HttpClient for API calls
builder.Services.AddScoped(sp => new HttpClient 
{ 
	BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) 
});

// Add authentication services
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => 
	provider.GetRequiredService<CustomAuthenticationStateProvider>());

await builder.Build().RunAsync();
