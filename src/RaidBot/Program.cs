using Microsoft.EntityFrameworkCore;
using RaidBot.Data;
using RaidBot.PeriodicTasks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextFactory<ApplicationDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDiscord(builder.Configuration);
builder.Services.AddPeriodicTask<RemoveExpiredRaidsTask>();
builder.Services.AddPeriodicTask<GuildExpansionTaskExecutor>();
builder.Services.AddPeriodicGuildExpansionTask<UpdateRaidTitlesTask>();
builder.Services.AddPeriodicGuildExpansionTask<UpdateRaidOrderTask>();

var app = builder.Build();

app.MapGet("/", () => $"Response created at {DateTimeOffset.UtcNow}");
app.MapGet("/ping", () => "pong");

{
    await using var db = await app.Services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContextAsync();
    await db.Database.MigrateAsync();
}
app.Run();
