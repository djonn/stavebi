import { Game } from "./game";

export type GuessResponse =
  | { success: true }
  | { success: false, reason: string}

export class Api {
  public static async newGame(): Promise<Game> {
    const gameDto = await fetch("/api").then(response => response.json());
    return new Game(gameDto.letters, gameDto.totalPoints);
  }

  public static async guess(game: Game, guess: string): Promise<GuessResponse> {
    return await fetch(`/api/${game.letters}?guess=${guess}`).then((response) => response.json())
  }
}
