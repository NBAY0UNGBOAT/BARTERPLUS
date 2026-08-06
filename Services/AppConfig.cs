using System;
using System.IO;
using System.Text.Json;

namespace BarterPOS.Services
{
    public static class AppConfig
    {
        private const string DefaultDatabaseName = "BARTERPLUS";
        private const string DefaultStoreName = "BarterPlus Store";
        private const string DefaultReceiptFooter = "Thank you for shopping with us!";

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

        public static StoreInfo GetStoreInfo()
        {
            var fileSettings = ReadSettingsFile("appsettings.Local.json")
                ?? ReadSettingsFile("appsettings.json")
                ?? new AppSettings();

            var store = fileSettings.Store ?? new StoreConfig();

            return new StoreInfo(
                Name: FirstNotEmpty(store.Name, DefaultStoreName) ?? DefaultStoreName,
                AddressLine1: store.AddressLine1 ?? string.Empty,
                AddressLine2: store.AddressLine2 ?? string.Empty,
                Phone: store.Phone ?? string.Empty,
                ReceiptFooter: FirstNotEmpty(store.ReceiptFooter, DefaultReceiptFooter) ?? DefaultReceiptFooter);
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

    public sealed record StoreInfo(
        string Name,
        string AddressLine1,
        string AddressLine2,
        string Phone,
        string ReceiptFooter);

    public sealed class AppSettings
    {
        public MongoConfig? Mongo { get; set; }
        public StoreConfig? Store { get; set; }
    }

    public sealed class MongoConfig
    {
        public string? ConnectionString { get; set; }
        public string? DatabaseName { get; set; }
    }

    public sealed class StoreConfig
    {
        public string? Name { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? Phone { get; set; }
        public string? ReceiptFooter { get; set; }
    }
}