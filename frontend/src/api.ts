import { Game } from "./game";
import { daysBetween } from "./util";

export type GuessResponse =
  | { success: true }
  | { success: false; reason: string };

export class Api {
  private static cache: Record<string, any> = {};

  private static async cachedFetch(url: string): Promise<any> {
    if (Api.cache[url]) return Api.cache[url];

    const response = await fetch(`${document.location.href}/${url}`).then(
      (response) => response.json()
    );

    Api.cache[url] = response;
    return response;
  }

  public static async newGame(): Promise<Game> {
    const gamesList = await Api.cachedFetch("/api/games.json");

    const i = Math.floor(Math.random() * gamesList.length);
    const gameDto = gamesList[i];
    return new Game(gameDto.Letters, gameDto.TotalScore);
  }

  public static async daily(): Promise<Game> {
    const gamesList = await Api.cachedFetch("/api/games.json");

    const genesis = 1715724000000; // May 15 2024 00:00 GMT+0200
    const dayNumber = daysBetween(new Date(genesis), new Date());

    const i = dayNumber % gamesList.length;
    const gameDto = gamesList[i];

    try {
      return Game.load(gameDto.Letters);
    } catch (e) {
      return new Game(gameDto.Letters, gameDto.TotalScore);
    }
  }

  public static async guess(game: Game, guess: string): Promise<GuessResponse> {
    const solutions = await Api.cachedFetch(`/api/game-${game.letters}.json`);
    return solutions.includes(guess)
      ? { success: true }
      : { success: false, reason: "WORD_DOES_NOT_EXIST" };
  }
}
