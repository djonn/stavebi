using Microsoft.AspNetCore.Mvc;

namespace StaveBi.Route;

public static class Api
{
  public static void Map(RouteGroupBuilder route)
  {
    route.MapGet("/", () =>
      {
        // TODO: Get from a GameService
        return new { letters = "abcdef", centerLetter = 'a', total = 0 };
      }
    );

    route.MapPost("/{id}", ([FromRoute] string id, [FromBody] string guess) =>
    {
      // TODO: Check some game service
      return true;
    });
  }
}