using System;
using System.IO;
using System.Text.Json;

namespace BarterPOS.Services
{
    internal static class LocalApplicationStorage
    {
        public static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public static string GetDataFilePath(string fileName)
        {
            string baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            if (string.IsNullOrWhiteSpace(baseFolder))
            {
                baseFolder = AppContext.BaseDirectory;
            }

            string folder = Path.Combine(baseFolder, "BarterPOS");
            Directory.CreateDirectory(folder);
            return Path.Combine(folder, fileName);
        }
    }
}
