using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("SFX Settings")]
    public AudioClip starPickupClip, winClip, loseClip;
    public GameObject Music;
    public static GameManager I;

    [Header("Goal Settings")]
    public LevelGenerator level;
    public int starsRequired = 3;

    [Header("UI")]
    public TextMeshProUGUI starText;
    public GameObject winPanel, losePanel;
    public bool isWin, isLose;

    int _stars;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;

        _stars = 0;
        UpdateUI();

        if (winPanel) winPanel.SetActive(false);

    
        if (level && level.currentGoal)
            level.currentGoal.SetActive(false);
    }

    public void AddStar(int amount = 1)
    {
        _stars += amount;
        SFXManager.I.Play2D(starPickupClip);
        UpdateUI();

        if (_stars >= starsRequired)
            OpenExit();
    }

    void UpdateUI()
    {
        if (starText)
            starText.text = $"Stars: {_stars}/{starsRequired}";
    }

    void OpenExit()
    {
        if (level && level.currentGoal)
            level.currentGoal.SetActive(true);
    }



public void Win()
{
    if (isWin) return;
    isWin = true;

    SFXManager.I.Play2D(winClip);
    Music.SetActive(false);

    if (winPanel) winPanel.SetActive(true);

    //Save data before switching scene
    SavePlayerData();

    //Unlock cursor and reset timescale for menu
    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;
    Time.timeScale = 1f;

    SceneManager.LoadScene(2); 
}


    void SavePlayerData()
    {
        PlayerDataRecorder recorder = FindObjectOfType<PlayerDataRecorder>();
        if (recorder != null)
        {
            recorder.SaveDataToJson();
        }
    }
}
