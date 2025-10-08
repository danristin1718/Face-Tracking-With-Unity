using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public string arSceneName = "CameraScene";
    
    public void GoToARScene()
    {
        SceneManager.LoadScene(arSceneName);
    }
}