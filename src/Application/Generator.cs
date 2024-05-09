using System.Text.RegularExpressions;
using StaveBi.IO;
using StaveBi.Model;

namespace StaveBi.Application;

public class GameGenerator
{
  public string WordListPath { get; private set; }

  static readonly HashSet<string> bannedConjugations = new HashSet<string>()
  {
    "art",
    "flerord",
    "fork",
    "formsubj",
    "gen",
    "infmærke",
    "kardinal",
    "lydord",
    "nom",
    "ordinal",
    "præfiks",
    "prop",
    "sms",
    "suffiks",
    "symbol",
    "udråbsord",
    "iflerord",
  };

  public GameGenerator()
  {
    WordListPath = "full_wordlist.tsv";
  }

  public GameGenerator(string wordlistPath)
  {
    WordListPath = wordlistPath;
  }

  bool isValidWord(WordDetails word)
  {
    // Has to be in the Danish dictionary
    if (word.Standardized != true) return false;

    // Word must be minimum 4 letters
    if (word.FullForm.Length < 4) return false;

    // Word can at most have 7 unique letters
    if (word.FullForm.Distinct().Count() > 7) return false;

    // Word may only contain danish letters
    if (Regex.IsMatch(word.FullForm, @"[^a-zæøå]")) return false;

    // Not interested in certain conjugations
    if (bannedConjugations.Overlaps(word.Conjugation)) return false;

    return true;
  }

  bool isPangram(WordDetails word)
  {
    return word.FullForm.Distinct().Count() == 7;
  }

  string selectUniqueLetters(WordDetails word)
  {
    return string.Join("", word.FullForm.Distinct().Order().Select((x) => x.ToString()));
  }

  T pickRandom<T>(IEnumerable<T> list)
  {
    var rnd = new Random();
    return list.ElementAt(rnd.Next(list.Count()));
  }

  public IEnumerable<WordDetails> findSolutions(char centerLetter, string letters, IEnumerable<WordDetails> words)
  {
    var letterSet = new HashSet<char>(letters);
    var wordSets = words.Select(x => new { Word = x, Letters = new HashSet<char>(x.FullForm) });
    return wordSets.Where(x => x.Letters.Contains(centerLetter) && x.Letters.IsSubsetOf(letterSet)).Select(x => x.Word);
  }

  int calculateWordScore(WordDetails word)
  {
    if (word.FullForm.Length == 4) return 1;
    var pangramBonus = isPangram(word) ? 7 : 0;
    return word.FullForm.Length + pangramBonus;
  }

  public int calculateTotalScore(IEnumerable<WordDetails> words)
  {
    int points = 0;

    foreach (var word in words)
    {
      points += calculateWordScore(word);
    }

    return points;
  }

  public IEnumerable<WordDetails> ParseWords()
  {
    var unfilteredWords = FileIO.ReadTsvFile(WordListPath, (fields) =>
    {
      if (fields.Count() < 6) throw new Exception("Wrong input data");

      return new WordDetails()
      {
        Id = fields.ElementAt(0),
        Lemma = fields.ElementAt(1),
        Example = fields.ElementAt(2),
        Conjugation = new HashSet<string>(fields.ElementAt(3)!.Split(".")),
        FullForm = fields.ElementAt(4),
        Standardized = fields.ElementAt(5) == "1",
      };
    });
    if (unfilteredWords is null) throw new Exception("Ya dun goof'd");

    return unfilteredWords;
  }

  public IEnumerable<WordDetails> GetValidWords()
  {
    var unfilteredWords = ParseWords();
    var words = unfilteredWords.Where(isValidWord);

    if (words is null) throw new Exception("Something went wrong while finding valid words");

    return words;
  }

  public Game GenerateGame(IQueryable<WordDetails> words)
  {
    var pangramWords = words.Where(isPangram);
    var pangramGroups = pangramWords.GroupBy(selectUniqueLetters, x => x, (key, words) => new { Key = key, Value = words });
    var nonUniqueLetters = pangramGroups.Where((a) => a.Value.Count() != 1);
    var letterSets = pangramGroups.Select(x => x.Key);

    // rely on return to break the loop
    while (true)
    {
      var allLetters = pickRandom(letterSets);
      var centerLetter = pickRandom(allLetters);

      var solutions = findSolutions(centerLetter, allLetters, words);
      var totalScore = calculateTotalScore(solutions);

      if (solutions.Count() < 10 || solutions.Count() > 30)
      {
        Console.WriteLine("Invalid letters \"{0}\" has {1} solutions", allLetters, solutions.Count());
        continue;
      }

      if (totalScore > 120)
      {
        Console.WriteLine("Invalid letters \"{0}\" has a score of {1}", allLetters, totalScore);
        continue;
      }

      Console.WriteLine("Letters: {0} ({1}) - Solutions={2}, Points={3}", allLetters, centerLetter, solutions.Count(), totalScore);
      Console.WriteLine("Example solutions: {0}", string.Join(", ", solutions));

      var gameKey = centerLetter + allLetters.Replace(centerLetter.ToString(), "");

      return new Game()
      {
        Letters = gameKey,
        TotalScore = totalScore,
      };
    }
  }
}
