# Mor Robot - Dost Takipçi Sistemi 🤖💜

## 📋 Açıklama
Mor robot artık sizin **dost arkadaşınız**! Nereye giderseniz gidin, sizi takip eder. 

### ✨ Özellikler:
- ✅ Oyuncuyu otomatik bulur ve takip eder
- ✅ Saldırmaz (dost bir takipçi)
- ✅ Hasar almaz (ölmez)
- ✅ Her zaman yanınızda (mesafe sınırı yok)
- ✅ NavMesh kullanarak akıllıca hareket eder
- ✅ Animasyon desteği (Walk, Speed parametreleri)
- ✅ Hafif ve performanslı

---

## 🎮 Unity'de Kurulum

### Adım 1: Mor Robot Prefab'ını Sahneye Ekleyin
1. `Assets/UntrackedFiles/mor_robot_1213121521_texture_fbx@T-Pose.fbx` dosyasını bulun
2. Sahneye (Hierarchy) sürükleyin
3. Mor robot objesini seçin

### Adım 2: NavMeshAgent Component'i Ekleyin
1. Inspector'da **Add Component** butonuna tıklayın
2. **NavMeshAgent** yazın ve ekleyin
3. NavMeshAgent ayarları:
   - **Radius**: 0.5
   - **Height**: 2
   - **Base Offset**: 0
   - **Speed**: 3.5

### Adım 3: EnemyFollower Script'ini Ekleyin
1. **Add Component** butonuna tıklayın
2. **EnemyFollower** yazın ve ekleyin
3. Script ayarları:
   - **Follow Distance**: 3 (oyuncuya bu kadar yaklaşır)
   - **Max Follow Distance**: 100 (bu mesafeden uzaksa takip etmez - ama Always Follow açıksa önemli değil)
   - **Move Speed**: 3.5 (hareket hızı)
   - **Rotation Speed**: 5 (dönüş hızı)
   - **Always Follow**: ✅ (her zaman takip et)
   - **Is Friendly**: ✅ (dost robot)

### Adım 4: NavMesh Oluşturun
1. **Window** > **AI** > **Navigation** menüsünden Navigation penceresini açın
2. **Bake** sekmesine gidin
3. **Bake** butonuna tıklayın
4. Zemin mavi renkte görünecek (NavMesh oluşturuldu demektir)

### Adım 5: Player Tag'ini Ayarlayın
1. Player objenizi seçin (karakteriniz)
2. Inspector'da üstte **Tag** dropdown'ını bulun
3. **Player** seçin
4. Eğer Player tag'i yoksa:
   - **Add Tag** tıklayın
   - **+** butonuna basın
   - "Player" yazın ve Save edin
   - Tekrar Player objesine gidip Tag'i Player yapın

---

## ✅ Tamamlandı!

Artık oyunu başlattığınızda:
- 🤖 Mor robot sizi otomatik bulacak
- 💜 Nereye giderseniz gitsin, peşinizden gelecek
- 🚶 Yaklaşınca duracak (3 metre mesafede)
- 👀 Size doğru bakacak
- ❌ Saldırmayacak (dost!)
- 🛡️ Hasar almayacak (ölmez)

---

## 🎨 Animasyon Ayarları (Opsiyonel)

Eğer mor robotun animasyonları varsa:

### Animator Controller Oluşturma:
1. **Project** penceresinde sağ tık
2. **Create** > **Animator Controller**
3. İsim verin: `MorRobotAnimator`
4. Mor robot objesine **Animator** component'i ekleyin
5. Oluşturduğunuz Animator Controller'ı atayın

### Gerekli Animasyon Parametreleri:
- **Walk** (Bool) - Yürüme animasyonu için
- **Speed** (Float) - Hız bazlı animasyon için

Eğer animasyonlarınız yoksa, script yine de çalışır (sadece hareket eder).

---

## ⚙️ Ayarlar Açıklaması

### Follow Distance (Takip Mesafesi)
- Mor robot oyuncuya bu kadar yaklaşır
- Varsayılan: **3 metre**
- Daha yakın olsun isterseniz: **1-2 metre**
- Daha uzak olsun isterseniz: **5-10 metre**

### Move Speed (Hareket Hızı)
- Mor robotun yürüme hızı
- Varsayılan: **3.5**
- Daha hızlı: **5-7**
- Daha yavaş: **2-3**

### Always Follow (Her Zaman Takip Et)
- ✅ Açık: Mor robot HER ZAMAN sizi takip eder (mesafe sınırı yok)
- ❌ Kapalı: Sadece Max Follow Distance içindeyken takip eder

### Is Friendly (Dost mu?)
- ✅ Açık: Dost robot (hasar almaz, saldırmaz)
- ❌ Kapalı: Normal düşman gibi davranır

---

## 🔧 Sorun Giderme

### ❌ Mor robot hareket etmiyor?
**Çözüm:**
1. ✅ NavMesh oluşturuldu mu? (Window > AI > Navigation > Bake)
2. ✅ NavMeshAgent component'i var mı?
3. ✅ Zemin NavMesh ile kaplanmış mı? (mavi renkte görünmeli)
4. ✅ Mor robot zeminin üzerinde mi? (havada değil)

### ❌ Mor robot oyuncuyu bulamıyor?
**Çözüm:**
1. ✅ Player objesi "Player" tag'ine sahip mi?
2. ✅ Console'da hata var mı? (Window > General > Console)
3. ✅ Player objesi sahnede aktif mi?

### ❌ Mor robot havada kalıyor?
**Çözüm:**
1. ✅ NavMeshAgent'ın **Base Offset** değerini 0 yapın
2. ✅ Mor robotun pozisyonunu zemine indirin
3. ✅ Collider'ı kontrol edin

### ❌ Mor robot çok yavaş/hızlı?
**Çözüm:**
1. ✅ **Move Speed** değerini ayarlayın (3.5 varsayılan)
2. ✅ NavMeshAgent'ın **Speed** değerini kontrol edin

---

## 🎯 Script'ten Kontrol Etme (İleri Seviye)

Eğer kod yazarak mor robotu kontrol etmek isterseniz:

```csharp
// Mor robot referansını al
EnemyFollower morRobot = GameObject.Find("MorRobot").GetComponent<EnemyFollower>();

// Takip mesafesini değiştir
morRobot.SetFollowDistance(5f); // 5 metre

// Hızını değiştir
morRobot.SetMoveSpeed(7f); // Daha hızlı

// Farklı bir hedef belirle
morRobot.SetTarget(başkaObje.transform);

// Başlangıç pozisyonuna geri dön
morRobot.ReturnToStart();
```

---

## 💡 İpuçları

1. **Mor robot çok yakın geliyorsa**: Follow Distance'ı artırın (örn: 5)
2. **Mor robot çok uzakta kalıyorsa**: Follow Distance'ı azaltın (örn: 2)
3. **Mor robot yavaş kalıyorsa**: Move Speed'i artırın (örn: 5)
4. **Birden fazla mor robot**: Aynı script'i birden fazla robota ekleyebilirsiniz!

---

## 🎮 Oyun İçi Davranış

Mor robot şöyle davranır:
1. 🔍 Oyun başladığında Player'ı bulur
2. 🏃 Oyuncuya doğru koşar
3. 🛑 Follow Distance kadar yaklaşınca durur
4. 👀 Size doğru bakar
5. 🚶 Siz hareket edince tekrar takip eder
6. 💚 Asla saldırmaz (dost!)
7. 🛡️ Asla ölmez (hasar almaz)

---

**Başarılar!** 🎮💜

Artık mor robot sizin sadık arkadaşınız! Nereye giderseniz gitsin, yanınızda olacak! 🤖✨

