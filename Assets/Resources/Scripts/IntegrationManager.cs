using UnityEngine;
using System.Collections;
using UnityEngine.XR.ARFoundation;
using System;
using System.Collections.Generic;

[System.Serializable]
public class PredictionOutput
{
    public double confidence;
    public string prediction;
}

[System.Serializable]
public class FilterSet
{
    public GameObject maleFilter;
    public GameObject femaleFilter;
}

public class IntegrationManager : MonoBehaviour
{
    [Header("Integrasi Model ML Lokal (TFLite)")]
    public MLManager mlManager;

    [Header("Komponen AR Camera")]
    public ARFaceManager faceManager;

    [Header("Kumpulan Filter Berdasarkan Gender")]
    public List<FilterSet> filterCollections;

    [Header("Hasil Prediksi Terakhir")]
    public PredictionOutput LatestResult { get; private set; }

    public event Action<PredictionOutput> OnPredictionUpdated;

    private bool isFaceDetected = false;
    private bool isBusy = false;
    private int activeFilterIndex = 0;

    [SerializeField] private float cooldownBetweenPredictions = 2.0f;
    private float lastRunTime = 0f;

    void Start()
    {
        // 🟢 Jika hanya 1 filter set, otomatis dipakai
        if (filterCollections != null && filterCollections.Count > 0)
        {
            activeFilterIndex = 0;
            Debug.Log($"[IntegrationManager] Filter aktif default: {activeFilterIndex}");
        }
        else
        {
            Debug.LogWarning("[IntegrationManager] Tidak ada filter set yang disediakan!");
        }
    }

    void OnEnable()
    {
        if (faceManager != null)
            faceManager.facesChanged += HandleFaceChange;
    }

    void OnDisable()
    {
        if (faceManager != null)
            faceManager.facesChanged -= HandleFaceChange;
    }

    private void HandleFaceChange(ARFacesChangedEventArgs evt)
    {
        if (evt.added.Count > 0 && !isFaceDetected && !isBusy)
        {
            isFaceDetected = true;
            StartCoroutine(RunPredictionRoutine());
        }
        else if (evt.removed.Count > 0)
        {
            isFaceDetected = false;
        }
    }

    private IEnumerator RunPredictionRoutine()
    {
        if (isBusy || Time.time - lastRunTime < cooldownBetweenPredictions)
            yield break;

        isBusy = true;
        lastRunTime = Time.time;

        yield return new WaitForEndOfFrame();

        // Ambil snapshot dari kamera
        Texture2D snapshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        snapshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        snapshot.Apply();

        string jsonResult = mlManager.Predict(snapshot);
        Destroy(snapshot);

        if (!string.IsNullOrEmpty(jsonResult))
        {
            LatestResult = JsonUtility.FromJson<PredictionOutput>(jsonResult);
            ApplyFilterBasedOnPrediction(LatestResult);
            OnPredictionUpdated?.Invoke(LatestResult);
        }

        isBusy = false;
    }

    private void ApplyFilterBasedOnPrediction(PredictionOutput result)
    {
        if (filterCollections == null || filterCollections.Count == 0)
        {
            Debug.LogWarning("[IntegrationManager] Belum ada filter yang diatur!");
            return;
        }

        FilterSet chosenSet = filterCollections[activeFilterIndex];
        GameObject nextFilter = result.prediction.ToLower() == "perempuan"
            ? chosenSet.femaleFilter
            : chosenSet.maleFilter;

        if (nextFilter == null)
        {
            Debug.LogWarning("[IntegrationManager] Filter untuk gender ini belum diset!");
            return;
        }

        if (faceManager.facePrefab != nextFilter)
        {
            faceManager.facePrefab = nextFilter;
            StartCoroutine(ReinitializeFaceManager());
            Debug.Log($"[IntegrationManager] Filter berubah ke: {nextFilter.name}");
        }
    }

    private IEnumerator ReinitializeFaceManager()
    {
        faceManager.enabled = false;
        yield return null;
        faceManager.enabled = true;
    }

    // Hanya aktif kalau ada lebih dari 1 filter set
    public void SwitchFilterSet(int index)
    {
        if (filterCollections == null || filterCollections.Count <= 1)
        {
            Debug.Log("[IntegrationManager] Hanya ada satu filter — pemilihan dinonaktifkan.");
            return;
        }

        if (index >= 0 && index < filterCollections.Count)
        {
            activeFilterIndex = index;
            Debug.Log($"[IntegrationManager] Filter aktif diganti ke: {index}");
        }
    }
}
