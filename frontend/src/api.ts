import { Game } from "./game";

export type GuessResponse =
  | { success: true }
  | { success: false, reason: string}

export class Api {

  private static cache: Record<string, any> = {};

  private static async cachedFetch(url: string): Promise<any> {
    if(Api.cache[url]) return Api.cache[url];

    const response = await fetch(url)
      .then(response => response.json());

    Api.cache[url] = response;
    return response;
  }

  public static async newGame(): Promise<Game> {
    const gamesList = await Api.cachedFetch("/api/games.json");
    
    const i = Math.floor(Math.random()*gamesList.length);
    const gameDto = gamesList[i];
    return new Game(gameDto.Letters, gameDto.TotalScore);
  }

  public static async guess(game: Game, guess: string): Promise<GuessResponse> {
    const solutions = await Api.cachedFetch(`/api/game-${game.letters}.json`);
    return solutions.includes(guess)
        ? { success: true }
        : { success: false, reason: "WORD_DOES_NOT_EXIST" };
  }
}
