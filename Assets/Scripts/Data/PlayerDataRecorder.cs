using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Collections;

[Serializable]
public class PlayerData
{
    public List<float> position = new List<float>();
    public float time;

    public PlayerData(Vector3 pos, float time)
    {
        position.Add(pos.x);
        position.Add(pos.y);
        position.Add(pos.z);
        this.time = time;
    }
}

[Serializable]
public class PlayerDataList
{
    public List<PlayerData> playerDataList = new List<PlayerData>();
}

public class PlayerDataRecorder : MonoBehaviour
{
    public PlayerDataList dataList = new PlayerDataList();
    public string directoryPath = "Results"; //Default directory folder name for data saving
    public float captureInterval = 0.1f;  //Adjust this interval as needed

    void Start()
    {
        //Initialize directoryPath to the "Results" folder in the root directory of the build
        directoryPath = Path.Combine(Application.dataPath, "..", directoryPath);

        //Make sure the directory exists
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        StartCoroutine(CapturePlayerData());

    }




    private IEnumerator CapturePlayerData()
    {
        while (true)
        {
            float currentTime = Time.time;
            dataList.playerDataList.Add(new PlayerData(transform.position, currentTime));
            yield return new WaitForSeconds(captureInterval);
        }
    }

    void OnApplicationQuit()
    {
        SaveDataToJson();
    }

    public void SaveDataToJson()
    {
        string filePath = Path.Combine(directoryPath, "playerData_" + GetNextFileIndex() + ".json");

        string json = JsonUtility.ToJson(dataList);
        File.WriteAllText(filePath, json);

        //Log the confirmation message to the console
        Debug.Log($"Player data saved to {filePath}");
    }

    int GetNextFileIndex()
    {
        int index = 0;
        while (File.Exists(Path.Combine(directoryPath, "playerData_" + index + ".json")))
        {
            index++;
        }
        return index;
    }
}
