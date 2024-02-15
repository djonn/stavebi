using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StaveBi.Application;
using StaveBi.Database;

namespace StaveBi.Route;

public static class Debug
{
  public static void Map(RouteGroupBuilder route)
  {
    route.MapGet("/find-word", ([FromServices] GameContext context, [FromQuery] string word) =>
    {
      return context.Words.Any(x => x.Value == word);
    });

    route.MapGet("/add-word", ([FromServices] GameContext context, [FromQuery] string word) =>
    {
      context.Words.Add(new Model.Word(word));
      context.SaveChanges();
    });

    route.MapGet("/game-count", ([FromServices] GameContext context) =>
    {
      return context.Games.Count();
    });

    route.MapGet("/find-solutions", ([FromServices] GameContext context, [FromQuery] string id) =>
    {
      var firstLetter = id.Substring(0, 1);
      return context.Words
        .Where(w => w.Value.Contains(firstLetter) && Regex.IsMatch(w.Value, $"^[{id}]+$"))
        .Select(w => w.Value);
    });

    route.MapGet("/words-by-type", ([FromServices] GameGenerator generator, [FromQuery] string conjugation, [FromQuery] int count, [FromQuery] int offset) =>
    {
      var unfilteredWords = generator.ParseWords();

      return unfilteredWords/*.Where(isValidWord)*/.Where(x => x.Conjugation.Contains(conjugation)).Select(x => $"{x.FullForm} - {string.Join(".", x.Conjugation)} - {(x.Standardized ? "✅" : "❌")}").Distinct().Skip(offset).Take(count);
    });

    route.MapGet("/non-standardized", ([FromServices] GameGenerator generator) =>
    {
      var unfilteredWords = generator.ParseWords();
      return unfilteredWords.Any(x => !x.Standardized);
    });

    route.MapGet("/migration", ([FromServices] GameContext context) =>
    {
      return new Dictionary<string, IEnumerable<string>>()
      {
        {"all", context.Database.GetMigrations()},
        {"applied", context.Database.GetAppliedMigrations()}
      };
    });
  }
}