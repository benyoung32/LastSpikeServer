using GameplaySessionTracker.Services;
using GameplaySessionTracker.Hubs;
using GameplaySessionTracker.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

// Add CORS - environment-aware configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
         policy =>
         {
             var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
             policy.WithOrigins(allowedOrigins)
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .AllowCredentials();
         });
});


string connectionString = (builder.Environment.IsDevelopment()
    ? builder.Configuration.GetConnectionString("LocalConnection")
    : builder.Configuration.GetConnectionString("DefaultConnection"))
    ?? throw new InvalidOperationException("Connection string not found");

// Register repositories as Singletons with connection string
builder.Services.AddSingleton<ISessionRepository>(
    sp => new SessionRepository(connectionString));
builder.Services.AddSingleton<IPlayerRepository>(
    sp => new PlayerRepository(connectionString));

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IGameBoardRepository>(
    sp => new GameBoardRepository(
        connectionString,
        sp.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>()));

// Register services as Singletons
builder.Services.AddSingleton<ISessionService, SessionService>();
builder.Services.AddSingleton<IPlayerService, PlayerService>();
builder.Services.AddSingleton<IGameBoardService, GameBoardService>();

builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

Console.WriteLine(connectionString);

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();
app.MapHub<GameHub>("/gamehub");

app.Run();
