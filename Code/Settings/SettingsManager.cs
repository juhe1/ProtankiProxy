using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProtankiProxy.Settings
{
    public class SettingsManager
    {
        private static readonly string AppDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ProtankiProxy");
        private static readonly string SettingsPath = Path.Combine(AppDataPath, "settings.json");

        public static async Task SaveSettingsAsync(ConnectionSettings settings)
        {
            try
            {
                // Ensure directory exists
                Directory.CreateDirectory(AppDataPath);

                // Serialize settings to JSON
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                // Write to file
                await File.WriteAllTextAsync(SettingsPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save settings: {ex.Message}");
                throw;
            }
        }

        public static async Task<ConnectionSettings> LoadSettingsAsync()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = await File.ReadAllTextAsync(SettingsPath);
                    return JsonSerializer.Deserialize<ConnectionSettings>(json) ?? new ConnectionSettings();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load settings: {ex.Message}");
                throw;
            }

            return new ConnectionSettings();
        }
    }
} 