# Proyek UTS AR/VR: Aplikasi Klasifikasi Gender

Aplikasi ini merupakan proyek **Augmented Reality (AR)** untuk platform Android yang dibuat menggunakan **Unity** sebagai bagian dari proyek Ujian Tengah Semester (UTS) mata kuliah Augmented & Virtual Reality.
Aplikasi mampu melakukan **klasifikasi gender (Perempuan/Laki-laki)** secara *real-time* berdasarkan input dari kamera depan perangkat.
Proyek ini menggunakan **arsitektur klien–server**, di mana proses inferensi *machine learning* dilakukan melalui **REST API lokal** yang dibangun dengan **Flask (Python)**.

---

## 🧾 Proyek UTS 

**Mata Kuliah:** Augmented & Virtual Reality  
**Dosen Pengampu:** Muhammad Panji Muslim, S.Pd., M.Kom.

---

## 👥 Anggota Kelompok

Proyek ini dikerjakan oleh:

| No | Nama Lengkap Anggota | NPM |
|----|-----------------------|-----|
| 1  | Dana Christin | 221051115 |
| 2  | Anja Bunga Aditya | 2210511161 |
| 3  | Noer Fauzan Detya Gulfiar | 2210511151 |
| 4  | Nathan Abigail Rahman | 2410511036 |
| 5  | [Nama Lengkap Anggota 5] | [NPM Anggota 5] |
| 6  | [Nama Lengkap Anggota 6] | [NPM Anggota 6] |

---

## 🎬 Alur Kerja Aplikasi

Aplikasi ini memiliki alur pengguna yang terdiri dari tiga *scene* utama:

1. **Splash Screen**  
   Tampilan pembuka yang muncul beberapa detik saat aplikasi pertama dijalankan.

2. **Menu Utama**  
   Setelah splash screen, pengguna masuk ke menu utama yang berisi judul aplikasi dan tombol **"Open Camera"**.

3. **Scene AR**  
   Setelah menekan tombol, aplikasi akan beralih ke scene utama, mengaktifkan kamera depan, dan mulai mengirim gambar ke server untuk klasifikasi.  
   Hasil prediksi (label dan skor kepercayaan) akan ditampilkan di layar.

---

## ✨ Fitur Utama & Tambahan

- **Klasifikasi Gender** — Menganalisis gambar dari kamera depan untuk memprediksi gender.  
- **Arsitektur Klien–Server** — Proses inferensi AI dilakukan di server Flask lokal agar performa di perangkat tetap optimal.  
- **Alur UI Lengkap** — Implementasi tiga halaman: Splash Screen → Menu Utama → Scene AR.  
- **Filter Wajah Dinamis (Fitur Tambahan)** — Berdasarkan hasil klasifikasi, aplikasi menampilkan *prefab* atau filter 3D berbeda di wajah pengguna (menggunakan AR Face Manager).

---

## 🛠️ Arsitektur & Teknologi

Proyek dibagi menjadi dua komponen utama: **Frontend (Unity)** dan **Backend (Flask)**.

---

### 🧩 1. Frontend (Unity)

**Engine:** Unity  
**Editor Digunakan:** Unity Editor 6.0  
**Platform Target:** Android  

**Paket Utama:**
- AR Foundation  
- ARCore XR Plugin  

**Struktur Folder Unity:**
```
Assets/
├── Prefabs/        # Prefab filter untuk wajah
├── Resources/
│   ├── Script/     # Semua script C#
│   └── UI/         # Aset gambar UI
└── Scenes/         # Semua file scene
```

**Daftar Scene:**
- `SplashScreenScene.unity`
- `MainMenuScene.unity`
- `MainScene.unity`

---

### ⚙️ 2. Backend (Server Flask Lokal)

**Framework:** Flask (Python)  
**Model:** TensorFlow/Keras (.h5)  

**Fungsi:**  
- Menerima gambar dari Unity  
- Melakukan pra-pemrosesan  
- Menjalankan prediksi dengan model  
- Mengembalikan hasil dalam format JSON  

**Endpoint API:**
- `GET /` → Health check  
- `POST /predict` → Prediksi gender berdasarkan gambar

---

## 💻 Penjelasan Script C# (Frontend)

### **SplashScreenManager.cs**
**Tugas:** Mengatur durasi tampilan splash screen.  
**Cara kerja:** Menggunakan `Invoke()` untuk memanggil `LoadNextScene()` setelah jeda 3 detik agar otomatis berpindah ke menu utama.

---

### **MainMenuManager.cs**
**Tugas:** Menangani interaksi menu utama.  
**Cara kerja:** Fungsi publik `GoToARScene()` dipanggil saat tombol “Open Camera” ditekan. Fungsi ini memuat *scene* utama (MainScene).

---

### **GenderClassifierAR.cs / APIClassifier.cs**
**Tugas:** Komponen utama untuk komunikasi Unity–Server Flask.

**Cara kerja:**
- `Start()` → Inisialisasi ARCameraManager dan alamat server.  
- `OnCameraFrameReceived()` → Menangkap frame kamera, dikirim tiap 1 detik.  
- `UploadAndPredict()` → Coroutine asynchronous untuk mengirim gambar ke server menggunakan `WWWForm`.  
- Setelah respons diterima, hasil JSON di-*parse* untuk menampilkan label dan confidence di UI.

---

## 🚀 Panduan Menjalankan Proyek

Berikut langkah-langkah untuk menjalankan proyek ini dari GitHub.

---

### 🔹 Langkah 1: Clone Repositori

```bash
git clone https://github.com/danristin1718/Face-Tracking-With-Unity.git
```

---

### 🔹 Langkah 2: Menjalankan Backend (Server Flask)

Masuk ke folder server, lalu instal semua dependensi:

```bash
pip install Flask tensorflow Pillow numpy
```

Pastikan file **model.h5** ada di direktori yang sama dengan `server.py`.

---

### 🔹 Langkah 3: Jalankan Server Flask

Gunakan perintah berikut untuk menjalankan server:

```bash
python server.py
```

Setelah berjalan, server akan menampilkan pesan:

```
==============================
Model berhasil dimuat. Server siap menerima permintaan.
==============================
```

Catat **alamat IPv4** komputer Anda dengan perintah berikut (Windows):

```bash
ipconfig
```

---

### 🔹 Langkah 4: Menjalankan Frontend (Unity)

1. Buka Unity Hub → klik **Open** → arahkan ke folder proyek hasil clone.  
2. Buka **MainScene** dari `Assets/Scenes/`.  
3. Pilih **AR Session Origin** di Hierarchy.  
4. Pada script `GenderClassifierAR`, ubah **Server IP** sesuai alamat IPv4 komputer yang menjalankan Flask.  
5. Buka menu:  
   **File > Build Settings...**  
   - Tambahkan ketiga scene (`SplashScreenScene`, `MainMenuScene`, `MainScene`)  
   - Pilih platform **Android**  
   - Klik **Build And Run**

⚠️ Pastikan perangkat Android dan komputer berada di **jaringan WiFi yang sama**.

---

## 🧠 Kode Server (server.py)

```python
from flask import Flask, request, jsonify
import tensorflow as tf
from PIL import Image
import numpy as np
import io

# --- PENGATURAN ---
H5_MODEL_PATH = "model.h5"
INPUT_WIDTH = 64
INPUT_HEIGHT = 64
LABELS = ["Perempuan", "Laki-laki"]

# --- INISIALISASI ---
app = Flask(__name__)
model = tf.keras.models.load_model(H5_MODEL_PATH)
print("="*30)
print("Model berhasil dimuat. Server siap menerima permintaan.")
print("="*30)

# --- FUNGSI PREDIKSI ---
def prepare_image(image_bytes):
    img = Image.open(io.BytesIO(image_bytes))
    if img.mode != "RGB":
        img = img.convert("RGB")
    img = img.resize((INPUT_WIDTH, INPUT_HEIGHT))
    img_array = np.array(img) / 255.0
    img_array = np.expand_dims(img_array, axis=0)
    return img_array

# --- ENDPOINT API ---
@app.route("/predict", methods=["POST"])
def predict():
    if 'file' not in request.files:
        return jsonify({"error": "File tidak ditemukan"}), 400
    
    file = request.files['file']
    img_bytes = file.read()
    prepared_image = prepare_image(img_bytes)
    predictions = model.predict(prepared_image)
    
    score = float(np.max(predictions))
    label_index = int(np.argmax(predictions))
    label = LABELS[label_index]
    
    return jsonify({
        "label": label,
        "confidence": score
    })

@app.route("/", methods=["GET"])
def health_check():
    return jsonify({
        "status": "ok",
        "message": "Server is running and the model is loaded."
    })

# --- MENJALANKAN SERVER ---
if __name__ == "__main__":
    app.run(host='0.0.0.0', port=5000)
```

---

## 🧩 Catatan Tambahan

- Pastikan Flask dan TensorFlow telah terinstal dengan versi kompatibel.  
- Jalankan Flask sebelum membuka aplikasi Unity.  
- Gunakan jaringan WiFi yang sama agar koneksi antar perangkat berhasil.  
- File `model.h5` harus sesuai dengan format input `(64, 64, 3)`.

---
