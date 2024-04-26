import { Game } from "./game";
import { shuffle } from "./util";

const ID_INPUT_GUESS = "input-guess";
const ID_SCORE_CURRENT = "current-score";
const ID_SCORE_TOTAL = "total-score";
const ID_LIST_WORDS = "found-words";
const ID_GAME_SELECTOR = "selected-game";
const SELECTOR_HIVE_TEXTS = ".hive-cell text";

export const setHive = (letters: string) => {
  const letterOrder = letters[0] + shuffle(letters.split("").filter(x => x !== letters[0])).join("")
  document.querySelectorAll(SELECTOR_HIVE_TEXTS).forEach((elem, i) => {
    elem.textContent = letterOrder[i] ?? "";
  })
}

export const getGuess = (): string => (<HTMLInputElement | null>document.getElementById(ID_INPUT_GUESS))?.value ?? "";
export const clearGuess = (): void => {
  const input = (<HTMLInputElement | null>document.getElementById(ID_INPUT_GUESS));
  if(input) input.value = "";
};

export const setInputPattern = (letters: string): void => {
  (<HTMLInputElement | null>document.getElementById(ID_INPUT_GUESS))?.setAttribute("pattern", `[${letters}]*`);
}

export const setScore = (game: Game): void => {
  const currentScoreElem = document.getElementById(ID_SCORE_CURRENT)
  if (currentScoreElem?.textContent) currentScoreElem.textContent = game.currentPoints.toString();
  
  const totalScoreElem = document.getElementById(ID_SCORE_TOTAL)
  if (totalScoreElem?.textContent) totalScoreElem.textContent = game.totalPoints.toString();
}

export const drawGameSelector = (games: Game[]) => {
  const gameSelectorElem = document.getElementById(ID_GAME_SELECTOR);
  if(!gameSelectorElem) return;

  gameSelectorElem.innerHTML = "";

  games.forEach((game) => {
    let option = document.createElement("li");
    option.textContent = `${game.letters} (${game.currentPoints}/${game.totalPoints})`;
    option.value = game.letters;
    gameSelectorElem.append(option);
  });


  // <option value="fiklort">fiklort (6/176)</option>

};

export const drawFoundWords = (game: Game) => {
  const wordsElem = document.getElementById(ID_LIST_WORDS);
  const wordCountElem = document.getElementById("found-word-count");
  if(!wordsElem || !wordCountElem) return;

  wordsElem.innerHTML = "";

  game.guessedWords.forEach((word) => {
    let li = document.createElement("li");
    li.textContent = word;
    wordsElem.append(li);
  })

  wordCountElem.textContent = game.guessedWords.length.toString();
};

export const keyEvent = (key: string, callback: () => void) => {
  document.addEventListener("keydown", (event) => {
    event.stopPropagation();
  
    if (event.key == key) {
      callback();
    }
  });
}

export enum Message {
  FAKE_ID = "Noget er gået galt, prøv at genstarte siden.",
  INVALID_GUESS = "Ugyldigt gæt, kun de viste bogstaver må bruges og det gule bogstav SKAL bruges",
  WORD_DOES_NOT_EXIST = "Ordet findes ikke",
  ALREADY_GUESSED = "Allerede gættet",
  CORRECT = "Korrekt!",
  UNKNOWN = "Noget er gået galt"
}

export type ToastType = "info" | "success" | "error"

export const parseMessage = (type: string): Message => {
  switch(type){
    case "FAKE_ID":
      return Message.FAKE_ID
    case "INVALID_GUESS":
      return Message.INVALID_GUESS;
    case "WORD_DOES_NOT_EXIST":
      return Message.WORD_DOES_NOT_EXIST;
    default:
      return Message.UNKNOWN;
  }
}

export class Toast {
  private static messageTimeoutId: number | undefined;

  public static displayMessage(message: Message, type: ToastType): void {
    if (Toast.messageTimeoutId) {
      clearTimeout(Toast.messageTimeoutId);
    }

    const messageElem = document.getElementById("message");
    if (!messageElem) return;

    messageElem.className = type;
    messageElem.textContent = message;

    Toast.messageTimeoutId = setTimeout(() => {
      messageElem.className = "";
      Toast.messageTimeoutId = undefined;
    }, 1500);
  };
}