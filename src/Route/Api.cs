using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using StaveBi.Database;

namespace StaveBi.Route;

enum WrongGuessReason
{
  FAKE_ID,
  INVALID_GUESS,
  WORD_DOES_NOT_EXIST
}

class GuessResponse
{
  public bool Success { get; set; }
  public WrongGuessReason Reason { get; set; }

  public static GuessResponse CreateSuccess()
  {
    return new GuessResponse() { Success = true };
  }

  public static GuessResponse CreateFailure(WrongGuessReason reason)
  {
    return new GuessResponse() { Success = false, Reason = reason };
  }
}

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
      if (!validateId(id)) return GuessResponse.CreateFailure(WrongGuessReason.FAKE_ID);
      if (!validateGuess(id, guess)) return GuessResponse.CreateFailure(WrongGuessReason.INVALID_GUESS);
      if (!context.Words.Any(x => x.Value == guess)) return GuessResponse.CreateFailure(WrongGuessReason.WORD_DOES_NOT_EXIST);

      return GuessResponse.CreateSuccess();
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