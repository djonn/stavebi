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

  T pickRandom<T>(IEnumerable<T> list, int count)
  {
    var rnd = new Random();
    return list.ElementAt(rnd.Next(count));
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

  public IEnumerable<Game> GenerateGame(IQueryable<WordDetails> words)
  {
    var pangramWords = words.Select(x => x.FullForm).AsEnumerable().Where(x => isPangram(x));
    var pangramGroups = pangramWords.GroupBy((x) => selectUniqueLetters(x), x => x, (key, words) => new { Key = key, Value = words });
    var nonUniqueLetters = pangramGroups.Where((a) => a.Value.Count() != 1);
    var letterSets = pangramGroups.Select(x => x.Key);

    var safeWords = words.ToList().ToImmutableList();

    // rely on return to break the loop
    var letterSetsCount = letterSets.Count();
    Console.WriteLine($"{letterSetsCount} pangram groups found!");

    var source = letterSets
            .Take(1500)
            .ToArray();
    var result = new ConcurrentBag<Game>();

    var partitioner = Partitioner.Create(0, source.Length);

    Console.WriteLine("starting loop");

    var watch = Stopwatch.StartNew();
    Parallel.ForEach(partitioner, (range, state) => {
      for (int i = range.Item1; i < range.Item2; i++)
      {
        var allLetters = source[i];
        for(int j = 0; j < 7; j++)
        {
          var letters = allLetters[j] + allLetters.Remove(j,1);
          var game = ValidateGame(letters, safeWords);

          if(game is not null)
          {
            result.Add(game);
            Console.WriteLine($"Found game - {game.Letters} ({game.TotalScore})");
          }
        }
      }
    });
    watch.Stop();
    Console.WriteLine($"loop finished in {watch.Elapsed}");

    var resultCount = result.Count();

    Console.WriteLine($"{resultCount} valid games found!!!");

    return result.ToArray();
  }

  public Game? ValidateGame(string letters, IEnumerable<WordDetails> words)
  {
    if (letters.Length != 7 || selectUniqueLetters(letters).Length != 7)
    {
      // Console.WriteLine("Invalid letters \"{0}\" is not a set for 7 unique characters", letters);
      return null;
    }

    var allSolutions = findSolutions(letters[0], letters, words);

    var uniqueSolutions = allSolutions.Select(x => x.FullForm).Distinct();
    var totalScore = calculateTotalScore(uniqueSolutions);
    var baseWordCount = allSolutions.Select(x => x.Lemma).Distinct().Count();

    if (uniqueSolutions.Count() < 10 || uniqueSolutions.Count() > 50)
    {
      // Console.WriteLine("Invalid letters \"{0}\" has {1} solutions", letters, uniqueSolutions.Count());
      return null;
    }

    if (baseWordCount < 5)
    {
      // Console.WriteLine("Invalid letters \"{0}\" too few unique base words / lemmas", letters, baseWordCount);
      return null;
    }

    if (totalScore > 120)
    {
      // Console.WriteLine("Invalid letters \"{0}\" has a score of {1}", letters, totalScore);
      return null;
    }

    // Console.WriteLine("Letters: {0} ({1}) - Solutions={2}, Points={3}", letters, letters[0], allSolutions.Count(), totalScore);
    // Console.WriteLine("Example solutions: {0}", string.Join(", ", allSolutions));

    return new Game()
    {
      Letters = letters,
      TotalScore = totalScore,
    };
  }

}
