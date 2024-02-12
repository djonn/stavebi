using System.IO;
using System.Text.Json;

public static class FileIO
{
  public static T ReadJsonFile<T>(string path)
  {
    var content = File.ReadAllText(path);

    var options = new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true
    };
    return JsonSerializer.Deserialize<T>(content, options);
  }

  public static IEnumerable<T> ReadTsvFile<T>(string path, Func<IEnumerable<string>, T> rowSelector)
  {
    return File.ReadAllText(path).Trim().Split("\n").Select((row) => rowSelector(row.Split("\t")));
  }
}
