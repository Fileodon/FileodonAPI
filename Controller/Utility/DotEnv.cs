namespace Controller.Utility;

/// <summary>
/// A utility class for loading environment variables from a .env file.
/// </summary>
public class DotEnv
{
    /// <summary>
    /// Loads environment variables from a .env file into the current process's environment variables.
    /// </summary>
    /// <param name="filePath"></param>
    public static void Load(string filePath)
    {
        if (!File.Exists(filePath))
            return;

        foreach (var line in File.ReadAllLines(filePath))
        {
            if (line.StartsWith('#') || string.IsNullOrWhiteSpace(line))
                continue;

            var parts = line.Split(
                '=',
                StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
                continue;

            Environment.SetEnvironmentVariable(parts[0], parts[1]);
        }
    }

    public static void CreateEnvFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, string.Empty);
        }
    }

    /// <summary>
    /// Writes or updates an entry in the .env file. If the entry already exists, it will be updated with the new data. If it does not exist, a new entry will be added. The method ensures that there is only one instance of the entry in the file.
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="data"></param>
    public static void WriteEntry(string entry, string data)
    {
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
        List<string> lines = [];

        entry = entry.Trim().ToUpper();

        if (File.Exists(filePath))
        {
            lines.AddRange(File.ReadAllLines(filePath));

            while (lines.Count(line => line.StartsWith($"{entry}=")) > 1)
            {
                lines.RemoveAll(line => line.StartsWith($"{entry}="));
            }

            var linePosition = lines.FindIndex(line => line.StartsWith($"{entry}="));
            if (linePosition != -1)
            {
                lines[linePosition] = $"{entry}={data}";
            }
            else
            {
                lines.Add($"{entry}={data}");
            }

            File.WriteAllLines(filePath, lines);
        }
    }

    public static void WriteEntryAndUpdateEnvironmentVariable(string entry, string data)
    {
        WriteEntry(entry, data);
        Environment.SetEnvironmentVariable(entry.Trim().ToUpper(), data);
    }
}
