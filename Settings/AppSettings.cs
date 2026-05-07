using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;


namespace SpotifyVolumeControl.Settings
{
    public class AppSettings
    {
        private static readonly string settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SpotifyVolumeControl", "settings.json");
        private static readonly JsonSerializerOptions jsonOptions = new() { WriteIndented = true };

        public Keys ToggleKey { get; set; } = Keys.F24;
        public float VolumeStep { get; set; } = 0.02f;

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(settingsPath))
                {
                    string json = File.ReadAllText(settingsPath);
                    return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch { }

            return new AppSettings();
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(settingsPath)!);
                File.WriteAllText(settingsPath, JsonSerializer.Serialize(this, jsonOptions));
            }
            catch { }
        }
    }

}
