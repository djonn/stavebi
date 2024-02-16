using Microsoft.EntityFrameworkCore;
using StaveBi.Application;
using StaveBi.Database;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<GameContext>();
builder.Services.AddScoped((sp) => new GameGenerator("full_wordlist.tsv"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
  scope.ServiceProvider.GetService<GameContext>().Database.Migrate();
}

var publicRoute = app.MapGroup("/api");
StaveBi.Route.Api.Map(publicRoute);

// TODO: add auth to admin
var adminRoute = app.MapGroup("/admin");
StaveBi.Route.Admin.Map(adminRoute);

if (app.Environment.IsEnvironment("Development"))
{
  var debugRoute = app.MapGroup("/debug");
  StaveBi.Route.Debug.Map(debugRoute);
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.Run();
