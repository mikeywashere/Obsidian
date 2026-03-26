using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Authentication.WebAssembly.Msal;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Obsidian.Web;
using Obsidian.Web.Authorization;
using Obsidian.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure API HttpClient base URL
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:5001/";

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

// Register MSAL-authorized HTTP services — tokens are attached via BaseAddressAuthorizationMessageHandler.
// NOTE: BaseAddressAuthorizationMessageHandler attaches tokens when the request URL starts with the
// app's base address. If the API is at a different origin, configure AuthorizationMessageHandler instead.
builder.Services.AddHttpClient<IServerService, HttpServerService>(client =>
    client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

builder.Services.AddHttpClient<IServerPropertiesService, HttpServerPropertiesService>(client =>
    client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

builder.Services.AddHttpClient<IServerPlayerService, HttpServerPlayerService>(client =>
    client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

builder.Services.AddHttpClient<HttpAdminService>(client =>
    client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();
builder.Services.AddScoped<IAdminService, HttpAdminService>();

await builder.Build().RunAsync();

