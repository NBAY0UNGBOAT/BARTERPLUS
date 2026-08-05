using System;
using System.IO;
using System.Text.Json;

namespace BarterPOS.Services
{
    public static class AppConfig
    {
        private const string DefaultDatabaseName = "BARTERPLUS";

        public static MongoSettings GetMongoSettings()
        {
            var fileSettings = ReadSettingsFile("appsettings.Local.json")
                ?? ReadSettingsFile("appsettings.json")
                ?? new AppSettings();

            string? connectionString = FirstNotEmpty(
                fileSettings.Mongo?.ConnectionString,
                Environment.GetEnvironmentVariable("BARTERPLUS_MONGO_CONNECTION"));

            string databaseName = FirstNotEmpty(
                fileSettings.Mongo?.DatabaseName,
                Environment.GetEnvironmentVariable("BARTERPLUS_MONGO_DATABASE"),
                DefaultDatabaseName) ?? DefaultDatabaseName;

            return new MongoSettings(connectionString, databaseName);
        }

        private static AppSettings? ReadSettingsFile(string fileName)
        {
            string? path = GetSettingsPath(fileName);

            if (path == null)
            {
                return null;
            }

            try
            {
                string json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<AppSettings>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                return null;
            }
        }

        private static string? GetSettingsPath(string fileName)
        {
            string outputPath = Path.Combine(AppContext.BaseDirectory, fileName);

            if (File.Exists(outputPath))
            {
                return outputPath;
            }

            string workingDirectoryPath = Path.Combine(Environment.CurrentDirectory, fileName);

            return File.Exists(workingDirectoryPath)
                ? workingDirectoryPath
                : null;
        }

        private static string? FirstNotEmpty(params string?[] values)
        {
            foreach (string? value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }

            return null;
        }
    }

    public sealed record MongoSettings(string? ConnectionString, string DatabaseName);

    public sealed class AppSettings
    {
        public MongoConfig? Mongo { get; set; }
    }

    public sealed class MongoConfig
    {
        public string? ConnectionString { get; set; }
        public string? DatabaseName { get; set; }
    }
}
