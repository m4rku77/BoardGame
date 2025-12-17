using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Text leaderboardText; // drag your UI Text here

    [Header("Settings")]
    [SerializeField] private int maxEntries = 10;

    private string savePath;

    [Serializable]
    public class Entry
    {
        public string name;
        public float timeSeconds; // total time
        public int moves;
        public int score;
        public string date; // optional
    }

    [Serializable]
    private class EntryList
    {
        public List<Entry> entries = new List<Entry>();
    }

    private EntryList data = new EntryList();

    private void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, "leaderboard.json");
        Load();
    }

    private void OnEnable()
    {
        // when the Leaderboard panel is shown
        Render();
    }

    public void AddEntry(string playerName, float timeSeconds, int moves, int score)
    {
        if (string.IsNullOrWhiteSpace(playerName)) playerName = "Player";

        data.entries.Add(new Entry
        {
            name = playerName,
            timeSeconds = timeSeconds,
            moves = moves,
            score = score,
            date = DateTime.Now.ToString("yyyy-MM-dd")
        });

        SortAndTrim();
        Save();
        Render();
    }

    private void SortAndTrim()
    {
        // Sort: score DESC, then time ASC, then moves ASC
        data.entries.Sort((a, b) =>
        {
            int s = b.score.CompareTo(a.score);
            if (s != 0) return s;

            int t = a.timeSeconds.CompareTo(b.timeSeconds);
            if (t != 0) return t;

            return a.moves.CompareTo(b.moves);
        });

        if (data.entries.Count > maxEntries)
            data.entries.RemoveRange(maxEntries, data.entries.Count - maxEntries);
    }

    public void Render()
    {
        if (leaderboardText == null) return;

        if (data.entries.Count == 0)
        {
            leaderboardText.text = "No scores yet!";
            return;
        }

        // Header
        string txt = "Rank  Name            Time    Moves  Score\n";
        txt += "------------------------------------------\n";

        for (int i = 0; i < data.entries.Count; i++)
        {
            var e = data.entries[i];
            txt += $"{(i + 1),-4}  {TrimTo(e.name, 12),-12}  {FormatTime(e.timeSeconds),-6}  {e.moves,5}  {e.score,5}\n";
        }

        leaderboardText.text = txt;
    }

    private string FormatTime(float seconds)
    {
        int m = Mathf.FloorToInt(seconds / 60f);
        int s = Mathf.FloorToInt(seconds % 60f);
        return $"{m:00}:{s:00}";
    }

    private string TrimTo(string s, int max)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Length <= max ? s : s.Substring(0, max);
    }

    public void Save()
    {
        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(savePath, json);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Leaderboard Save failed: " + ex.Message);
        }
    }

    public void Load()
    {
        try
        {
            if (!File.Exists(savePath))
            {
                data = new EntryList();
                return;
            }

            string json = File.ReadAllText(savePath);
            data = JsonUtility.FromJson<EntryList>(json) ?? new EntryList();
            SortAndTrim();
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Leaderboard Load failed: " + ex.Message);
            data = new EntryList();
        }
    }

    // Optional: button to clear leaderboard
    public void ClearAll()
    {
        data.entries.Clear();
        Save();
        Render();
    }
}
