using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Obsidian.Api.Hubs;
using Obsidian.Api.Services;
using Obsidian.DataAccess;
using Obsidian.Models.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS (configured for SignalR with credentials)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:7001", "http://localhost:5002")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Add SignalR
builder.Services.AddSignalR();

// Database
builder.Services.AddDbContext<ObsidianDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=obsidian.db"));

// Health checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy())
    .AddDbContextCheck<ObsidianDbContext>("database");

// Authentication — validate Azure AD JWT bearer tokens
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"{builder.Configuration["AzureAd:Instance"]}{builder.Configuration["AzureAd:TenantId"]}";
        options.Audience = builder.Configuration["AzureAd:Audience"];
        options.TokenValidationParameters.ValidateIssuer = false; // multi-tenant
    });

// Claims transformation — injects role claims from local DB overrides
builder.Services.AddScoped<IClaimsTransformation, AdminOverrideClaimsTransformation>();

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.RequireSystemAdmin, policy => policy.RequireRole(Roles.SystemAdmin));
    options.AddPolicy(Policies.RequireAdmin, policy => policy.RequireRole(Roles.Admin, Roles.SystemAdmin));
    options.AddPolicy(Policies.RequireUser, policy => policy.RequireRole(Roles.User, Roles.Admin, Roles.SystemAdmin));
});

// Register server manager
builder.Services.AddSingleton<IServerManager, ServerManager>();

// Register player tracker
builder.Services.AddSingleton<IPlayerTracker, PlayerTracker>();

// Register SignalR broadcasters
builder.Services.AddHostedService<ServerLogBroadcaster>();
builder.Services.AddHostedService<PlayerBroadcaster>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.MapDefaultEndpoints();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ServerLogHub>("/hubs/serverlogs");
app.MapHub<PlayerHub>("/hubs/players");

app.Run();
