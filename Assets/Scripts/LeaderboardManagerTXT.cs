using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public class LeaderboardManagerTXT : MonoBehaviour
{
    [Serializable]
    public class Entry
    {
        public string name;
        public float timeSeconds;
        public int moves;
        public int score;
        public string date;
    }

    private string PersistentPath => Path.Combine(Application.persistentDataPath, "Leaderboard.txt");

    private void Awake()
    {
        EnsurePersistentFileExists();
    }

    private void EnsurePersistentFileExists()
    {
        if (File.Exists(PersistentPath))
            return;

        // Try seed from Resources/Leaderboard.txt
        TextAsset seed = Resources.Load<TextAsset>("Leaderboard"); // no .txt
        if (seed != null && !string.IsNullOrWhiteSpace(seed.text))
        {
            File.WriteAllText(PersistentPath, seed.text);
        }
        else
        {
            // Create empty file if no seed
            File.WriteAllText(PersistentPath, "");
        }

        Debug.Log("Leaderboard created at: " + PersistentPath);
    }

    public List<Entry> LoadEntries()
    {
        EnsurePersistentFileExists();

        Debug.Log("Reading leaderboard from: " + PersistentPath);

        var list = new List<Entry>();
        var lines = File.ReadAllLines(PersistentPath);

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            // Name|83.52|24|9120|2025-12-17
            var parts = line.Split('|');
            if (parts.Length < 4) continue;

            string name = parts[0];

            if (!float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float timeSec))
                continue;

            if (!int.TryParse(parts[2], out int moves)) continue;
            if (!int.TryParse(parts[3], out int score)) continue;

            string date = parts.Length >= 5 ? parts[4] : "";

            list.Add(new Entry
            {
                name = name,
                timeSeconds = timeSec,
                moves = moves,
                score = score,
                date = date
            });
        }

        return list;
    }

    public void AddEntry(string name, float timeSeconds, int moves, int score)
    {
        EnsurePersistentFileExists();

        string date = DateTime.Now.ToString("yyyy-MM-dd");
        string line = $"{name}|{timeSeconds.ToString(CultureInfo.InvariantCulture)}|{moves}|{score}|{date}\n";

        File.AppendAllText(PersistentPath, line);
        Debug.Log("Saved entry to: " + PersistentPath);
    }
}
