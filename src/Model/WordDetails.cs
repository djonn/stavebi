namespace StaveBi.Model;

public class WordDetails
{
  public string Id { get; set; }
  public string Lemma { get; set; }
  public string Example { get; set; }
  public HashSet<string> Conjugation { get; set; }
  public string FullForm { get; set; }
  public bool Standardized { get; set; }

  public double? LemmaFrequency { get; set; }
}
