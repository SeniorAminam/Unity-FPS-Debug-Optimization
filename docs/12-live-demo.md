# 🎮 دمو و نمایش عملی

<div align="center">

**نمایش زنده دیباگ و بهینه‌سازی در Unity**

</div>

---

## 🚀 راه‌اندازی پروژه

### مراحل

```
۱. باز کردن پروژه در Unity 2022.3.55f1
۲. باز کردن Scene اصلی: Assets/Scenes/Main.unity
۳. تنظیم Photon AppId (اگر نیاز بود)
۴. Play!
```

### بررسی اولیه

```csharp
// در Console باید ببینید:
// [NetworkManager] Starting connection...
// [NetworkManager] Connected to Master!
// [NetworkManager] Joined lobby

// اگر خطا دیدید:
// ✅ بررسی اینترنت
// ✅ بررسی Photon AppId
// ✅ بررسی Firewall
```

---

## 📊 نمایش Profiler در حین بازی

### باز کردن Profiler

```
Window > Analysis > Profiler (Ctrl+7)
```

### مراحل دمو

```
┌─────────────────────────────────────────────────────────────┐
│                    PROFILER DEMO                             │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ۱. شروع بازی در Editor                                   │
│     └── Play Mode                                          │
│                                                             │
│  ۲. Join به یک اتاق                                        │
│     └── مشاهده CPU Usage در Profiler                      │
│                                                             │
│  ۳. شروع تیراندازی                                        │
│     └── مشاهده Spikes هنگام Instantiate                   │
│                                                             │
│  ۴. بررسی Memory                                           │
│     └── مشاهده GC Allocation                              │
│                                                             │
│  ۵. بررسی Network Stats                                    │
│     └── Photon Stats Window                                │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 🐛 شناسایی باگ به صورت لایو

### سناریو ۱: NullReferenceException

```csharp
// در Console:
// NullReferenceException: Object reference not set to an instance...

// مراحل دیباگ لایو:
// ۱. روی خطا دابل‌کلیک → باز شدن فایل
// ۲. بررسی خط مشکل‌دار
// ۳. در Inspector بررسی Reference
// ۴. اگر null است → Assign کردن

// مثال:
// PlayerHealth.cs line 56
// damageImage = null!
// → در Prefab Player، DamageImage را assign کنید
```

### سناریو ۲: GC Spike

```csharp
// در Profiler:
// GC.Collect spike دیده می‌شود

// مراحل:
// ۱. روی Spike کلیک
// ۲. در Hierarchy پیدا کردن منبع Allocation
// ۳. معمولاً: PhotonNetwork.Instantiate
// ۴. راه‌حل: Object Pooling
```

---

## 🔧 اعمال بهینه‌سازی Object Pooling

### قبل از بهینه‌سازی

```
در Profiler مشاهده کنید:
├── FpsGun.Shoot() → GC Alloc: 128B
├── PhotonNetwork.Instantiate → Time: 0.9ms
└── هر تیر = یک Allocation
```

### مراحل اعمال Pool

```
۱. ایجاد ImpactPoolManager GameObject در Scene
۲. Add کردن ImpactPoolManager script
۳. تنظیم Impact Prefabs در Inspector:
   ├── impactFlesh
   ├── impactWood
   ├── impactMetal
   ├── impactConcrete
   └── impactWater

۴. تغییر FpsGun.cs:
   // قبل:
   PhotonNetwork.Instantiate("impactFlesh", ...);
   
   // بعد:
   ImpactPoolManager.Instance.Spawn("impactFlesh", ...);
```

### بعد از بهینه‌سازی

```
در Profiler:
├── FpsGun.Shoot() → GC Alloc: 0B ✅
├── ImpactPool.Spawn() → Time: 0.1ms ✅
└── بدون Allocation!
```

---

## 📈 مقایسه قبل و بعد

### جدول مقایسه

```
┌────────────────────────────────────────────────────────────┐
│                 BEFORE vs AFTER                             │
├────────────────────────────────────────────────────────────┤
│                                                            │
│  Metric              │ Before     │ After      │ Improve  │
│  ────────────────────┼────────────┼────────────┼────────  │
│  Frame Time (avg)    │ 18.5ms     │ 14.2ms     │ -23%     │
│  GC Alloc/Frame      │ 2.3 KB     │ 128 B      │ -94%     │
│  GC Spikes/minute    │ 12         │ 2          │ -83%     │
│  Shoot() Time        │ 2.1ms      │ 0.4ms      │ -81%     │
│  Memory Usage        │ 256 MB     │ 198 MB     │ -23%     │
│                                                            │
└────────────────────────────────────────────────────────────┘
```

### نمودار FPS

```
FPS Over Time:

Before:
60 ┤                    ▼Spike
   │  ╭─╮  ╭─╮  ╭─╮   │     ╭─╮  ╭─╮
30 ┤  │ │  │ │  │ │   └─╮   │ │  │ │
   │  ╰─╯  ╰─╯  ╰─╯     ╰───╯ ╰──╯ │
 0 ┼──────────────────────────────────▶ Time

After:
60 ┤  ────────────────────────────────
   │
30 ┤
   │
 0 ┼──────────────────────────────────▶ Time

پایدار و بدون Spike!
```

---

## 📝 اسکریپت دمو

### مراحل صحبت کردن

```markdown
## اسلاید ۱: مقدمه (۱ دقیقه)
"سلام، امروز می‌خواهیم دیباگ و بهینه‌سازی را در یک پروژه 
واقعی Unity به صورت عملی ببینیم."

## اسلاید ۲: بازی (۲ دقیقه)
*شروع بازی*
"این یک FPS Multiplayer با Photon است. بیایید یک اتاق 
بسازیم و ببینیم چه اتفاقی می‌افتد..."

## اسلاید ۳: Profiler (۳ دقیقه)
*باز کردن Profiler*
"ببینید اینجا Spike داریم. هر بار که تیر می‌زنیم، 
یک GC Allocation رخ می‌دهد..."

## اسلاید ۴: مشکل (۲ دقیقه)
*نمایش کد*
"مشکل اینجاست: PhotonNetwork.Instantiate هر بار یک 
GameObject جدید می‌سازد..."

## اسلاید ۵: راه‌حل (۳ دقیقه)
*اعمال Object Pool*
"با Object Pooling، آبجکت‌ها را از قبل می‌سازیم و 
استفاده مجدد می‌کنیم..."

## اسلاید ۶: نتیجه (۲ دقیقه)
*مقایسه Profiler*
"ببینید! GC Allocation صفر شد و Spike ها از بین رفتند!"

## اسلاید ۷: جمع‌بندی (۱ دقیقه)
"نکته کلیدی: همیشه اول اندازه بگیرید، بعد بهینه کنید."
```

---

## ✨ نکات ارائه

```
✅ DO:
├── Profiler را باز نگه دارید
├── Console را Clear کنید قبل از دمو
├── کد تغییرات را از قبل آماده داشته باشید
├── سناریو را تمرین کنید
└── برای سوالات آماده باشید

❌ DON'T:
├── کد را live ننویسید (احتمال خطا!)
├── Build نکنید (زمان‌بر است)
├── بیش از حد جزئیات ندهید
└── هول نشوید اگر Crash شد!
```

---

## 🚀 بخش بعدی

در بخش بعدی، **جمع‌بندی** و منابع را می‌بینیم.

<div align="center">

**[⏮️ بخش قبلی](./11-code-optimization.md)** | **[⏭️ بخش بعدی: جمع‌بندی](./13-summary.md)**

</div>

---

<div align="center">

*Developed by Amin Davodian*

</div>
