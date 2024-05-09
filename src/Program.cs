using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using StaveBi.Application;
using StaveBi.Database;

GameContext CreateDb() {
    var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

  var db = new GameContext(config);

  db.Database.Migrate();

  if(db.Words.Count() == 0){
    Console.WriteLine("Seeding words into database");

    var generator = new GameGenerator();
    var words = generator.GetValidWords();
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
    var newGame = generator.GenerateGame(db.Words.AsQueryable());

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

void AddGamesChallenge(int count) {
  var db = CreateDb();
  var generator = new GameGenerator();
  var wordsQuery = db.Words.AsQueryable();

  var createdCount = 0;

  while(createdCount<count){
    Console.WriteLine($"Remaining games to generate: {count - createdCount}");
    var newGame = generator.GenerateGame(wordsQuery);

    if(db.Games.Any(x => x.Letters == newGame.Letters))
    {
      Console.WriteLine($"Game {newGame.Letters} already exists");
      continue;
    }

    var solutions = generator.findSolutions(newGame.Letters[0], newGame.Letters, wordsQuery);

    Console.WriteLine("Challenge:");
    Console.WriteLine("  "+newGame.Letters);
    Console.WriteLine("Words:");
    Console.WriteLine("  "+string.Join("\n  ", solutions));
    Console.WriteLine("");
    Console.WriteLine("Do you accept? [y]/n");

    var response = Console.ReadKey();

    if(
      response.Key == ConsoleKey.Y
      || response.Key == ConsoleKey.Enter
    )
    {
      db.Games.Add(newGame);
      db.SaveChanges();
      createdCount++;
    }
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

  var wordsQuery = db.Words.AsQueryable();
  var gamesWithWords = games.ToDictionary(game => game.Letters, game => generator.findSolutions(game.Letters[0], game.Letters, wordsQuery));
  foreach (var game in gamesWithWords)
  {
    var gameFilePath = Path.Combine(outputPath, $"game-{game.Key}.json");
    File.WriteAllText(gameFilePath, JsonSerializer.Serialize(game.Value));
  }
}


void DescribeGame() {
  var db = CreateDb();
  var letters = args[1].ToLower();
  var game = db.Games.SingleOrDefault(x => x.Letters == letters);

  if(game is null){
    Console.WriteLine("Could not find game");
    return;
  }

  var generator = new GameGenerator();
  var wordsQuery = db.Words.AsQueryable();
  var solutions = generator.findSolutions(game.Letters[0], game.Letters, wordsQuery);

  Console.WriteLine($"Game: {letters}");
  Console.WriteLine($"Total points: {game.TotalScore}");
  Console.WriteLine($"Words (count): {solutions.Count()}");
  Console.WriteLine($"Words:\n{string.Join(", ", solutions.Select(x => x.FullForm).OrderBy(x => x.Length))}");
}
// --------------

if(args.Count() == 1 && args[0].ToLower() == "debug"){
  Console.WriteLine("running debug command");

  // -------------------------

  var db = CreateDb();
  var generator = new GameGenerator();

  var wordsQuery = db.Words.AsQueryable();

  var game = generator.GenerateGame(wordsQuery);
  var solutions = generator.findSolutions(game.Letters[0], game.Letters, wordsQuery);

  Console.WriteLine("Actual solutions: {0}", string.Join(", ", solutions));

  // -------------------------

  return;
}

if(args.Count() == 2 && args[0].ToLower() == "add"){
  var gamesToCreate = int.Parse(args[1]);
  AddGames(gamesToCreate);
  return;
}

if(args.Count() == 2 && args[0].ToLower() == "add-challenge"){
  var gamesToCreate = int.Parse(args[1]);
  AddGamesChallenge(gamesToCreate);
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

if(args.Count() == 2 && args[0].ToLower() == "describe"){
  DescribeGame();
  return;
}

// No valid action
Console.WriteLine("Please specify what you want done.");

