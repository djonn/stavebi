using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using StaveBi.Database;

namespace StaveBi.Route;

public static class Api
{
  public static void Map(RouteGroupBuilder route)
  {
    route.MapGet("/", ([FromServices] GameContext context) =>
      {
        var rnd = new Random();
        return context.Games.Skip(rnd.Next(context.Games.Count())).First();
      }
    );

    route.MapGet("/{id}", ([FromServices] GameContext context, [FromRoute] string id, [FromQuery] string guess) =>
    {
      if (!validateId(id)) return false;
      if (!validateGuess(id, guess)) return false;

      return context.Words.Any(x => x.Value == guess);
    });
  }

  private static bool validateId(string id)
  {
    if (id.Length != 7) return false;
    if (new HashSet<char>(id).Count() != 7) return false;
    if (Regex.IsMatch(id, @"[^a-zæøå]")) return false;

    return true;
  }

  private static bool validateGuess(string id, string guess)
  {
    return guess.Contains(id.First()) && new HashSet<char>(guess).IsSubsetOf(new HashSet<char>(id));
  }
}