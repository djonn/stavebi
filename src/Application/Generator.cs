using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using StaveBi.IO;
using StaveBi.Model;

namespace StaveBi.Application;

public class GameGenerator
{
  public string WordListPath { get; private set; }
  public string WordFreqListPath { get; private set; }

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
    WordFreqListPath = "freq-30k-ex.txt";
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

  public IEnumerable<LemmaFrequency> ParseFequencies()
  {
    var freq = FileIO.ReadTsvFile(WordFreqListPath, (fields) =>
    {
      if (fields.Count() < 3) throw new Exception("Wrong input data");

      return new LemmaFrequency()
      {
        Tag = fields.ElementAt(0),
        Lemma = fields.ElementAt(1),
        Frequency = double.Parse(fields.ElementAt(2), NumberStyles.Any, CultureInfo.InvariantCulture),
      };
    });
    if (freq is null) throw new Exception("Ya dun goof'd");

    return freq;
  }

  public void AddFrequencies(List<WordDetails> words, List<LemmaFrequency> lemmaFrequencies){
    var wordsByLemma = words.GroupBy(x => x.Lemma).ToDictionary(g => g.Key, g => g.ToList());
    var existingLemmas = wordsByLemma.Select(x => x.Key).ToHashSet();
    var existingLemmaFrequencies = lemmaFrequencies.Where(x => existingLemmas.Contains(x.Lemma)).ToList();

    foreach(var lemmaFrequency in existingLemmaFrequencies) {
      var lemmaGroup = wordsByLemma[lemmaFrequency.Lemma];
      Console.WriteLine("Lemma: {0} - {1}", lemmaFrequency.Lemma, lemmaGroup.Count());
      foreach(var word in lemmaGroup) {
        word.LemmaFrequency = lemmaFrequency.Frequency;
      }
    }
  }

  public IEnumerable<WordDetails> GetValidWords()
  {
    Console.WriteLine("Parsing wordlist");
    var unfilteredWords = ParseWords();

    Console.WriteLine("Filtering out invalid words");
    var words = unfilteredWords.Where(isValidWord).ToList();

    Console.WriteLine("Parsing word frequency list");
    var lemmaFrequencies = ParseFequencies().ToList();

    Console.WriteLine("Adding frequencies to words");
    AddFrequencies(words, lemmaFrequencies);

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

}
