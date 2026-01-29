# 10TH LIGHTYEAR
> **Elysian Might** ekibi tarafından geliştirilen 3D Action-RPG / Hack and Slash oyunu.

##  Proje Hakkında ve Geliştirme Süreci
Bu proje, **BALIKESIR TEKNOKENT/GAME JAM II** tarafından düzenlenen yoğun oyun geliştirme eğitimi sonrasında ortaya çıkmıştır.
Eğitim sürecinde edindiğimiz teorik bilgileri (Unity motoru, C# mimarisi, Oyun Tasarımı), 3 kişilik ekibimizle bir araya gelerek pratiğe döktük.

Amacımız, takım çalışması (collaborative work) ile sürdürülebilir bir kod yapısı kurmak ve keyifli bir oynanış deneyimi sunmaktır.
---

##  Geliştirici Ekip

Proje sürecinde görev dağılımımız uzmanlık alanlarımıza göre şu şekilde yapılmıştır:

* **Yiğit Erdoğdu**
  * **Rol:** Gameplay Developer & Level Designer (Oyun Mekanikleri ve Bölüm Tasarımı)
  * **Sorumluluk:** Karakter hareket sistemleri, dövüş mekaniklerinin kodlanması ve oyun sahnelerinin (Levels) inşası.

* **Tülin Eren**
  * **Rol:** UI Developer (Kullanıcı Arayüzü Geliştiricisi)
  * **Sorumluluk ve Teknik Uygulamalar:**
    * **Responsive Tasarım:** Canvas Scaler konfigürasyonu ile farklı ekran çözünürlüklerine (Aspect Ratios) tam uyumlu arayüz mimarisi.
    * **Event Yönetimi:** Unity Event System kullanılarak buton ve menü etkileşimlerinin kodlanması.
    * **Görsel Optimizasyon:** TextMeshPro entegrasyonu ile yüksek performanslı metin render işlemleri.
    * **Sahne Yönetimi:** SceneManager API kullanılarak sahneler arası (Menü -> Oyun) asenkron ve akıcı geçişlerin sağlanması.

* **Burak Sarpkaya**
  * **Rol:** Visual Artist & Asset Manager (Görsel Tasarım ve Varlık Yönetimi)
  * **Sorumluluk:** Karakter görünümleri, çevre modellerinin seçimi/entegrasyonu.
  
> **Unity ile geliştirilmiş, **3D Action-RPG / Hack and Slash** projesi.**

# 🎮 [OYNA!](https://yigiterdogdu.itch.io/10th-lightyear) / (https://tulineren.itch.io/10th-light-year)

##  Ekran Görüntüleri
 ![SS_1](https://github.com/user-attachments/assets/687aa2aa-521b-4b6f-9ffd-9ae32f9ed41e)
 ![SS_2](https://github.com/user-attachments/assets/3ec53f1f-c7e3-4b65-81aa-4d89f072c50b)
 ![SS_3](https://github.com/user-attachments/assets/9f433995-64c4-4524-93e5-4f9d893f48cd)
 ![SS_4](https://github.com/user-attachments/assets/569c06a4-3335-4c5d-9d71-08206e3693dd)
 ![SS_5](https://github.com/user-attachments/assets/9a74bf29-b1dc-4899-8117-e8f4ed3e6ba7)
 ![SS_5](https://github.com/user-attachments/assets/292c4ac1-83c7-4623-b37a-9500e9779442)

---
##  Teknik Detaylar (Geliştirici Notları)
Proje geliştirilirken **Nesne Yönelimli Programlama (OOP)** prensiplerine sadık kalınmıştır.

##  Proje Hakkında (Hikaye)
Oyun, **Semi-Open World (Yarı Açık Dünya)** yapısına sahip bir hapishane kompleksinde geçmektedir.

Ana karakterimiz, işlemediği bir suç yüzünden krallığın en acımasız zindanına hapsedilmiş bir savaşçıdır.
Hapishane yönetiminin sunduğu tek bir çıkış yolu vardır: **Arena.** 
Karakterin özgürlüğünü kazanabilmesi için ölümcül turnuvaya katılması, rakiplerini yenmesi ve hayatta kalması gerekmektedir.

> *"Sadece en güçlüler zincirlerini kırabilir..."*

###  Öne Çıkan Özellikler
* **Refleks Odaklı Oynanış:** Hızlı tepki süresi ve zamanlama gerektiren dinamik yapı.
* **Arena Sistemi:** Zorlu Boss savaşları ve dalga tabanlı düşman sistemi.
* **Dövüş Mekanikleri:** Kombo tabanlı saldırı sistemi ve düşman yapay zekası (AI).
* **Atmosfer:** Karanlık zindan teması ve yarı açık dünya keşif alanları.
---
##  Kontroller

| Eylem | Tuş Kombinasyonu |
| :--- | :--- |
| **Hareket** | W, A, S, D / Ok Tuşları |
| **Etkileşim** | E |
| **Saldırı** | Sol Fare Tuşu |

---

##  Dosya Yapısı ve Kurulum
Bu depo (repository), oyunun **Unity Kaynak Kodlarını** içerir.
* `Assets/Scripts`: Tüm C# kodları bu klasördedir.
* `Assets/Scenes`: Oyun sahneleri buradadır.
* `Assets/Prefabs`: Hazır oyun objeleri.

**Projeyi İncelemek İçin:**
1. Bu repoyu klonlayın.
2. Unity Hub üzerinden projeyi açın (Unity Sürümü: Unity 6.2(6000.2.14f1).
3. `Scenes` klasöründeki `MainMenu` sahnesini başlatın.

---

##  Teşekkür (Acknowledgments)
Bu projeyi hayata geçirmemizde büyük emeği olan, bize oyun geliştirme vizyonu katan **BALIKESIR TEKNOKENT** ailesine ve değerli eğitmenlerimize; teknik rehberlikleri ve destekleri için sonsuz teşekkürlerimizi sunarız.

---
*Bu proje eğitim amaçlı geliştirilmiştir.*
