using Microsoft.EntityFrameworkCore;
using RaidBot2.Data;
using RaidBot2.PeriodicTasks;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDiscord(builder.Configuration);
builder.Services.AddPeriodicTask<RemoveExpiredRaidsTask>();
builder.Services.AddPeriodicTask<GuildExpansionTaskExecutor>();
builder.Services.AddPeriodicGuildExpansionTask<UpdateRaidTitlesTask>();
builder.Services.AddPeriodicGuildExpansionTask<UpdateRaidOrderTask>();

var app = builder.Build();

app.MapGet("/", () => $"Response created at {DateTimeOffset.UtcNow}");
app.MapGet("/ping", () => "pong");

await using (var migrationScope = app.Services.CreateAsyncScope())
{
    await migrationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync();
}
await app.RunAsync();
