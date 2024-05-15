import { Api } from "./api";
import { Game } from "./game";
import {
  clearGuess,
  getGuess,
  setHive,
  setScore,
  setInputPattern,
  drawFoundWords,
  drawGameSelector,
  Toast,
  Message,
  parseMessage,
  keyEvent,
  clickEvent,
  ID_BUTTON_SHUFFLE,
  ID_BUTTON_SUBMIT,
  ID_BUTTON_NEW_GAME,
  selectEvent,
  ID_GAME_SELECTOR,
  ID_BUTTON_SPLASH_CONTINUE,
  ID_BUTTON_SPLASH_NEW_GAME,
  ID_BUTTON_SPLASH_DAILY,
  hideSplash,
} from "./ui";

let game: Game;
let isLoading = false;

const splahStartUp = async () => {
  clickEvent(ID_BUTTON_SPLASH_CONTINUE, () => {
    if(isLoading) return;
    isLoading = true;
    startUp("CONTINUE");
    isLoading = false;
  });
  clickEvent(ID_BUTTON_SPLASH_NEW_GAME, () => {
    if(isLoading) return;
    isLoading = true;
    startUp("NEWGAME");
    isLoading = false;
  });
  clickEvent(ID_BUTTON_SPLASH_DAILY, () => {
    if(isLoading) return;
    isLoading = true;
    startUp("DAILY");
    isLoading = false;
  });
};

const startUp = async (startUpStrategy: string) => {
  hideSplash();

  if(startUpStrategy == "CONTINUE"){
    const lastGameId = Game.getLatestGame();
    if (lastGameId) {
      game = Game.load(lastGameId);
    } else {
      startUpStrategy = "NEWGAME"
    }
  }

  if(startUpStrategy == "NEWGAME"){
    const newGame = await Api.newGame();
    game = newGame;
    game.save();
  }

  if(startUpStrategy == "DAILY"){
    const dailyGame = await Api.daily();
    game = dailyGame;

    if(!Game.listGames().includes(game.letters)){
      game.save();
    }
  }

  if(!game){
    alert("Noget gik galt..")
    throw new Error("No game found!");
  }


  const previousGames = Game.listGames().map((id) => Game.load(id));

  setHive(game.letters);
  setInputPattern(game.letters);
  setScore(game);
  drawGameSelector(previousGames, game);
  drawFoundWords(game);

  keyEvent("Enter", () => {
    const guess = getGuess();
    makeGuess(game, guess).then(() => clearGuess());
  });

  clickEvent(ID_BUTTON_SUBMIT, () => {
    const guess = getGuess();
    makeGuess(game, guess).then(() => clearGuess());
  });

  clickEvent(ID_BUTTON_SHUFFLE, () => {
    setHive(game.letters);
  });

  clickEvent(ID_BUTTON_NEW_GAME, async () => {
    let newGame;
    const previousGameIds = Game.listGames();

    do {
      newGame = await Api.newGame();
    } while (previousGameIds.includes(newGame.letters));

    game = newGame;
    game.save();

    const previousGames = Game.listGames().map((id) => Game.load(id));

    setHive(game.letters);
    setInputPattern(game.letters);
    setScore(game);
    drawGameSelector(previousGames, game);
    drawFoundWords(game);
  });

  selectEvent(ID_GAME_SELECTOR, (gameId: string) => {
    game = Game.load(gameId);
    game.setLatestGame();

    const previousGames = Game.listGames().map((id) => Game.load(id));

    setHive(game.letters);
    setInputPattern(game.letters);
    setScore(game);
    drawGameSelector(previousGames, game);
    drawFoundWords(game);
  });
};

const makeGuess = async (game: Game, guess: string) => {
  if (game.validate(guess)) {
    Toast.displayMessage(Message.INVALID_GUESS, "info");
    return;
  }

  if (game.guessedWords.includes(guess)) {
    Toast.displayMessage(Message.ALREADY_GUESSED, "info");
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
};

splahStartUp();
