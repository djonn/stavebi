namespace StaveBi.Model;

public class Game
{
  public string Letters { get; set; }
  public char CenterLetter { get; set; }
  public IEnumerable<string> Solutions { get; set; }
  public int TotalScore { get; set; }
}