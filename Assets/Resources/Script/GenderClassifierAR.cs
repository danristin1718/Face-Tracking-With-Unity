using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.Collections.LowLevel.Unsafe;
using TMPro;
using System.Collections;
using UnityEngine.Networking; // Diperlukan untuk request web

public class APIClassifier : MonoBehaviour
{
    [Header("Referensi Scene")]
    public TextMeshProUGUI logText;

    [Header("Pengaturan Server")]
    // Ganti dengan alamat IP lokal komputer Anda
    public string serverIP = "192.168.1.10"; 
    public string serverPort = "5000";

    [Header("Pengaturan Kamera")]
    public int imageWidth = 224;
    public int imageHeight = 224;
    // Seberapa sering mengirim gambar ke server (dalam detik)
    public float predictionFrequency = 1.0f; 

    private ARCameraManager cameraManager;
    private Texture2D inputTexture;
    private bool isRequestInProgress = false;
    private float timeSinceLastPrediction = 0f;
    private string serverUrl;

    void Start()
    {
        cameraManager = FindFirstObjectByType<ARCameraManager>();
        if (cameraManager == null)
        {
            Debug.LogError("ARCameraManager tidak ditemukan.");
            return;
        }

        // Gabungkan alamat IP dan Port menjadi URL lengkap
        serverUrl = $"http://{serverIP}:{serverPort}/predict";
        inputTexture = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false);
        logText.text = "Arahkan ke wajah...";
    }

    void OnEnable() { if (cameraManager != null) cameraManager.frameReceived += OnCameraFrameReceived; }
    void OnDisable() { if (cameraManager != null) cameraManager.frameReceived -= OnCameraFrameReceived; }

    void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
    {
        // Kontrol agar tidak mengirim gambar terlalu sering
        timeSinceLastPrediction += Time.deltaTime;
        if (timeSinceLastPrediction < predictionFrequency || isRequestInProgress)
        {
            return;
        }

        timeSinceLastPrediction = 0f;

        if (!cameraManager.TryAcquireLatestCpuImage(out XRCpuImage cpuImage)) return;

        // Proses konversi gambar tetap sama
        var conversionParams = new XRCpuImage.ConversionParams {
            inputRect = new RectInt(0, 0, cpuImage.width, cpuImage.height),
            outputDimensions = new Vector2Int(imageWidth, imageHeight),
            outputFormat = TextureFormat.RGB24,
            transformation = XRCpuImage.Transformation.MirrorY
        };

        var buffer = new Unity.Collections.NativeArray<byte>((int)(imageWidth * imageHeight * 3), Unity.Collections.Allocator.Temp);
        unsafe { cpuImage.Convert(conversionParams, new System.IntPtr(buffer.GetUnsafePtr()), buffer.Length); }
        cpuImage.Dispose();
        
        inputTexture.LoadRawTextureData(buffer);
        inputTexture.Apply();
        buffer.Dispose();

        // Mulai proses pengiriman gambar ke server
        StartCoroutine(UploadAndPredict(inputTexture));
    }

    IEnumerator UploadAndPredict(Texture2D image)
    {
        isRequestInProgress = true;

        // Encode gambar menjadi format JPEG dalam bentuk byte array
        byte[] imageData = image.EncodeToJPG();
        
        // Buat form untuk mengirim data file
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", imageData, "capture.jpg", "image/jpeg");

        // Kirim request POST ke server
        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                // Jika berhasil, parse JSON yang diterima
                string jsonResponse = www.downloadHandler.text;
                PredictionResult result = JsonUtility.FromJson<PredictionResult>(jsonResponse);
                
                // Tampilkan hasil
                logText.text = $"{result.label}: {result.confidence:P1}";
            }
            else
            {
                // Jika gagal, tampilkan error
                logText.text = "Error: " + www.error;
                Debug.LogError("Error from server: " + www.downloadHandler.text);
            }
        }

        isRequestInProgress = false;
    }
}

// Class helper untuk mem-parse JSON dari server
[System.Serializable]
public class PredictionResult
{
    public string label;
    public float confidence;
}