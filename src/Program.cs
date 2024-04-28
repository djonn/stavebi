using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using StaveBi.Application;
using StaveBi.Database;
using StaveBi.Model;

GameContext CreateDb() {
    var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

  var db = new GameContext(config);

  db.Database.Migrate();

  if(db.Words.Count() == 0){
    Console.WriteLine("Seeding words into database");

    var generator = new GameGenerator();
    var words = generator.GetValidWords().Select(x => new Word(x));
    db.Words.AddRange(words);
    db.SaveChanges();

    // generate new db so we don't keep changetracking all words
    db.Dispose();
    db = new GameContext(config);
  }

  return db;
}

void AddGames(int count) {
  var db = CreateDb();
  var generator = new GameGenerator();

  var createdCount = 0;

  while(createdCount<count){
    Console.WriteLine($"Remaining games to generate: {count - createdCount}");
    var newGame = generator.GenerateGame(db.Words.Select(x => x.Value).AsQueryable());

    if(db.Games.Any(x => x.Letters == newGame.Letters))
    {
      Console.WriteLine($"Game {newGame.Letters} already exists");
      continue;
    }
    
    db.Games.Add(newGame);
    db.SaveChanges();
    createdCount++;
  }
}

void GenerateFiles() {
  var db = CreateDb();
  var generator = new GameGenerator();

  var games = db.Games.ToList();
  Console.WriteLine($"{games.Count()} games found");
  
  var outputPath = "../dist/api";
  var gamesListFilePath = Path.Combine(outputPath, "games.json");
  File.WriteAllText(gamesListFilePath, JsonSerializer.Serialize(games));

  var wordsQuery = db.Words.Select(x => x.Value).AsQueryable();
  var gamesWithWords = games.ToDictionary(game => game.Letters, game => generator.findSolutions(game.Letters[0], game.Letters, wordsQuery));
  foreach (var game in gamesWithWords)
  {
    var gameFilePath = Path.Combine(outputPath, $"game-{game.Key}.json");
    File.WriteAllText(gameFilePath, JsonSerializer.Serialize(game.Value));
  }
}

// --------------

if(args.Count() == 2 && args[0].ToLower() == "add"){
  var gamesToCreate = int.Parse(args[1]);
  AddGames(gamesToCreate);
  return;
}

if(args.Count() == 1 && args[0].ToLower() == "generate"){
  Console.WriteLine("Generating JSON files from games in db");
  GenerateFiles();
  return;
}

if(args.Count() == 1 && args[0].ToLower() == "count"){
  var db = CreateDb();
  Console.WriteLine($"Db contains {db.Games.Count()} games");
  return;
}

if(args.Count() == 1 && args[0].ToLower() == "list"){
  var db = CreateDb();
  var gamesList = String.Join(Environment.NewLine, db.Games.Select(x => x.Letters));
  Console.WriteLine("Game list:");
  Console.WriteLine(gamesList);
  return;
}

// No valid action
Console.WriteLine("Please specify what you want done.");
Console.WriteLine("Options are [add, generate]");

