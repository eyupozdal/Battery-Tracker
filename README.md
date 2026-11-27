# Pil Takip

[English below](#english)

Bluetooth ile bağlı kablosuz cihazlarının (fare, klavye, kulaklık...) şarj yüzdesini görev çubuğunda gösteren küçük bir Windows uygulaması. Her seferinde Ayarlar'a girip bakmak yerine tepsiye bakıyorsun, o kadar.

Hangi cihazların görüneceğini kendin seçiyorsun. Seçtiğin her cihaz için tepsede ayrı bir ikon çıkıyor ve üzerinde yüzdesi yazıyor. Şarj azaldıkça ikon yeşilden turuncuya, sonra kırmızıya dönüyor, istersen düşük pil uyarısı da veriyor.

## Kurulum

[Releases](../../releases) sayfasından en son `PilTakip-Setup-x.y.z.exe` dosyasını indirip çalıştır. Yönetici izni istemiyor.

Kurulum dosyası imzalı değil, o yüzden Windows SmartScreen "bilgisayarınızı korudu" diyebilir. "Ek bilgi" ve ardından "Yine de çalıştır" ile geçebilirsin. İstemezsen kaynak kodu burada, aşağıdaki gibi kendin de derleyebilirsin.

## Nasıl çalışıyor

Windows, Bluetooth cihazların pil bilgisini zaten tutuyor (Ayarlar > Bluetooth ve cihazlar'da gördüğün yüzde). Uygulama sadece bunu okuyor. Bu yüzden:

- Sadece Bluetooth ile eşleşmiş cihazlar çalışıyor. 2.4 GHz dongle ile bağlananlar şimdilik yok.
- Windows pil yüzdesini göstermiyorsa cihaz listede de çıkmaz.
- İnternete bağlanmıyor, yönetici izni istemiyor. Ayarlar `%AppData%\PilTakip\settings.json` dosyasında duruyor.
- Windows 11'de yeni tepsi ikonları önce ^ menüsünde bekliyor. Oradan sürükleyip görev çubuğuna bırakırsan hep görünür.

## Kendin derlemek istersen

Bilgisayarında [.NET 8 SDK](https://dotnet.microsoft.com/download) ve [Inno Setup 6](https://jrsoftware.org/isdl.php) kurulu olsun, sonra `build.bat` dosyasına çift tıkla. Kurulum dosyası `src\bin\installer` klasörüne çıkar.

## Lisans

GPL-3.0

---

## English

A small Windows app that shows the battery level of your Bluetooth devices (mouse, keyboard, headphones...) right in the taskbar. No more digging through Settings every time.

You pick which devices to show. Each selected device gets its own tray icon with the percentage on it. The icon goes from green to orange to red as the battery drains, and you can get a low battery warning too.

### Install

Grab the latest `PilTakip-Setup-x.y.z.exe` from the [Releases](../../releases) page and run it. It doesn't need admin rights.

The installer isn't code-signed, so Windows SmartScreen may complain. Click "More info" and then "Run anyway". If you'd rather not, the source is right here and you can build it yourself.

### How it works

Windows already keeps track of the battery level of Bluetooth devices (it's the percentage you see in Settings > Bluetooth & devices). The app just reads that. Which means:

- Only devices paired over Bluetooth work. Ones that use a 2.4 GHz dongle aren't supported for now.
- If Windows doesn't show a battery percentage for a device, it won't show up in the list either.
- It doesn't connect to the internet or need admin rights. Settings live in `%AppData%\PilTakip\settings.json`.
- On Windows 11, new tray icons start out in the ^ overflow menu. Drag them out to the taskbar to keep them visible.

### Building it yourself

Install the [.NET 8 SDK](https://dotnet.microsoft.com/download) and [Inno Setup 6](https://jrsoftware.org/isdl.php), then double-click `build.bat`. The installer ends up in `src\bin\installer`.

### License

GPL-3.0
