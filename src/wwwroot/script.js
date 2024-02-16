console.log("hello mom");

document.addEventListener("keydown", (event) => {
  event.stopPropagation();

  if (event.key !== "Enter") {
    return;
  }

  const guess = document.getElementById("btn-guess").value;
  makeGuess(guess).then(() => {
    document.getElementById("btn-guess").value = "";
  });
});

let centerLetter, letters, totalScore;
let currentScore = 0;
let foundWords = [];
let isLoading = false;
let messageTimeoutId = undefined;

const shuffle = (arr) => {
  return arr
    .map(x => ({ value: x, sort: Math.random() }))
    .sort((a, b) => a.sort - b.sort)
    .map(({ value }) => value);
};

const setHive = (centerLetter, letters) => {
  const letterOrder = centerLetter + shuffle(letters.split("").filter(x => x !== centerLetter)).join("")
  document.querySelectorAll('.hive-cell text').forEach((elem, i) => {
    elem.textContent = letterOrder[i];
  })
}

const setInputPattern = (letters) => {
  document.getElementById("btn-guess").setAttribute("pattern", `[${letters}]*`);
}

const setScore = () => {
  document.getElementById("current-score").textContent = currentScore;
  document.getElementById("total-score").textContent = totalScore;
}

const calculateWordScore = (word) => {
  if (word.length == 4) return 1;
  return word.length + (new Set(word.split(""))).size ? 7 : 0;
}

const addToScore = (word) => {
  currentScore += calculateWordScore(word);
  setScore();
}

const startUp = async () => {
  const game = await fetch("/api").then(response => response.json());

  letters = game.letters;
  centerLetter = game.letters[0];
  totalScore = game.totalScore;

  setHive(centerLetter, letters);
  setInputPattern(letters);
  setScore(currentScore);
}

const drawFoundWords = () => {
  const wordsElem = document.getElementById("found-words");
  wordsElem.innerHTML = "";

  foundWords.forEach((word) => {
    let li = document.createElement("li");
    li.textContent = word;
    wordsElem.append(li);
  })

  document.getElementById("found-word-count").textContent = foundWords.length;
};

const wrongGuessMap = {
  FAKE_ID: "Noget er gået galt, prøv at genstarte siden.",
  INVALID_GUESS: "Ugyldigt gæt, kun de viste bogstaver må bruges og det gule bogstav SKAL bruges",
  WORD_DOES_NOT_EXIST: "Ordet findes ikke",
  ALLREADY_GUESSED: "Allerede gættet",
  CORRECT: "Korrekt!",
  [undefined]: "Lortet er gået galt af en eller anden grund...",
}

const displayMessage = (messageEnum, type) => {
  const message = wrongGuessMap[messageEnum];

  if (messageTimeoutId) {
    clearTimeout(messageTimeoutId);
  }

  const messageElem = document.getElementById("message");

  messageElem.className = type;
  messageElem.textContent = message;

  messageTimeoutId = setTimeout(() => {
    messageElem.className = "";
    messageTimeoutId = undefined;
  }, 1500);
};

const makeGuess = async (guess) => {
  if (!RegExp(`^[${letters}]{4,}$`).test(guess) || !guess.includes(centerLetter)) {
    displayMessage("INVALID_GUESS", "info")
    return;
  }

  if (foundWords.includes(guess)) {
    displayMessage("ALLREADY_GUESSED", "info")
    return;
  }

  if (isLoading) {
    return;
  }
  isLoading = true;
  const { success, reason } = await fetch(`/api/${letters}?guess=${guess}`).then((response) => response.json())
  isLoading = false;

  if (!success) {
    displayMessage(reason, "error");
    return;
  }

  displayMessage("CORRECT", "success");

  foundWords.unshift(guess);
  drawFoundWords();
  addToScore(guess)
}

startUp();