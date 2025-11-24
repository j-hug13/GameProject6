using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GameProject6
{
    public class LeaderboardEntry
    {
        public string PlayerName { get; set; }
        public float Time { get; set; }
    }

    public class Leaderboard
    {
        public List<LeaderboardEntry> Entries { get; private set; } = new List<LeaderboardEntry>();
        public CubeType Type { get; private set; }

        private string file;
    
        public Leaderboard(CubeType cube)
        {
            Type = cube;
            file = "leaderboard_" + Type.ToString();
            LoadContent();
        }

        public LeaderboardEntry AddEntry(string player, float time)
        {
            LeaderboardEntry entry = new LeaderboardEntry();
            entry.PlayerName = player;
            entry.Time = time;
            Entries.Add(entry);
            Entries = Entries.OrderBy(x => x.Time).ToList();
            Save();
            return entry;
        }

        public void LoadContent()
        {
            if (File.Exists(file) == true)
            {
                var json = File.ReadAllText(file);
                Entries = JsonSerializer.Deserialize<List<LeaderboardEntry>>(json) ?? new List<LeaderboardEntry>();
            }
        }

        public void Save()
        {
            var json = JsonSerializer.Serialize(Entries, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(file, json);
        }
    }
}
