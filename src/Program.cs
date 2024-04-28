using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using StaveBi.Application;
using StaveBi.Database;
using StaveBi.Model;

void RunWebApp() {
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
}

void RunFileGenerator() {
  var gameCount = 25;
  var outputPath = "../dist/api";

  var generator = new GameGenerator("full_wordlist.tsv");
  var words = generator.GetValidWords();

  var games = new List<Game>();

  while(games.Count < gameCount){
    var game = generator.GenerateGame(words.AsQueryable());

    if(!games.Any(x => x.Letters == game.Letters)){
      games.Add(game);
    }
  }

  var gamesListFilePath = Path.Combine(outputPath, "games.json");
  File.WriteAllText(gamesListFilePath, JsonSerializer.Serialize(games));

  foreach (var game in games)
  {
    var solutions = generator.findSolutions(game.Letters[0], game.Letters, words);
    var gameFilePath = Path.Combine(outputPath, $"game-{game.Letters}.json");
    File.WriteAllText(gameFilePath, JsonSerializer.Serialize(solutions));
  }

}

// RunWebApp();
RunFileGenerator();