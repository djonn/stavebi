import { Api } from "./api";
import { Game } from "./game";
import { clearGuess, getGuess, setHive, setScore, setInputPattern, drawFoundWords, Toast, Message, parseMessage, keyEvent } from "./ui";

let game: Game;
let isLoading = false;

const startUp = async () => {
  const lastGame = Game.listGames().at(-1);
  if (lastGame) {
    game = Game.load(lastGame);
  } else {
    let newGame;
    const previousGames = Game.listGames();

    do {
      newGame = await Api.newGame();
    } while (previousGames.includes(newGame.letters));
    
    game = newGame;
    game.save();
  }

  setHive(game.letters);
  setInputPattern(game.letters);
  setScore(game);
  drawFoundWords(game);

  keyEvent("Enter", () => {
    const guess = getGuess();
    makeGuess(game, guess).then(() => clearGuess());
  });
}

const makeGuess = async (game: Game, guess: string) => {
  if (game.validate(guess)) {
    Toast.displayMessage(Message.INVALID_GUESS, "info")
    return;
  }

  if (game.guessedWords.includes(guess)) {
    Toast.displayMessage(Message.ALREADY_GUESSED, "info")
    return;
  }

  if (isLoading) {
    return;
  }

  isLoading = true;
  const result = await Api.guess(game, guess);
  isLoading = false;

  if (!result.success) {
    const message = parseMessage(result.reason);
    Toast.displayMessage(message, "error");
    return;
  }

  Toast.displayMessage(Message.CORRECT, "success");

  game.addGuessedWord(guess);
  game.save();
  setScore(game);
  drawFoundWords(game);
}

startUp();
