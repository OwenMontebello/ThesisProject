#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(PlayerDataVisualizer))]
public class PlayerDataVisualizerEditor : Editor
{
    private SerializedObject serializedObj;
    private PlayerDataList dataList;
    private float currentTime = 0f; //Current time, directly correlated to the slider

    private void OnEnable()
    {
        serializedObj = new SerializedObject(target);
    }

    public override void OnInspectorGUI()
    {
        serializedObj.Update();
        DrawDefaultInspector();

        if (GUILayout.Button("Load Data"))
        {
            LoadData();
        }

        if (dataList != null && dataList.playerDataList.Count > 0)
        {
            float totalTime = dataList.playerDataList.Last().time;
            EditorGUI.BeginChangeCheck();
            currentTime = EditorGUILayout.Slider("Current Time", currentTime, 0f, totalTime);
            if (EditorGUI.EndChangeCheck())
            {
                UpdateCurrentData();
                SceneView.RepaintAll();
            }

        }

        serializedObj.ApplyModifiedProperties();
    }

    private void LoadData()
    {
        PlayerDataVisualizer visualizer = (PlayerDataVisualizer)target;
        if (visualizer.playerDataJson != null)
        {
            string json = visualizer.playerDataJson.text;
            dataList = JsonUtility.FromJson<PlayerDataList>(json);
        }
        else
        {
            Debug.LogError("playerDataJson is null. Please assign a valid JSON file.");
        }
    }

    private void UpdateCurrentData()
    {
        PlayerDataVisualizer visualizer = (PlayerDataVisualizer)target;
        if (dataList != null && dataList.playerDataList.Count > 1)
        {
            // Find the data at the current time
            PlayerData startData = dataList.playerDataList.LastOrDefault(p => p.time <= currentTime);
            PlayerData endData = dataList.playerDataList.FirstOrDefault(p => p.time >= currentTime);

            if (startData != null && endData != null)
            {
                float segmentT = (currentTime - startData.time) / (endData.time - startData.time);
                segmentT = float.IsNaN(segmentT) ? 0 : segmentT;  // Check for NaN
            }
        }
    }

    private void OnSceneGUI()
    {
        if (dataList != null && dataList.playerDataList.Count > 1)
        {
            Handles.color = Color.red;
            for (int i = 1; i < dataList.playerDataList.Count; i++)
            {
                if (dataList.playerDataList[i - 1].position.Count < 3 || dataList.playerDataList[i].position.Count < 3)
                {
                    Debug.LogError("Position data is invalid.");
                    continue;
                }

                Vector3 startPos = new Vector3(dataList.playerDataList[i - 1].position[0], dataList.playerDataList[i - 1].position[1], dataList.playerDataList[i - 1].position[2]);
                Vector3 endPos = new Vector3(dataList.playerDataList[i].position[0], dataList.playerDataList[i].position[1], dataList.playerDataList[i].position[2]);
                Handles.DrawLine(startPos, endPos);
            }

            //Find the position at the current time
            PlayerData startData = dataList.playerDataList.LastOrDefault(p => p.time <= currentTime);
            PlayerData endData = dataList.playerDataList.FirstOrDefault(p => p.time >= currentTime);

            if (startData != null && endData != null)
            {
                float segmentT = (currentTime - startData.time) / (endData.time - startData.time);
                segmentT = float.IsNaN(segmentT) ? 0 : segmentT;  //Check for NaN

                Vector3 startPosition = new Vector3(startData.position[0], startData.position[1], startData.position[2]);
                Vector3 endPosition = (startData == endData) ? startPosition : new Vector3(endData.position[0], endData.position[1], endData.position[2]);
                Vector3 interpolatedPosition = Vector3.Lerp(startPosition, endPosition, segmentT);

                Handles.color = Color.green;
                float sphereSize = 0.2f;  //Adjust the size as needed
                Handles.SphereHandleCap(0, interpolatedPosition, Quaternion.identity, sphereSize, EventType.Repaint);
                Handles.Label(interpolatedPosition + Vector3.up * sphereSize, $"Time: {currentTime:F2} secs");
            }
            else
            {
                if (startData == null) Debug.LogError("Start data is null.");
                if (endData == null) Debug.LogError("End data is null.");
            }
        }
        else
        {
            if (dataList == null) Debug.LogError("Data list is null.");
            if (dataList != null && dataList.playerDataList.Count <= 1) Debug.LogError("Not enough data points to visualize.");
        }
    }
}

public class PlayerDataVisualizer : MonoBehaviour
{
    public TextAsset playerDataJson;

}
#endif
