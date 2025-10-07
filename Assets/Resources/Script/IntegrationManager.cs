using UnityEngine;
using System.Collections;
using UnityEngine.XR.ARFoundation;
using System;

[System.Serializable]
public class PredictionResponse
{
    public double confidence;
    public string prediction;
}

public class IntegrationManager : MonoBehaviour
{
    [Header("Integrasi ML Lokal (TFLite)")]
    public MLManager mlManager;

    [Header("Integrasi AR Camera")]
    public ARFaceManager faceManager;

    [Header("Hasil Prediksi Terbaru (bisa diakses script lain)")]
    public PredictionResponse LatestPrediction { get; private set; }

    // Event: dipanggil setiap kali ada hasil prediksi baru
    public event Action<PredictionResponse> OnPredictionUpdated;

    private bool isFaceTracked = false;
    private bool isProcessing = false;

    // Opsional: jeda antar prediksi agar tidak berat (dalam detik)
    [SerializeField] private float predictionCooldown = 2.0f;
    private float lastPredictionTime = 0f;

    void OnEnable()
    {
        if (faceManager != null)
            faceManager.facesChanged += OnFacesChanged;
    }

    void OnDisable()
    {
        if (faceManager != null)
            faceManager.facesChanged -= OnFacesChanged;
    }

    private void OnFacesChanged(ARFacesChangedEventArgs eventArgs)
    {
        if (eventArgs.added.Count > 0 && !isFaceTracked && !isProcessing)
        {
            isFaceTracked = true;
            Debug.Log("[IntegrationManager] Wajah terdeteksi, mulai prediksi...");
            StartCoroutine(ProcessPrediction());
        }
        else if (eventArgs.removed.Count > 0)
        {
            isFaceTracked = false;
            Debug.Log("[IntegrationManager] Wajah hilang dari kamera.");
        }
    }

    private IEnumerator ProcessPrediction()
    {
        // Hindari prediksi berulang terlalu cepat
        if (isProcessing || Time.time - lastPredictionTime < predictionCooldown)
            yield break;

        isProcessing = true;
        lastPredictionTime = Time.time;

        yield return new WaitForEndOfFrame();

        // Ambil gambar dari frame kamera
        Texture2D screenTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenTexture.Apply();

        // Jalankan prediksi lewat MLManager (lokal)
        string resultJson = mlManager.Predict(screenTexture);
        Destroy(screenTexture);

        if (!string.IsNullOrEmpty(resultJson))
        {
            Debug.Log("[MLManager] Hasil prediksi: " + resultJson);

            try
            {
                // Konversi JSON ke objek PredictionResponse
                LatestPrediction = JsonUtility.FromJson<PredictionResponse>(resultJson);
                Debug.Log($"[IntegrationManager] Prediksi: {LatestPrediction.prediction} ({LatestPrediction.confidence:F2})");

                // Kirim notifikasi ke script lain jika berlangganan event
                OnPredictionUpdated?.Invoke(LatestPrediction);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[IntegrationManager] Gagal parse JSON hasil prediksi: " + e.Message);
            }
        }
        else
        {
            Debug.LogWarning("[MLManager] Gagal mendapatkan hasil prediksi.");
        }

        isProcessing = false;
    }
}
