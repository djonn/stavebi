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

  bool isPangram(string word)
  {
    return word.Distinct().Count() == 7;
  }

  string selectUniqueLetters(string word)
  {
    return string.Join("", word.Distinct().Order().Select((x) => x.ToString()));
  }

  T pickRandom<T>(IEnumerable<T> list)
  {
    var rnd = new Random();
    return list.ElementAt(rnd.Next(list.Count()));
  }

  public IEnumerable<string> findSolutions(char centerLetter, string letters, IEnumerable<string> words)
  {
    var letterSet = new HashSet<char>(letters);
    var wordSets = words.Select(x => new { Word = x, Letters = new HashSet<char>(x) });
    return wordSets.Where(x => x.Letters.Contains(centerLetter) && x.Letters.IsSubsetOf(letterSet)).Select(x => x.Word);
  }

  int calculateWordScore(string word)
  {
    if (word.Length == 4) return 1;
    var pangramBonus = isPangram(word) ? 7 : 0;
    return word.Length + pangramBonus;
  }

  public int calculateTotalScore(IEnumerable<string> words)
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

  public IEnumerable<string> GetValidWords()
  {
    var unfilteredWords = ParseWords();
    // Console.WriteLine("total words: {0}", unfilteredWords.Count());

    var words = unfilteredWords.Where(isValidWord).Select(x => x.FullForm).Distinct();
    // Console.WriteLine("filtered words: {0}", words.Count());

    if (words is null) throw new Exception("Something went wrong while finding valid words");

    return words;
  }

  public Game GenerateGame(IQueryable<string> words)
  {
    var pangramWords = words.Where(isPangram);
    // Console.WriteLine("pangrams: {0}", pangramWords.Count());

    var pangramGroups = pangramWords.GroupBy(selectUniqueLetters, x => x, (key, words) => new { Key = key, Value = words });
    var nonUniqueLetters = pangramGroups.Where((a) => a.Value.Count() != 1);
    var letterSets = pangramGroups.Select(x => x.Key);

    // rely on return to break the loop
    while (true)
    {
      var letters = pickRandom(letterSets);
      var centerLetter = pickRandom(letters);
      var solutions = findSolutions(centerLetter, letters, words);

      // Letters must have between 10 and 50 solution words
      if (solutions.Count() < 10 || solutions.Count() > 50)
      {
        Console.WriteLine("Invalid letters \"{0}\" has {1} solutions", letters, solutions.Count());
        continue;
      }

      var totalScore = calculateTotalScore(solutions);

      Console.WriteLine("Letters: {0} ({1}) - Solutions={2}, Points={3}", letters, centerLetter, solutions.Count(), totalScore);
      Console.WriteLine("Example solutions: {0}", string.Join(", ", solutions));

      return new Game()
      {
        Letters = letters,
        TotalScore = totalScore,
      };
    }
  }
}
