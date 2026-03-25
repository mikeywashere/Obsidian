using Obsidian.Api.Hubs;
using Obsidian.Api.Services;

var builder = WebApplication.CreateBuilder(args);

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

app.UseAuthorization();

app.MapControllers();
app.MapHub<ServerLogHub>("/hubs/serverlogs");
app.MapHub<PlayerHub>("/hubs/players");

app.Run();
