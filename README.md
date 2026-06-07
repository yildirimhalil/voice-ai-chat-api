# Voice AI Chat API

.NET 8 ile yazılmış basit bir sohbet Web API'sidir. Tek bir endpoint (`POST /api/chat`) üzerinden gelen mesaja yanıt döner. Yanıt iki şekilde üretilebilir:

- **Static** (varsayılan): Dışarıya bağımlılığı olmayan, anahtar kelime tabanlı sabit yanıtlar. Hiçbir kurulum gerektirmeden çalışır.
- **Ollama**: Lokalde çalışan ücretsiz bir LLM (örn. `llama3.2`) üzerinden yanıt üretir. Ollama'ya erişilemezse otomatik olarak static moda düşer.

## Gereksinimler

- .NET 8 SDK
- (Opsiyonel) Ollama — yalnızca AI modunu denemek isterseniz

## Çalıştırmak için

```bash
cd VoiceAiChatApi
dotnet run
```

Uygulama varsayılan olarak `http://localhost:5098` adresinde ayağa kalkar. Development ortamında Swagger arayüzü `http://localhost:5098/swagger` altında açılır.

## API

### `POST /api/chat`

İstek:

```json
{
  "message": "Merhaba"
}
```

Yanıt:

```json
{
  "response": "Merhaba, size nasıl yardımcı olabilirim?"
}
```

`message` boş gönderilirse `400 Bad Request` döner.

### Test

`curl` ile:

```bash
curl -X POST http://localhost:5098/api/chat \
  -H "Content-Type: application/json" \
  -d '{"message":"Merhaba"}'
```

Alternatifler: Swagger UI, Postman veya repo içindeki `VoiceAiChatApi/VoiceAiChatApi.http` dosyası (VS Code REST Client / Rider).

## Yapay zeka modunu açma

`VoiceAiChatApi/appsettings.json` içinde:

```json
"Chat": {
  "Provider": "Ollama",
  "Ollama": {
    "Endpoint": "http://localhost:11434",
    "Model": "llama3.2"
  }
}
```

Öncesinde:

```bash
ollama pull llama3.2
ollama serve
```

Model erişilemezse istek hata vermez; static yanıt mekanizmasına geri düşer.

## Proje yapısı

```
VoiceAiChatApi/
  Controllers/ChatController.cs     HTTP katmanı, doğrulama
  Models/                           İstek/yanıt modelleri
  Services/
    IChatService.cs / ChatService.cs  Sağlayıcı seçimi + fallback
    StaticResponder.cs              Anahtar kelime tabanlı yanıtlar
    OllamaResponder.cs              Ollama HTTP istemcisi
    ChatOptions.cs                  Yapılandırma
```

---

# Dokümantasyon

## 1. Asterisk Entegrasyonu

Asterisk telefon trafiğini yönetir, bu API ise yalnızca metin sohbet mantığını sağlar. İkisini birbirine bağlayan parça Asterisk dialplan'i ve onun çağırdığı bir aracı script/uygulamadır.

**Çağrının karşılanması:** Gelen çağrı dialplan'de bir extension'a düşer. `Answer()` ile çağrı açılır, ardından kullanıcıya karşılama anonsu çalınır ve konuşması beklenir.

**API'nin çağrılması:** İki yaygın yol var:

- **AGI/EAGI** veya daha modern olarak **ARI (Asterisk REST Interface)**. ARI tercih edilir: Asterisk WebSocket üzerinden olayları (StasisStart, kullanıcı konuşması bitti vb.) dışarıdaki bir uygulamaya bildirir. Bu uygulama önce konuşmayı STT ile metne çevirir, metni bu API'nin `POST /api/chat` ucuna gönderir, dönen `response` metnini alır.
- Hızlı bir prototip için dialplan'den `Shell`/`AGI` script'i ile `curl` çağrısı da yapılabilir, ancak üretimde ARI daha sağlıklıdır.

**Sesin oynatılması:** API'den dönen metin TTS ile ses dosyasına (genelde 8 kHz, mono, `wav`/`gsm`) dönüştürülür ve ARI `play` komutu (`channel.play(media=sound:...)`) ya da dialplan'de `Playback()`/`Stream` ile kullanıcıya çalınır. Akış genel olarak: **çağrı açılır → kullanıcı konuşur → STT → /api/chat → TTS → ses çalınır → döngü tekrar eder.**

## 2. STT (Speech To Text)

Türkçe konuşma çözümü gerektiği için ilk tercihim **OpenAI Whisper** (lokal `faster-whisper` ile) olurdu.

- Türkçe doğruluğu yüksek, telefon hattının gürültülü/8 kHz sesinde diğer açık kaynak çözümlere göre belirgin şekilde daha iyi.
- Lokal/offline çalışabilir, çağrı verisi dışarı çıkmaz (KVKK açısından önemli).
- Lisans maliyeti yok; `faster-whisper` ile CPU'da bile makul gecikmeyle çalışır, GPU'da gerçek zamanlıya yakındır.

Daha düşük gecikme kritikse **Vosk** alternatif olabilir (hafif, streaming, anında kısmi sonuç), ancak doğruluğu Whisper'ın gerisindedir. Bulut tarafında Google Speech-to-Text Türkçede güçlüdür fakat maliyet ve veri gizliliği dezavantajı taşır.

## 3. TTS (Text To Speech)

**Coqui TTS / Piper** tercihim olurdu.

- **Piper** hızlı, hafif ve lokal çalışır; düşük gecikme telefon görüşmesinde kritik olduğu için öne çıkar. Türkçe ses modelleri mevcuttur.
- Lokal olması veri gizliliğini korur ve çağrı başına maliyet oluşturmaz.
- Çıktıyı Asterisk'in beklediği formata (8 kHz mono) dönüştürmek kolaydır.

Daha doğal/insansı ses isteniyorsa **ElevenLabs** veya **Azure Neural TTS** kalite olarak öndedir, ancak bunlar bulut bağımlılığı ve kullanım başına ücret getirir. Maliyet/gizlilik öncelikse lokal Piper, ses kalitesi öncelikse bulut Neural TTS arasında seçim yapardım.

## 4. Yapay Zeka

Lokal ve ücretsiz çözüm olarak **Ollama** üzerinde çalışan bir model (örn. **Llama 3.2** veya Türkçe için ince ayarlı bir model) tercih ederdim.

- Tek komutla kurulup OpenAI uyumlu/REST bir API sunar; entegrasyonu basittir (bu projede de bu şekilde bağladım).
- Tamamen lokal çalışır: çağrı içerikleri dışarı gitmez, kullanım ücreti yoktur.
- Model boyutu donanıma göre seçilebilir; küçük modeller CPU'da bile kabul edilebilir gecikme verir.

Kalite ihtiyacı yükseldiğinde aynı arayüzü koruyarak daha büyük bir modele ya da bir bulut sağlayıcısına (OpenAI/Anthropic) geçmek kolaydır. Bu yüzden kodda yanıt üretimini `IChatService` arkasında soyutladım; sağlayıcı değişimi tek bir yapılandırma/uygulama değişikliğiyle yapılır.

## 5. Test Süreci

Uygulamayı sıfırdan kuracak biri için adımlar:

**1. Proje nasıl çalıştırılır**

```bash
# .NET 8 SDK kurulu olmalı
git clone <repo-url>
cd voice-ai-chat-api/VoiceAiChatApi
dotnet run
```

Uygulama `http://localhost:5098` üzerinde çalışır.

**2. API nasıl test edilir**

- `curl`:
  ```bash
  curl -X POST http://localhost:5098/api/chat \
    -H "Content-Type: application/json" \
    -d '{"message":"Merhaba"}'
  ```
- Swagger UI: `http://localhost:5098/swagger`
- Postman veya `VoiceAiChatApi.http` dosyası

**3. Asterisk ile nasıl test edilir**

- Asterisk'i lokalde Docker ile ayağa kaldır.
- Bir SIP/PJSIP extension tanımla ve yazılım telefonu (**Zoiper**, **Linphone**) ile bağlan.
- Dialplan'de test edilen extension'ı, konuşmayı STT'ye → `/api/chat`'e → TTS'e yönlendiren aracı script'e (ARI uygulaması) bağla.
- Yazılım telefonundan ara, konuş ve dönen sesi dinleyerek uçtan uca akışı doğrula.
- İzole test için önce STT ve TTS adımlarını atlayıp dialplan'den doğrudan API'ye `curl` çağrısı yaparak entegrasyonun çalıştığını görmek faydalıdır.

**4. Hangi araçlar kullanılır**

| Amaç | Araç |
|------|------|
| API geliştirme/çalıştırma | .NET 8 SDK |
| API testi | curl, Postman, Swagger UI |
| Telefon istemcisi | Zoiper, Linphone |
| Telefon altyapısı | Asterisk (Docker) |
| STT | Whisper / faster-whisper |
| TTS | Piper / Coqui TTS |
| Yapay zeka | Ollama (Llama 3.2) |
