namespace Controller.Validators;

public static class CommonRules
{
    public static bool BeAValidPath(string path)
    {
        try
        {
            var fullPath = Path.GetFullPath(path);
            return !string.IsNullOrEmpty(fullPath);
        }
        catch (Exception)
        {
            return false;
        }
    }
}
