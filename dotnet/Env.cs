using System;
using System.IO;

namespace debmenu;

public static class Env
{
    private static readonly string envFileName = ".env";

    public static void Load()
    {
        string envPath = Path.Combine(AppContext.BaseDirectory, envFileName);
        // Fallback: If it's not in the output directory, check 3 levels up (project root during `dotnet run`)
        if (!File.Exists(envPath))
        {
            envPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", envFileName);
        }

        foreach (var line in File.ReadAllLines(envPath))
        {
            // Skip empty lines or comments
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                continue;

            // Split by the first '=' character only
            int index = line.IndexOf('=');
            if (index > 0)
            {
                string key = line[..index].Trim();
                string value = line[(index + 1)..].Trim();

                // Strip surrounding quotes if present
                if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                    (value.StartsWith("'") && value.EndsWith("'")))
                {
                    value = value[1..^1];
                }

                // Set as Environment Variable for the current process
                Environment.SetEnvironmentVariable(key, value, EnvironmentVariableTarget.Process);
            }
        }
    }
}
