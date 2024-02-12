using Microsoft.AspNetCore.Mvc;
using StaveBi.Application;
using StaveBi.Database;
using StaveBi.Model;

namespace StaveBi.Route;

public static class Admin
{
  public static void Map(RouteGroupBuilder route)
  {
    route.MapGet("/seed-words", ([FromServices] GameContext context, [FromServices] GameGenerator generator) =>
      {
        var words = generator.GetValidWords().Select(x => new Word(x));
        context.Words.AddRange(words);
        context.SaveChanges();
      }
    );

    route.MapGet("/generate", ([FromServices] GameContext context, [FromServices] GameGenerator generator) =>
      {
        var newGame = generator.GenerateGame(context.Words.Select(x => x.Value).AsQueryable());
        context.Games.Add(newGame);
        context.SaveChanges();

        return newGame;
      }
    );
  }
}