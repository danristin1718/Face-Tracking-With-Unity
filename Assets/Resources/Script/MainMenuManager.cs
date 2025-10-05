using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // Ganti "NamaSceneARAnda" dengan nama scene AR utama Anda yang sebenarnya
    public string arSceneName = "MainScene";

    // Fungsi publik ini akan kita panggil dari tombol
    public void GoToARScene()
    {
        SceneManager.LoadScene(arSceneName);
    }
}