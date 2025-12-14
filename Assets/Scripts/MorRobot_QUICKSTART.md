# 🤖💜 Mor Robot - Hızlı Başlangıç Kılavuzu

## 🎯 Ne Yaptık?

Mor robot artık sizin **dost arkadaşınız**! 
- ✅ Sizi takip eder
- ✅ Saldırmaz
- ✅ Siz de ona saldıramazsınız
- ✅ Ölmez (hasar almaz)

---

## ⚡ 5 Adımda Kurulum

### 1️⃣ Mor Robotu Sahneye Ekle
- `Assets/UntrackedFiles/mor_robot_1213121521_texture_fbx@T-Pose.fbx`
- Sahneye sürükle

### 2️⃣ NavMeshAgent Ekle
- Mor robot seçili → **Add Component** → **NavMeshAgent**
- Ayarlar: Radius=0.5, Height=2, Speed=3.5

### 3️⃣ EnemyFollower Script'i Ekle
- **Add Component** → **EnemyFollower**
- Ayarlar: Follow Distance=3, Always Follow=✅

### 4️⃣ NavMesh Oluştur
- **Window** → **AI** → **Navigation** → **Bake**

### 5️⃣ Player Tag'i Ayarla
- Player objesini seç → Tag: **Player**

---

## ✅ Tamamlandı!

Oyunu başlat ve mor robotun sizi takip ettiğini gör! 🎮

**Detaylı bilgi için:** `MorRobot_AI_README.md` dosyasına bak.

---

## 🎮 Davranış

- 🏃 Sizi takip eder
- 🛑 3 metre mesafede durur
- 👀 Size doğru bakar
- 💜 Asla saldırmaz
- 🛡️ Asla ölmez

---

**İyi oyunlar!** 🤖✨
