# Proyek UTS AR/VR: Aplikasi Klasifikasi Gender

Aplikasi ini merupakan proyek **Augmented Reality (AR)** untuk platform Android yang dibuat menggunakan **Unity** sebagai bagian dari proyek Ujian Tengah Semester (UTS) mata kuliah Augmented & Virtual Reality.  
Aplikasi mampu melakukan **klasifikasi gender (Perempuan/Laki-laki)** secara *real-time* berdasarkan input dari kamera depan perangkat.  
Proyek ini menggunakan model *machine learning* yang dijalankan secara **lokal** melalui file **GenderClassModel.tflite** menggunakan **TensorFlow Lite plugin dari Koki Ibukuro**.

---

## 🧾 Proyek UTS 

**Mata Kuliah:** Augmented & Virtual Reality  
**Dosen Pengampu:** Muhammad Panji Muslim, S.Pd., M.Kom.

---

## 👥 Anggota Kelompok

| No | Nama Lengkap Anggota | NPM |
|----|-----------------------|-----|
| 1  | Dana Christin | 2210511115 |
| 2  | Anja Bunga Aditya | 2210511161 |
| 3  | Noer Fauzan Detya Gulfiar | 2210511151 |
| 4  | Nathan Abigail Rahman | 2410511036 |
| 5  | Melva Fereyzha Kirana Myko Putri | 2410511165 |
| 6  | Jeremia Marco Namara | 2410511143 |

---

## 🎬 Alur Kerja Aplikasi

Aplikasi ini memiliki tiga *scene* utama:

1. **Splash Screen** – Menampilkan tampilan pembuka selama beberapa detik.
2. **Main Menu Scene** – Menu utama dengan tombol **“Open Camera”**.
3. **Camera Scene** – Mengaktifkan kamera depan dan melakukan klasifikasi gender menggunakan model **GenderClassModel.tflite** yang tersimpan secara lokal.

---

## ✨ Fitur Utama

- **Klasifikasi Gender (Local)** – Model TensorFlow Lite berjalan langsung di perangkat Android tanpa koneksi jaringan.  
- **Alur UI Lengkap** – Tiga scene: Splash Screen → Menu Utama → Camera Scene.  
- **Filter AR Dinamis** – Menampilkan *prefab* wajah berbeda sesuai hasil klasifikasi gender.  

---

## 🛠️ Arsitektur & Teknologi

### 🧩 Unity 

**Engine:** Unity  
**Editor:** Unity Editor 6.0  
**Plugin Tambahan:** TensorFlow Lite for Unity (Koki Ibukuro)  
**Platform Target:** Android  

**Struktur Folder:**
```
Assets/
├── Prefabs/        
├── Resources/
│   ├── Script/     
│   └── UI/         
│   └── Prefab/     
└── Scenes/         
    ├── SplashScreenScene.unity
    ├── MainMenuScene.unity
    └── CameraScene.unity
```

---

## 💻 Penjelasan Script C#

### 1. SplashScreenManager.cs
**Fungsi:** Menampilkan splash screen selama beberapa detik lalu berpindah ke *Main Menu*.

```csharp
public class SplashScreenManager : MonoBehaviour
{
    public float delay = 3f;
    public string sceneToLoad = "MainMenuScene";

    void Start()
    {
        Invoke("LoadNextScene", delay);
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
```

---

### 2. MainMenuManager.cs
**Fungsi:** Menangani tombol “Open Camera” untuk masuk ke scene utama.

```csharp
public class MainMenuManager : MonoBehaviour
{
    public string arSceneName = "CameraScene";

    public void GoToARScene()
    {
        SceneManager.LoadScene(arSceneName);
    }
}
```

---

### 3. MLManager.cs
**Fungsi:** Menjalankan model *TensorFlow Lite* secara lokal menggunakan plugin Koki Ibukuro.

```csharp
using TensorFlowLite;

public class MLManager : MonoBehaviour
{
    [SerializeField] private TextAsset modelFile;
    private Interpreter interpreter;
    private float[,,,] inputTensor = new float[1, 224, 224, 3];
    private float[,,] outputTensor = new float[1, 1, 2];

    void Start()
    {
        interpreter = new Interpreter(modelFile.bytes);
        interpreter.AllocateTensors();
    }

    public string Predict(Texture2D inputTexture)
    {
        // Konversi gambar ke tensor input
        var pixels = inputTexture.GetPixels32();
        for (int y = 0; y < 224; y++)
        {
            for (int x = 0; x < 224; x++)
            {
                Color32 color = pixels[y * 224 + x];
                inputTensor[0, y, x, 0] = color.r / 255f;
                inputTensor[0, y, x, 1] = color.g / 255f;
                inputTensor[0, y, x, 2] = color.b / 255f;
            }
        }

        // Jalankan inferensi
        interpreter.SetInputTensorData(0, inputTensor);
        interpreter.Invoke();
        interpreter.GetOutputTensorData(0, outputTensor);

        float femaleProb = outputTensor[0, 0, 0];
        float maleProb = outputTensor[0, 0, 1];

        string label = maleProb > femaleProb ? "Laki-laki" : "Perempuan";
        float confidence = Mathf.Max(maleProb, femaleProb);

        return $"{label} ({confidence * 100f:F2}%)";
    }
}
```

---

### 4. IntegrationManager.cs
**Fungsi:** Menghubungkan hasil prediksi ML dengan *AR Face Manager* untuk menampilkan filter yang sesuai.

```csharp
private void ApplyFilterBasedOnPrediction(string label)
{
    FilterSet chosenSet = filterCollections[activeFilterIndex];
    GameObject nextFilter = label.ToLower() == "perempuan"
        ? chosenSet.femaleFilter
        : chosenSet.maleFilter;

    if (faceManager.facePrefab != nextFilter)
    {
        faceManager.facePrefab = nextFilter;
        StartCoroutine(ReinitializeFaceManager());
    }
}
```

---

## 💡 Alur Eksekusi

1. Splash screen muncul 3 detik.  
2. Beralih ke menu utama.  
3. Pengguna klik “Open Camera”.  
4. Kamera depan aktif dan model `.tflite` dijalankan untuk klasifikasi.  
5. Filter wajah berubah sesuai hasil prediksi gender.
6. Akan muncul UI info berupa hasil pengolahan ML terhadap deteksi gambar wajah, apabila ML mendeteksi wajah perempuan, maka akan tampil sebagai "Gender: Perempuan" dengan confidence yang berbeda-beda, begitupun dengan deteksi wajah lelaki.

---

## 🚀 Panduan Menjalankan Proyek

### 1. Unduh atau Clone Repositori
```bash
git clone https://github.com/danristin1718/Face-Tracking-With-Unity.git
```

### 2. Buka Proyek di Unity
1. Buka Unity Hub → **Open Project**.  
2. Pastikan TensorFlow Lite plugin dari Koki Ibukuro sudah di-*import*.  
3. Letakkan file `GenderClassModel.tflite` di folder `Assets/StreamingAssets/`.  

### 3. Build ke Android
- **Build Settings → Android → Build And Run**  
- Pastikan kamera depan aktif dan izin kamera diberikan.

---
