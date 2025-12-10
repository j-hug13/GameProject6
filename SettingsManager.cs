using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;

namespace GameProject6
{
    public class SettingsManager
    {
        public MoveSpeed MoveSpeed { get; set; } = MoveSpeed.Normal;
        public bool IsFullscreen { get; set; } = false;
        public int ResolutionIndex { get; set; } = 0;

        private static string file = "settings.json";

        public static SettingsManager Load()
        {
            if (File.Exists(file) == false)
            {
                return new SettingsManager();
            }

            var json = File.ReadAllText(file);
            return JsonSerializer.Deserialize<SettingsManager>(json) ?? new SettingsManager();
        }

        public void Save()
        {
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(file, json);
        }
    }
}
