# 🎨 بهینه‌سازی رندرینگ

<div align="center">

**کاهش Draw Calls و بهبود عملکرد گرافیکی**

</div>

---

## 📊 Draw Calls چیست؟

```
┌─────────────────────────────────────────────────────────────┐
│                    DRAW CALL FLOW                            │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│   CPU                              GPU                       │
│  ┌─────────┐                    ┌─────────┐                │
│  │ Game    │ ═══ Draw Call ═══▶│ Render  │                │
│  │ Logic   │      Command       │  Mesh   │                │
│  └─────────┘                    └─────────┘                │
│                                                             │
│  هر Draw Call = یک دستور رندر                              │
│  = سربار زیاد روی CPU!                                     │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  Draw Calls           Performance                    │   │
│  │  ────────────         ────────────                   │   │
│  │  < 100               ✅ عالی                         │   │
│  │  100 - 500           🟡 متوسط                        │   │
│  │  500 - 1000          🟠 نیاز به بهینه‌سازی           │   │
│  │  > 1000              🔴 مشکل جدی                     │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### مشاهده در Unity

```
Game View > Stats
├── Batches: تعداد Draw Calls
├── Saved by batching: کاهش به دلیل Batching
├── Tris: تعداد مثلث‌ها
├── Verts: تعداد رأس‌ها
└── SetPass calls: تغییرات Shader/Material
```

---

## 🔗 Static Batching

### مفهوم

```
بدون Batching:
┌───┐ ┌───┐ ┌───┐ ┌───┐
│ A │ │ B │ │ C │ │ D │    = 4 Draw Calls
└───┘ └───┘ └───┘ └───┘

با Static Batching:
┌───────────────────────┐
│    A + B + C + D      │    = 1 Draw Call!
└───────────────────────┘

شرایط:
✅ Static در Inspector
✅ Material یکسان
✅ جابجا نمی‌شوند (Static!)
```

### فعال‌سازی

```
۱. Inspector > GameObject > Static ✓
۲. یا فقط Batching Static ✓

یا در کد:
gameObject.isStatic = true;
```

---

## ⚡ Dynamic Batching

### شرایط

```
برای آبجکت‌های متحرک با:
├── کمتر از 900 vertex attributes
├── Material یکسان
├── Scale یکسان (یا uniform scale)
└── بدون Real-time shadows
```

### فعال‌سازی

```
Project Settings > Player > Other Settings:
Dynamic Batching ✓
```

---

## 🏠 GPU Instancing

### برای تعداد زیاد آبجکت مشابه

```csharp
// مثال: ۱۰۰۰ گلوله
// بدون Instancing: 1000 Draw Calls
// با Instancing: 1 Draw Call!

// در Material:
// Enable GPU Instancing ✓

// در Shader:
#pragma multi_compile_instancing
```

---

## 📦 LOD (Level of Detail)

```
┌─────────────────────────────────────────────────────────────┐
│                       LOD SYSTEM                             │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Distance from Camera:                                      │
│  ◀────────────────────────────────────────────────────────▶│
│   Near                                              Far     │
│                                                             │
│   LOD0         LOD1           LOD2          Culled         │
│  ┌──────┐    ┌──────┐      ┌──────┐        ╳╳╳╳           │
│  │██████│    │▓▓▓▓▓▓│      │░░░░░░│                        │
│  │██████│    │▓▓▓▓▓▓│      │░░░░░░│        نمایش          │
│  │██████│    │▓▓▓▓▓▓│      │░░░░░░│        نمی‌دهد         │
│  └──────┘    └──────┘      └──────┘                        │
│  10000 tri   1000 tri      100 tri         0 tri          │
│                                                             │
│  0m ─────── 20m ─────── 50m ─────── 100m ─────────▶       │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### تنظیم در Unity

```
۱. Add Component > LOD Group
۲. تنظیم LOD Levels و درصدها
۳. Assign meshes به هر LOD
```

---

## 👁️ Occlusion Culling

### مفهوم

```
┌─────────────────────────────────────────────────────────────┐
│                   OCCLUSION CULLING                          │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│    👁️ Camera                                                │
│      │                                                      │
│      │         ┌─────┐                                     │
│      │         │Wall │                    ┌─────┐          │
│      │─────────│     │←NOT RENDERED──────│Behind│          │
│      │         └─────┘                    └─────┘          │
│      │                                                      │
│      ▼                                                      │
│   ┌─────┐                                                  │
│   │Seen │ ← RENDERED                                       │
│   └─────┘                                                  │
│                                                             │
│   💡 پشت دیوار رندر نمی‌شود = صرفه‌جویی!                   │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### تنظیم

```
۱. Window > Rendering > Occlusion Culling
۲. Occluders (دیوارها) > Occluder Static ✓
۳. Occludees (پشت دیوار) > Occludee Static ✓
۴. Bake
```

---

## 💡 Light Baking

### مقایسه

```
Real-time Lights:          Baked Lights:
├── هر فریم محاسبه         ├── یکبار محاسبه (Bake)
├── سنگین                  ├── سبک
├── Shadows پویا           ├── Shadows ثابت
└── Mixed mode             └── Lightmaps
```

### تنظیم برای بازی FPS

```csharp
// در Scene:
// ۱. نورهای ثابت (Sun, Room lights) → Baked
// ۲. نورهای پویا (Muzzle flash) → Real-time

// Lighting Settings:
// Mixed Lighting > Baked Indirect
// Lightmapper > Progressive GPU (سریع‌تر)
```

---

## 🎨 Shader Optimization

### ساده‌سازی Shader

```
پیچیدگی Shader:
├── Unlit             = سبک‌ترین
├── Mobile/Diffuse    = سبک
├── Standard          = سنگین
└── Custom Complex    = سنگین‌ترین
```

### بهترین روش برای Mobile/Low-end

```csharp
// استفاده از Mobile shaders
// Project Settings > Graphics:
// Tier Settings > Low > Use optimized shaders ✓
```

---

## 📸 Texture Optimization

### تنظیمات مهم

```
Texture Import Settings:
├── Max Size: 512 یا 1024 برای غیر اصلی‌ها
├── Compression: Normal Quality
├── Generate Mip Maps: ✓ برای 3D
└── Streaming Mip Maps: ✓ (Unity 2018+)
```

### Texture Atlas

```
بدون Atlas:
[Tex1][Tex2][Tex3][Tex4] = 4 Material = 4 Draw Calls

با Atlas:
┌──────────────────┐
│ Tex1 │ Tex2      │
├──────┼───────────│ = 1 Material = 1 Draw Call!
│ Tex3 │ Tex4      │
└──────────────────┘
```

---

## 📊 Frame Debugger

### استفاده

```
Window > Analysis > Frame Debugger

نمایش می‌دهد:
├── ترتیب Draw Calls
├── چه چیزی رندر می‌شود
├── چه Shader استفاده می‌شود
└── State changes
```

### شناسایی مشکل

```
اگر ببینید:
├── Draw Calls زیاد برای یک object → Missing Batching
├── SetPass calls زیاد → Materials زیاد
└── Clear calls نامناسب → Camera settings
```

---

## 📋 Rendering Checklist

```
□ Draw Calls < 100 (mobile) / < 500 (PC)?
□ Static Batching فعال برای ثابت‌ها?
□ GPU Instancing برای تکراری‌ها?
□ LOD برای مدل‌های پیچیده?
□ Occlusion Culling baked?
□ Lights baked (جز پویاها)?
□ Textures compressed?
□ Mip Maps فعال?
```

---

## 🚀 بخش بعدی

در بخش بعدی، با **بهینه‌سازی شبکه** آشنا می‌شویم.

<div align="center">

**[⏮️ بخش قبلی](./08-memory-optimization.md)** | **[⏭️ بخش بعدی: بهینه‌سازی شبکه](./10-network-optimization.md)**

</div>

---

<div align="center">

*Developed by Amin Davodian*

</div>
