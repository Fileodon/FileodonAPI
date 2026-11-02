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

    [Fact]
    public void Creates_env_file_from_env_example_when_available()
    {
        ExecuteInTemporaryDirectory(() =>
        {
            const string exampleContents = "JWT_KEY=example-key\nDATABASE_URL=example-database";
            File.WriteAllText(".env.example", exampleContents);

            DotEnv.CreateEnvFile(".env");

            Assert.Equal(exampleContents, File.ReadAllText(".env"));
        });
    }

    [Fact]
    public void Creates_empty_env_file_when_env_example_is_unavailable()
    {
        ExecuteInTemporaryDirectory(() =>
        {
            DotEnv.CreateEnvFile(".env");

            Assert.True(File.Exists(".env"));
            Assert.Equal(string.Empty, File.ReadAllText(".env"));
        });
    }

    [Fact]
    public void Does_not_overwrite_an_existing_env_file()
    {
        ExecuteInTemporaryDirectory(() =>
        {
            File.WriteAllText(".env.example", "JWT_KEY=example-key");
            File.WriteAllText(".env", "JWT_KEY=existing-key");

            DotEnv.CreateEnvFile(".env");

            Assert.Equal("JWT_KEY=existing-key", File.ReadAllText(".env"));
        });
    }

    [Fact]
    public void Updates_an_existing_entry()
    {
        ExecuteInTemporaryDirectory(() =>
        {
            File.WriteAllLines(".env", ["EXISTING_KEY=old-value", "OTHER_KEY=keep"]);

            DotEnv.WriteEntry(" existing_key ", "new-value");

            Assert.Equal(
                ["EXISTING_KEY=new-value", "OTHER_KEY=keep"],
                File.ReadAllLines(".env"));
        });
    }

    [Fact]
    public void Appends_a_new_entry()
    {
        ExecuteInTemporaryDirectory(() =>
        {
            File.WriteAllLines(".env", ["EXISTING_KEY=keep"]);

            DotEnv.WriteEntry("new_key", "new-value");

            Assert.Equal(
                ["EXISTING_KEY=keep", "NEW_KEY=new-value"],
                File.ReadAllLines(".env"));
        });
    }

    [Fact]
    public void Removes_duplicate_entries_when_updating()
    {
        ExecuteInTemporaryDirectory(() =>
        {
            File.WriteAllLines(
                ".env",
                ["DUPLICATE_KEY=first", "OTHER_KEY=keep", "DUPLICATE_KEY=second"]);

            DotEnv.WriteEntry("duplicate_key", "new-value");

            Assert.Equal(
                ["OTHER_KEY=keep", "DUPLICATE_KEY=new-value"],
                File.ReadAllLines(".env"));
        });
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

    private static void ExecuteInTemporaryDirectory(Action action)
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        string temporaryDirectory = Directory.CreateTempSubdirectory("fileodon-test-").FullName;

        try
        {
            Directory.SetCurrentDirectory(temporaryDirectory);
            action();
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }
}

[CollectionDefinition("Environment variables", DisableParallelization = true)]
public sealed class EnvironmentVariablesCollection;
