using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.RegularExpressions;
using StaveBi.IO;
using StaveBi.Model;

namespace StaveBi.Application;

public class GameGenerator
{
  public string WordListPath { get; private set; }

  static readonly HashSet<string> bannedConjugations = new HashSet<string>()
  {
    "art",       // artikel
    "flerord",   // flerordsforbindelse
    "fork",      // forkortelse
    "formsubj",  // formelt subjekt
    "gen",       // genitiv
    "infmærke",  // infinitivmærke
    "kardinal",  // kardinalform
    "lydord",    // lydord
    "nom",       // nominal
    "ordinal",   // ordinalform
    "præfiks",   // præfiks
    "prop",      // proprium
    "sms",       // sammensætningsform
    "suffiks",   // suffiks
    "symbol",    // symbol
    "udråbsord", // udråbsord
    "iflerord",  // del af flerordsforbindelse
  };

  public GameGenerator()
  {
    WordListPath = "full_wordlist.tsv";
  }

  public GameGenerator(string wordlistPath)
  {
    WordListPath = wordlistPath;
  }

  public bool isValidWord(WordDetails word)
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

  public IEnumerable<WordDetails> findSolutions(char centerLetter, string letters, IEnumerable<WordDetails> words)
  {
    var letterSet = new HashSet<char>(letters);
    var wordSets = words.Select(x => new { Word = x, Letters = new HashSet<char>(x.FullForm) });
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

  public IEnumerable<WordDetails> GetValidWords()
  {
    var unfilteredWords = ParseWords();
    var words = unfilteredWords.Where(isValidWord);

    if (words is null) throw new Exception("Something went wrong while finding valid words");

    return words;
  }

  public Game GenerateGame(IQueryable<WordDetails> words)
  {
    var pangramWords = words.Where(x => isPangram(x.FullForm));
    var pangramGroups = pangramWords.GroupBy((x) => selectUniqueLetters(x.FullForm), x => x, (key, words) => new { Key = key, Value = words });
    var nonUniqueLetters = pangramGroups.Where((a) => a.Value.Count() != 1);
    var letterSets = pangramGroups.Select(x => x.Key);

    // rely on return to break the loop
    while (true)
    {
      var allLetters = pickRandom(letterSets);
      var centerLetter = pickRandom(allLetters);

      var letters = centerLetter + allLetters.Replace(centerLetter.ToString(), "");
      var game = ValidateGame(letters, words);

      if (game is not null) return game;
    }
  }

  public Game? ValidateGame(string letters, IQueryable<WordDetails> words)
  {
    if (letters.Length != 7 || selectUniqueLetters(letters).Length != 7)
    {
      Console.WriteLine("Invalid letters \"{0}\" is not a set for 7 unique characters", letters);
      return null;
    }

    var allSolutions = findSolutions(letters[0], letters, words);

    var uniqueSolutions = allSolutions.Select(x => x.FullForm).Distinct();
    var totalScore = calculateTotalScore(uniqueSolutions);
    var baseWordCount = allSolutions.Select(x => x.Lemma).Distinct().Count();

    if (uniqueSolutions.Count() < 10 || uniqueSolutions.Count() > 50)
    {
      Console.WriteLine("Invalid letters \"{0}\" has {1} solutions", letters, uniqueSolutions.Count());
      return null;
    }

    if (baseWordCount < 5)
    {
      Console.WriteLine("Invalid letters \"{0}\" too few unique base words / lemmas", letters, baseWordCount);
      return null;
    }

    if (totalScore > 120)
    {
      Console.WriteLine("Invalid letters \"{0}\" has a score of {1}", letters, totalScore);
      return null;
    }

    Console.WriteLine("Letters: {0} ({1}) - Solutions={2}, Points={3}", letters, letters[0], allSolutions.Count(), totalScore);
    Console.WriteLine("Example solutions: {0}", string.Join(", ", allSolutions));

    return new Game()
    {
      Letters = letters,
      TotalScore = totalScore,
    };
  }

  /// <summary>
  /// This finds the "hardest" game by the following definition that the hardest game
  ///
  /// - of the possible pangram solutions to the games' letterset the shortest pangram defines the difficulty
  /// - higher word length is harder game
  /// - any non-pangram word is ignored
  /// - any longer pangrams are ignored
  ///
  /// </summary>
  public void FindHardest(IEnumerable<Game> games, IQueryable<WordDetails> words)
  {
    var safeWords = words.ToList().ToImmutableList();

    var source = games.ToArray();

    var partitioner = Partitioner.Create(0, source.Length);

    Console.WriteLine("starting loop");

    var hardestPangramFoundLength = 7;
    var hardestPangramFound = "0";
    var hardestGameFound = "";

    var watch = Stopwatch.StartNew();

    Parallel.ForEach(partitioner, (range, state) => {
      for (int i = range.Item1; i < range.Item2; i++)
      {
        if((i-range.Item1) % 50 == 1){
          var current = i - range.Item1;
          var count = range.Item2 - range.Item1;
          var progress = Math.Round(current*1.0/count*100);
          Console.WriteLine($"[{range.Item1}-{range.Item2}]: {current} ({progress}%)");
        }

        var game = source[i];
        var shortestPangram = findSolutions(game.Letters[0], game.Letters, safeWords)
              .Select(x => x.FullForm)
              .Where(isPangram)
              .OrderBy(x => x.Length).First();

        if(shortestPangram.Length >= hardestPangramFoundLength){
          lock(this)
          {
            if(shortestPangram.Length > hardestPangramFoundLength){
              hardestPangramFoundLength = shortestPangram.Length;
              hardestPangramFound = shortestPangram;
              hardestGameFound = game.Letters;

              Console.WriteLine($"New hardest found with shortest pangram having length\nPangram with length {shortestPangram.Length} was: {shortestPangram}\nGame was: {game.Letters}");
            }
          }
        }
      }
    });
    watch.Stop();
    Console.WriteLine($"loop finished in {watch.Elapsed}");
  }

}
