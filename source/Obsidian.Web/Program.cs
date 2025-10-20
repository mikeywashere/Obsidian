using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Authentication.WebAssembly.Msal;
using Obsidian.Web;
using Obsidian.Web.Authorization;
using Obsidian.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Configure Microsoft Authentication
// Note: To enable authentication, configure the AzureAd section in wwwroot/appsettings.json
// with your Azure AD application's ClientId
builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
    options.ProviderOptions.DefaultAccessTokenScopes.Add("User.Read");
});

// Configure Authorization Policies
builder.Services.AddAuthorizationCore(options =>
{
    // SystemAdmin policy - requires SystemAdmin role
    options.AddPolicy(Policies.RequireSystemAdmin, policy =>
        policy.RequireRole(Roles.SystemAdmin));

    // Admin policy - requires Admin or SystemAdmin role
    options.AddPolicy(Policies.RequireAdmin, policy =>
        policy.RequireRole(Roles.Admin, Roles.SystemAdmin));

    // User policy - requires User, Admin, or SystemAdmin role
    options.AddPolicy(Policies.RequireUser, policy =>
        policy.RequireRole(Roles.User, Roles.Admin, Roles.SystemAdmin));
});

// Register services
builder.Services.AddScoped<IServerService, MockServerService>();

await builder.Build().RunAsync();
