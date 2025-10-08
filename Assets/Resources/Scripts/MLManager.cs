using UnityEngine;
using TensorFlowLite;
using System.IO;
using System.Collections;
using UnityEngine.Networking;
using TMPro;

public class MLManager : MonoBehaviour
{

    public TextMeshProUGUI infoText;

    [Tooltip("Nama file model .tflite yang ada di dalam folder StreamingAssets")]
    public string modelFileName = "GenderClassModel.tflite";

    private float[,] outputTensor = new float[1, 2];

    private Interpreter interpreter;

    IEnumerator Start()
    {
        string modelPath = Path.Combine(Application.streamingAssetsPath, modelFileName);
        Debug.Log("[MLManager] Mencoba memuat model dari path: " + modelPath);

        using (UnityWebRequest www = UnityWebRequest.Get(modelPath))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("[MLManager] Gagal memuat model dari StreamingAssets: " + www.error);
                yield break;
            }
            
            byte[] modelData = www.downloadHandler.data;

            if (modelData != null && modelData.Length > 0)
            {
                var options = new InterpreterOptions() { threads = 2 };
                interpreter = new Interpreter(modelData, options);
                interpreter.AllocateTensors();
                Debug.Log("[MLManager] Model TFLite berhasil dimuat.");
            }
            else
            {
                Debug.LogError("[MLManager] Data model kosong atau korup.");
            }
        }
    }

    // private float Sigmoid(float x)
    // {
    //     return 1.0f / (1.0f + Mathf.Exp(-x));
    // }

    public string Predict(Texture2D inputTexture)
{
    if (interpreter == null)
    {
        Debug.LogError("[MLManager] ERROR: Interpreter belum siap. Model mungkin gagal dimuat.");
        return null;
    }
    
    int inputWidth = 224;
    int inputHeight = 224;
    var inputTensor = new float[1, inputWidth, inputHeight, 3];

    var resizedTexture = ResizeTexture(inputTexture, inputWidth, inputHeight);
    for (int y = 0; y < inputHeight; y++)
    {
        for (int x = 0; x < inputWidth; x++)
        {
            Color32 pixel = resizedTexture.GetPixel(x, y);
            inputTensor[0, y, x, 0] = pixel.r / 255.0f;
            inputTensor[0, y, x, 1] = pixel.g / 255.0f;
            inputTensor[0, y, x, 2] = pixel.b / 255.0f;
        }
    }
    UnityEngine.Object.Destroy(resizedTexture);

    interpreter.SetInputTensorData(0, inputTensor);
    interpreter.Invoke();
    interpreter.GetOutputTensorData(0, outputTensor);

    float femaleProb = outputTensor[0, 0];
    float maleProb = outputTensor[0, 1];

    string prediction;
    float confidence;

    if (maleProb > femaleProb)
    {
        prediction = "Laki-laki";
        confidence = maleProb;
    }
    else
    {
        prediction = "Perempuan";
        confidence = femaleProb;
    }

    string result = $"{{\"prediction\":\"{prediction}\",\"confidence\":{confidence}}}";
    Debug.Log("[MLManager] Hasil prediksi: " + result);

    // === TAMBAHAN UNTUK MENAMPILKAN DI UI ===
    if (infoText != null)
    {
        infoText.text = $"Gender: {prediction}\nConfidence: {confidence:F2}";
    }
    else
    {
        Debug.LogWarning("[MLManager] infoText belum disambungkan di Inspector!");
    }
    // =========================================

    return result;
}


    private Texture2D ResizeTexture(Texture2D source, int newWidth, int newHeight)
    {
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        Graphics.Blit(source, rt);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D readableTexture = new Texture2D(newWidth, newHeight);
        readableTexture.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        readableTexture.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);
        return readableTexture;
    }

    void OnDestroy()
    {
        interpreter?.Dispose();
    }
}
