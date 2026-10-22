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
}
