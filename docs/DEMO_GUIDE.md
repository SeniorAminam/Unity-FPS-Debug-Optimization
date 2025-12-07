# 🎮 راهنمای دموی لایو

<div align="center">

**راه‌اندازی و استفاده از سیستم دمو در Unity**

</div>

---

## ⚡ راه‌اندازی سریع

### ۱. ایجاد DemoManager در Scene

```
۱. در Hierarchy کلیک راست کنید
۲. Create Empty → نام: "DemoManager"
۳. این کامپوننت‌ها را اضافه کنید:
   ├── DebugDemoManager
   ├── ObjectPoolDemo
   ├── NetworkStatsOverlay
   └── PerformanceMonitor
```

### ۲. شروع بازی

```
۱. Play را بزنید
۲. یک اتاق بسازید یا Join کنید
۳. حالا میتونید با کلیدها Toggle کنید!
```

---

## ⌨️ کلیدهای میانبر

| کلید | عملکرد | نمایش |
|:----:|--------|-------|
| **F1** | Raycast Debug | خطوط قرمز/سبز تیراندازی |
| **F2** | Network Stats | Ping, Packets, Room Info |
| **F3** | Performance | FPS, Memory, GC Graph |
| **F4** | Object Pooling | Toggle Pool vs Instantiate |
| **F5** | Null Bug | شبیه‌سازی NullReferenceException |
| **F6** | GC Spikes | ایجاد GC برای Profiler |
| **F7** | SqrMagnitude | Toggle Distance Optimization |
| **F8** | Cache Screen | Toggle Screen Center Cache |
| **F12** | Hide/Show Panel | مخفی/نمایش پنل دیباگ |

---

## 🎬 سناریوهای دمو

### 🐛 دمو ۱: نمایش Raycast Debug

```
موضوع: ابزارهای دیباگ (03-debugging-tools.md)

۱. بازی را شروع کنید
۲. F1 بزنید (Raycast Debug ON)
۳. شلیک کنید
۴. در Scene View خط قرمز/سبز را نشان دهید:
   - قرمز = miss
   - سبز = hit
   - آبی = normal surface
۵. توضیح دهید: Debug.DrawRay چطور کار می‌کند
```

### 📊 دمو ۲: Profiler و GC

```
موضوع: Console و Profiler (04-console-profiler.md)

۱. Profiler را باز کنید (Ctrl+7)
۲. F3 بزنید (Performance Stats ON)
۳. F6 بزنید (GC Spikes ON)
۴. در Profiler نشان دهید:
   - Spikes در CPU Usage
   - GC.Collect calls
۵. F6 را خاموش کنید
۶. مقایسه: spike ها از بین رفتند!
```

### 🌐 دمو ۳: دیباگ شبکه

```
موضوع: دیباگ شبکه (05-debugging-network.md)

۱. به بازی Join کنید
۲. F2 بزنید (Network Stats ON)
۳. نشان دهید:
   - Ping (رنگ سبز/زرد/قرمز)
   - Room name و Player count
   - Sent/Received packets
۴. توضیح: چطور Lag را شناسایی کنیم
```

### 💥 دمو ۴: NullReferenceException

```
موضوع: باگ‌های رایج (06-common-bugs.md)

۱. Console را باز کنید
۲. F5 بزنید (Null Bug ON)
۳. خطای قرمز در Console نشان دهید
۴. توضیح:
   - چرا NullReferenceException اتفاق افتاد
   - چطور جلوگیری کنیم (null check)
   - استفاده از TryGetComponent
```

### ♻️ دمو ۵: Object Pooling

```
موضوع: بهینه‌سازی حافظه (08-memory-optimization.md)

۱. Profiler باز باشد
۲. F4 بزنید (Object Pooling OFF)
۳. چند بار شلیک کنید
۴. در Profiler نشان دهید:
   - GC Allocation بالا
   - Instantiate calls
۵. حالا F4 بزنید (Object Pooling ON)
۶. دوباره شلیک کنید
۷. مقایسه:
   - GC Allocation: 0
   - "Got from pool" در Console
```

### ⚡ دمو ۶: Distance Optimization

```
موضوع: بهینه‌سازی کد (11-code-optimization.md)

۱. F7 بزنید (SqrMagnitude ON)
۲. توضیح دهید:
   - Vector3.Distance شامل Sqrt است
   - sqrMagnitude سریع‌تر است
۳. F8 بزنید (Cache Screen ON)
۴. توضیح:
   - new Vector3 هر فریم = GC
   - Cache = بدون GC
```

---

## 📋 چک‌لیست قبل از ارائه

```
□ DemoManager در Scene اضافه شده؟
□ همه کامپوننت‌ها attach شده‌اند؟
□ Profiler قابل دسترسی است؟
□ Console پاک است؟ (GC warnings دیگه نیست ✅)
□ Network connection برقرار است؟
□ کلیدها تست شده‌اند؟
□ F12 برای مخفی کردن پنل کار می‌کند؟
```

---

## 🗣️ صحبت‌های پیشنهادی

### هنگام Toggle کردن

```
"ببینید الان که F1 رو می‌زنم..."
"توجه کنید به Console/Profiler..."
"مقایسه کنید قبل و بعد..."
"این همون چیزیه که در مستندات توضیح دادیم..."
```

### هنگام نشان دادن مشکل

```
"این یه مثال از مشکلی هست که..."
"در پروژه‌های واقعی، این باعث می‌شه..."
"راه‌حل این مشکل اینه که..."
```

### بعد از بهینه‌سازی

```
"ببینید که با این تغییر ساده..."
"GC Allocation صفر شد..."
"FPS پایدارتر شد..."
```

---

## 📂 فایل‌های Demo

```
Assets/Scripts/Demo/
├── DebugDemoManager.cs      ← مدیریت اصلی + F1-F8
├── ObjectPoolDemo.cs        ← سیستم Object Pool
├── FpsGunDemoExtension.cs   ← دیباگ Raycast
├── NetworkStatsOverlay.cs   ← آمار Photon
└── PerformanceMonitor.cs    ← FPS/Memory Graph
```

---

<div align="center">

**موفق باشید! 🚀**

*Developed by Amin Davodian*

</div>
