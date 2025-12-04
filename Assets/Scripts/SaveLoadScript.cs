using System;
using System.IO;
using System.Runtime.Serialization.Json;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
public class SaveLoadScript : MonoBehaviour
{
    public string saveFileName = "mansFails.json";

    [Serializable]
    public class GameData
    {
        public int character;
        public String characterName;
        // add other if needed for leaderboard
    }

    private GameData gameData = new GameData();

    public void SaveGame(int character, String characterName)
    {
        gameData.character = character;
        gameData.characterName = characterName;
        string json = JsonUtility.ToJson(gameData);

        File.WriteAllText(Application.persistentDataPath + "/" + saveFileName, json);
        Debug.Log("Game saved to: "+ Application.persistentDataPath + "/" + saveFileName);
    }

    public void LoadGame()
    {
        string filePath = Application.persistentDataPath + "/" + saveFileName;

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            gameData = JsonUtility.FromJson<GameData>(json);

            // uste gamedata to restore game staate
        }
        else
        {
            Debug.LogWarning("Save file not found: " + filePath);
        }
    }

}

