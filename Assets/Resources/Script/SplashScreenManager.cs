using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScreenManager : MonoBehaviour
{
    // Berapa lama splash screen akan tampil (dalam detik)
    public float delay = 3f;

    // Nama scene yang akan dimuat setelahnya
    public string sceneToLoad = "MainMenuScene";

    // Fungsi ini dipanggil saat script pertama kali aktif
    void Start()
    {
        // Panggil fungsi LoadNextScene setelah waktu 'delay'
        Invoke("LoadNextScene", delay);
    }

    void LoadNextScene()
    {
        // Pindah ke scene berikutnya
        SceneManager.LoadScene(sceneToLoad);
    }
}