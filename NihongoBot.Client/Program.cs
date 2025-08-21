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

// Add authentication service
builder.Services.AddScoped<AuthenticationService>();

await builder.Build().RunAsync();
