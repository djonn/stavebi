import { sum } from "./util";

export class Game {
  private static readonly SEPARATOR = ";";

  private _letters: string;
  private _totalPoints: number;
  private _guessedWords: string[];
  private _currentPoints: number;

  public get letters () {return this._letters};
  public get totalPoints () {return this._totalPoints};
  public get guessedWords () {return this._guessedWords};
  public get currentPoints () {return this._currentPoints};
  
  constructor(letters: string, totalPoints: number, guessedWords: string[] = []){
    this._letters = letters;
    this._totalPoints = totalPoints;
    this._guessedWords = guessedWords;
    this._currentPoints = guessedWords.map(Game.calculateWordScore).reduce(sum, 0)
  }

  private static calculateWordScore = (word: string) => {
    if (word.length == 4) return 1;
    const bonus = (new Set(word.split(""))).size === 7 ? 7 : 0
    return word.length + bonus;
  }

  public static listGames(): string[] {
    return localStorage.getItem("game/ids")?.split(Game.SEPARATOR) ?? [];
  }

  public static load(letters: string): Game {
    const totalPoints = parseInt(localStorage.getItem(`game/${letters}/totalPoints`)!, 10);
    const guessedWords = localStorage.getItem(`game/${letters}/guessedWords`)?.split(Game.SEPARATOR);

    if(Number.isNaN(totalPoints) || guessedWords === null){
      throw new Error("Game could not be loaded");
    }

    return new Game(letters, totalPoints, guessedWords);
  }

  public save(): void {
    localStorage.setItem(`game/${this._letters}/totalPoints`, this._totalPoints.toString())
    localStorage.setItem(`game/${this._letters}/guessedWords`, this._guessedWords.join(Game.SEPARATOR))

    const existingIds = localStorage.getItem("game/ids")?.split(Game.SEPARATOR);
    if(existingIds == null){
      localStorage.setItem("game/ids", this._letters);
    } else if(!existingIds.includes(this._letters)) {
      localStorage.setItem("game/ids", [...existingIds, this._letters].join(Game.SEPARATOR))
    }
  }

  public addGuessedWord(word: string): void {
    this._guessedWords.push(word);
    this._currentPoints += Game.calculateWordScore(word);
  }

  public validate(guess: string){
    const firstLetter = this._letters[0];
    if (!firstLetter) throw new Error("Why no letters?");
    return !RegExp(`^[${this._letters}]{4,}$`).test(guess) || !guess.includes(firstLetter)
  }
}