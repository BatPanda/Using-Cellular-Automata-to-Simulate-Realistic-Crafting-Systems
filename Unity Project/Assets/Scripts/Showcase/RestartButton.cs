using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartButton : MonoBehaviour //A script that restarts the scene.
{
    public void restartScene() {
        SceneManager.LoadScene(0);
    }
}
