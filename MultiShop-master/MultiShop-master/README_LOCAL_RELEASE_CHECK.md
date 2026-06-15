# MultiShop Local Release Check

Bu dosya, projeyi yerelde "tek komutla" saglik kontrolunden gecirmek icin kisa kilavuzdur.

## 1) Ön kosullar

- SQL Server `.\SQLEXPRESS` ayakta olmali
- MongoDB `localhost:27017` ayakta olmali
- PostgreSQL container `localhost:5432` ayakta olmali
- Redis container `localhost:6379` ayakta olmali
- Mikroservisler ve WebUI calisiyor olmali (veya `-Mode run/all` ile baslatilmis olmali)

## 2) Tek komut final kontrol

PowerShell:

`powershell -ExecutionPolicy Bypass -File .\Run-MultiShopLocal.ps1 -Mode check -SkipBuild`

Bu komut sirayla:

- Ön kosul kontrolleri
- Altyapi port kontrolleri
- Kritik URL smoke testleri
- Tokenli Ocelot route smoke testleri

## 3) Beklenen basarili cikti

Asagidaki satirlarin tamami `OK` olmali:

- WebUI/Identity/servis swagger endpointleri
- Ocelot route testleri:
  - Catalog
  - Discount
  - Order
  - Cargo
  - Basket
  - Message
  - Comment
  - Payment
  - Images

## 4) Hata olursa hizli aksiyon

- `BAGLANTI YOK`:
  - Ilgili DB/container ayaga kaldir
  - Port cakismasi var mi kontrol et

- `HTTP 401/403` (gateway):
  - Identity ayakta mi kontrol et (`5001`)
  - Token alma adimini tekrar calistir

- `HTTP 404`:
  - Ocelot `UpstreamPathTemplate` ve servis route uyumunu kontrol et

- `HTTP 500`:
  - Ilgili mikroservis logunu incele
  - Son degisiklikten sonra servisi yeniden baslat

## 5) Tam kapanis (opsiyonel manuel tur)

Script yesil olsa bile final guvence icin manuel tur:

- Ana sayfa / kategori / urun detay
- Sepete ekle / sepet goruntule
- Kupon uygula
- Yorum ekleme
- Mesaj akis testi
- Siparis/adres akis testi

Bu turda her aksiyonun ilgili DB etkisi (SQL/Mongo/Postgres/Redis) goruluyorsa yerel kurulum "hazir" kabul edilir.
