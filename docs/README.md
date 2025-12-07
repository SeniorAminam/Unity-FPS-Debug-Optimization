<div align="center">

# 🎮 Unity Debugging & Optimization

<img src="https://img.shields.io/badge/Unity-2022.3.55f1-000000?style=for-the-badge&logo=unity&logoColor=white" alt="Unity">
<img src="https://img.shields.io/badge/Photon%20PUN2-Multiplayer-004480?style=for-the-badge&logo=photon&logoColor=white" alt="Photon">
<img src="https://img.shields.io/badge/C%23-10.0-239120?style=for-the-badge&logo=csharp&logoColor=white" alt="C#">
<img src="https://img.shields.io/badge/License-MIT-green?style=for-the-badge" alt="License">

<br><br>

**📚 ارائه دانشگاهی: دیباگ و بهینه‌سازی پروژه‌های یونیتی**

*مستندات کامل + اسکریپت‌های دمو لایو*

<br>

[📖 شروع مستندات](./docs/README.md) • [🎮 راهنمای دمو](./docs/DEMO_GUIDE.md) • [👨‍💻 درباره من](https://senioramin.com)

---

</div>

## ✨ ویژگی‌ها

<table>
<tr>
<td width="50%">

### 📚 مستندات کامل
- **۱۳ بخش آموزشی** به زبان فارسی
- کد نمونه از پروژه واقعی
- دیاگرام‌ها و نمودارها
- چک‌لیست‌های عملی

</td>
<td width="50%">

### 🎮 دموی لایو
- **۵ اسکریپت C#** برای نمایش زنده
- Toggle با کلیدهای F1-F8
- نمایش Profiler و Network Stats
- مقایسه Before/After

</td>
</tr>
</table>

---

## 📋 فهرست مستندات

| # | موضوع | توضیحات | لینک |
|:-:|-------|---------|:----:|
| 01 | **مقدمه** | معرفی پروژه و اهداف | [📖](./docs/01-intro.md) |
| 02 | **ساختار پروژه** | بررسی فایل‌ها و کلاس‌ها | [📖](./docs/02-project-structure.md) |
| 03 | **ابزارهای دیباگ** | Debug.Log, Gizmos, Attributes | [📖](./docs/03-debugging-tools.md) |
| 04 | **Console و Profiler** | CPU, Memory, Spikes | [📖](./docs/04-console-profiler.md) |
| 05 | **دیباگ شبکه** | Photon PUN2 و RPC | [📖](./docs/05-debugging-network.md) |
| 06 | **باگ‌های رایج** | NullRef, Race Conditions | [📖](./docs/06-common-bugs.md) |
| 07 | **اصول بهینه‌سازی** | قانون 80/20, Budgeting | [📖](./docs/07-optimization-basics.md) |
| 08 | **بهینه‌سازی حافظه** | Object Pooling, GC | [📖](./docs/08-memory-optimization.md) |
| 09 | **بهینه‌سازی رندرینگ** | Draw Calls, LOD, Batching | [📖](./docs/09-rendering-optimization.md) |
| 10 | **بهینه‌سازی شبکه** | Lag Compensation, Sync | [📖](./docs/10-network-optimization.md) |
| 11 | **بهینه‌سازی کد** | Caching, Update Methods | [📖](./docs/11-code-optimization.md) |
| 12 | **دمو لایو** | سناریوهای نمایش | [📖](./docs/12-live-demo.md) |
| 13 | **جمع‌بندی** | نتیجه‌گیری و منابع | [📖](./docs/13-summary.md) |

---

## ⌨️ کلیدهای میانبر دمو

<div align="center">

| کلید | عملکرد | توضیح |
|:----:|--------|-------|
| `F1` | 🎯 Raycast Debug | نمایش خطوط تیراندازی |
| `F2` | 🌐 Network Stats | Ping, Packets, Room |
| `F3` | 📊 Performance | FPS, Memory, GC Graph |
| `F4` | ♻️ Object Pooling | Pool vs Instantiate |
| `F5` | 🐛 Null Bug | شبیه‌سازی NullRef |
| `F6` | 🗑️ GC Spikes | ایجاد Garbage |
| `F7` | ⚡ SqrMagnitude | بهینه‌سازی Distance |
| `F8` | 💾 Cache Screen | Screen Center Cache |

</div>

---

## 🚀 راه‌اندازی سریع

### ۱. Clone کردن
```bash
git clone https://github.com/SeniorAminam/Unity-FPS-Debug-Optimization.git
```

### ۲. راه‌اندازی دمو در Unity
```
1️⃣ در Hierarchy: Create Empty → نام: "DemoManager"
2️⃣ Add Components:
   ├── DebugDemoManager
   ├── ObjectPoolDemo
   ├── NetworkStatsOverlay
   └── PerformanceMonitor
3️⃣ Play و از F1-F8 استفاده کنید!
```

---

## 📊 آمار پروژه

<div align="center">

| نوع | تعداد |
|:---:|:-----:|
| 📄 فایل مستند | **15** |
| 💻 اسکریپت دمو | **5** |
| ⌨️ کلید میانبر | **8** |
| 📝 خط کد | **~6500** |

</div>

---

## 🛠️ تکنولوژی‌ها

<div align="center">

<img src="https://skillicons.dev/icons?i=unity,cs,visualstudio,git,github" alt="Tech Stack">

</div>

- **Unity 2022.3.55f1 LTS** - Game Engine
- **Photon PUN2** - Multiplayer Networking
- **C# 10** - Scripting Language
- **Visual Studio** - IDE

---

## 👨‍💻 ارائه‌دهنده

<div align="center">

<img src="https://avatars.githubusercontent.com/SeniorAminam" width="100" style="border-radius: 50%">

### امین داودیان
**Amin Davodian**

[![Website](https://img.shields.io/badge/Website-senioramin.com-blue?style=flat-square&logo=google-chrome&logoColor=white)](https://senioramin.com)
[![GitHub](https://img.shields.io/badge/GitHub-SeniorAminam-181717?style=flat-square&logo=github&logoColor=white)](https://github.com/SeniorAminam)
[![LinkedIn](https://img.shields.io/badge/LinkedIn-SudoAmin-0A66C2?style=flat-square&logo=linkedin&logoColor=white)](https://linkedin.com/in/SudoAmin)

</div>

---

## 📜 لایسنس

این پروژه تحت لایسنس **MIT** منتشر شده است.

---

<div align="center">

**⭐ اگر این پروژه مفید بود، یک Star بزنید!**

</div>
