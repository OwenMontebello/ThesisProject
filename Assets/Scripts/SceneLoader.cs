using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // Call this from your button's OnClick
    public void LoadScene0()
    {
        SceneManager.LoadScene(0);
    }
        public void LoadScene1()
    {
        SceneManager.LoadScene(1);
    }
}