using Controller.Utility;

namespace Test;

[Collection("Environment variables")]
public sealed class DotEnvTests
{
    [Fact]
    public void Loads_key_value_pairs_into_environment()
    {
        const string key = "FILEODON_TEST_DOTENV_VALUE";
        string filePath = CreateEnvFile($"{key}=test-value");

        try
        {
            DotEnv.Load(filePath);

            Assert.Equal("test-value", Environment.GetEnvironmentVariable(key));
        }
        finally
        {
            RemoveEnvironmentVariable(key);
            File.Delete(filePath);
        }
    }

    [Fact]
    public void Ignores_blank_lines_and_comments()
    {
        const string key = "FILEODON_TEST_DOTENV_COMMENT";
        string filePath = CreateEnvFile(
            $"# {key}=ignored",
            string.Empty,
            $"{key}=loaded");

        try
        {
            DotEnv.Load(filePath);

            Assert.Equal("loaded", Environment.GetEnvironmentVariable(key));
        }
        finally
        {
            RemoveEnvironmentVariable(key);
            File.Delete(filePath);
        }
    }

    [Fact]
    public void Ignores_malformed_lines()
    {
        const string key = "FILEODON_TEST_DOTENV_MALFORMED";
        string filePath = CreateEnvFile(
            $"{key}",
            $"{key}=first=second");

        try
        {
            RemoveEnvironmentVariable(key);
            DotEnv.Load(filePath);

            Assert.Null(Environment.GetEnvironmentVariable(key));
        }
        finally
        {
            RemoveEnvironmentVariable(key);
            File.Delete(filePath);
        }
    }

    [Fact]
    public void Does_nothing_when_file_does_not_exist()
    {
        string filePath = Path.Combine(
            Path.GetTempPath(),
            $"fileodon-missing-{Guid.NewGuid():N}.env");

        DotEnv.Load(filePath);

        Assert.False(File.Exists(filePath));
    }

    private static string CreateEnvFile(params string[] lines)
    {
        string filePath = Path.Combine(
            Path.GetTempPath(),
            $"fileodon-test-{Guid.NewGuid():N}.env");
        File.WriteAllLines(filePath, lines);
        return filePath;
    }

    private static void RemoveEnvironmentVariable(string key)
    {
        Environment.SetEnvironmentVariable(key, null);
    }
}

[CollectionDefinition("Environment variables", DisableParallelization = true)]
public sealed class EnvironmentVariablesCollection;
