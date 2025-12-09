using GameplaySessionTracker.Services;
using GameplaySessionTracker.Hubs;
using GameplaySessionTracker.Repositories;
using GameplaySessionTracker.GameRules;
// using GameplaySessionTracker.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Bind configuration to strongly-typed settings
// var appSettings = new AppSettings();
// builder.Configuration.Bind(appSettings);

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
            policy.WithOrigins("http://localhost:3000")
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost;Database=LastSpike;Integrated Security=true;TrustServerCertificate=true;";

// Register repositories as Singletons with connection string
builder.Services.AddSingleton<ISessionRepository>(
    sp => new SessionRepository(connectionString));
builder.Services.AddSingleton<IPlayerRepository>(
    sp => new PlayerRepository(connectionString));
builder.Services.AddSingleton<IGameBoardRepository, GameBoardRepository>();

// Register services as Singletons
builder.Services.AddSingleton<ISessionService, SessionService>();
builder.Services.AddSingleton<IPlayerService, PlayerService>();
// builder.Services.AddSingleton<IGameBoardService, GameBoardService>();
builder.Services.AddSingleton<IGameBoardService, GameBoardService>();
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();
app.MapHub<GameHub>("/gamehub");

app.Run();
