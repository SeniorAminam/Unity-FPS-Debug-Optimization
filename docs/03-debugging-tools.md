# 🔧 ابزارهای دیباگ در یونیتی

<div align="center">

**شناخت و استفاده از ابزارهای داخلی Unity برای دیباگ**

</div>

---

## 📋 فهرست ابزارها

```
🔧 Unity Debugging Tools
│
├── 📝 Debug Class
│   ├── Debug.Log()
│   ├── Debug.LogWarning()
│   ├── Debug.LogError()
│   ├── Debug.DrawRay()
│   └── Debug.DrawLine()
│
├── 🎨 Gizmos
│   ├── OnDrawGizmos()
│   └── OnDrawGizmosSelected()
│
├── ⚙️ Attributes
│   ├── [Header]
│   ├── [Tooltip]
│   ├── [SerializeField]
│   └── [HideInInspector]
│
└── 🔀 Conditional Compilation
    ├── #if UNITY_EDITOR
    └── [System.Diagnostics.Conditional]
```

---

## 📝 Debug Class

### Debug.Log - لاگ ساده

```csharp
// ✅ استفاده صحیح
Debug.Log($"Player {playerName} joined room {roomName}");

// ✅ با Context - کلیک روی لاگ، آبجکت را انتخاب می‌کند
Debug.Log("Player spawned", gameObject);

// ❌ استفاده نادرست - اطلاعات کم
Debug.Log("here");
Debug.Log("test");
```

### Debug.LogWarning - هشدار

```csharp
// زمان استفاده: وقتی چیزی غیرعادی است اما بازی ادامه دارد
void TakeDamage(int amount, string enemyName) {
    if (isDead) {
        Debug.LogWarning($"[PlayerHealth] TakeDamage called on dead player: {name}");
        return;
    }
    
    if (amount <= 0) {
        Debug.LogWarning($"[PlayerHealth] Invalid damage amount: {amount}");
        return;
    }
    
    currentHealth -= amount;
}
```

### Debug.LogError - خطا

```csharp
// زمان استفاده: وقتی خطای جدی رخ داده
void Start() {
    playerCamera = GetComponent<Camera>();
    
    if (playerCamera == null) {
        Debug.LogError($"[{GetType().Name}] Camera component missing on {name}!");
        enabled = false;  // غیرفعال کردن اسکریپت
        return;
    }
}
```

### 📊 مقایسه انواع Log

| نوع | رنگ | استفاده | عملکرد |
|-----|-----|---------|--------|
| `Log` | ⚪ سفید | اطلاعات عمومی | سریع |
| `LogWarning` | 🟡 زرد | هشدارها | متوسط |
| `LogError` | 🔴 قرمز | خطاها | کند (Stack Trace) |
| `LogException` | 🔴 قرمز | استثناها | کندترین |

---

## 🎯 Debug.DrawRay و Debug.DrawLine

### برای دیباگ Raycast در FpsGun

```csharp
// 📍 مسیر: Assets/Scripts/FpsGun.cs
// اضافه کردن به متد Shoot()

void Shoot() {
    timer = 0.0f;
    
    Ray shootRay = raycastCamera.ScreenPointToRay(
        new Vector3(Screen.width/2, Screen.height/2, 0f)
    );
    
    // 🔍 DEBUG: رسم خط Raycast در Scene View
    #if UNITY_EDITOR
    Debug.DrawRay(
        shootRay.origin, 
        shootRay.direction * weaponRange, 
        Color.red, 
        0.5f  // مدت نمایش (ثانیه)
    );
    #endif
    
    if (Physics.Raycast(shootRay, out shootHit, weaponRange, 
        LayerMask.GetMask("Shootable"))) {
        
        // 🔍 DEBUG: نقطه برخورد
        #if UNITY_EDITOR
        Debug.DrawLine(
            shootHit.point, 
            shootHit.point + shootHit.normal * 2f, 
            Color.green, 
            1f
        );
        Debug.Log($"[FpsGun] Hit: {shootHit.transform.name} at {shootHit.point}");
        #endif
        
        // ... بقیه کد
    }
}
```

### نتیجه در Scene View

```
┌─────────────────────────────────────────────┐
│                SCENE VIEW                    │
│                                             │
│      Player ─────────────────────▶ Enemy    │
│        │         خط قرمز Ray          │     │
│        │                              │     │
│        │                         ────┼──    │
│                                 خط سبز     │
│                                 Normal      │
│                                             │
└─────────────────────────────────────────────┘
```

---

## 🎨 Gizmos - تجسم در Editor

### OnDrawGizmos vs OnDrawGizmosSelected

```csharp
// 📍 مثال: تجسم Spawn Points در NetworkManager

public class NetworkManager : MonoBehaviourPunCallbacks {
    
    [SerializeField]
    private Transform[] spawnPoints;
    
    // ✅ همیشه نمایش داده می‌شود
    void OnDrawGizmos() {
        if (spawnPoints == null) return;
        
        Gizmos.color = Color.cyan;
        foreach (var point in spawnPoints) {
            if (point != null) {
                Gizmos.DrawWireSphere(point.position, 0.5f);
            }
        }
    }
    
    // ✅ فقط وقتی آبجکت انتخاب شده نمایش داده می‌شود
    void OnDrawGizmosSelected() {
        if (spawnPoints == null) return;
        
        Gizmos.color = Color.green;
        foreach (var point in spawnPoints) {
            if (point != null) {
                // رسم خط از مرکز به Spawn Point
                Gizmos.DrawLine(transform.position, point.position);
                
                // رسم جهت Forward
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(point.position, point.forward * 2f);
            }
        }
    }
}
```

### Gizmos Methods پرکاربرد

```csharp
// 🔵 اشکال ساده
Gizmos.DrawSphere(position, radius);          // کره توپر
Gizmos.DrawWireSphere(position, radius);      // کره توخالی
Gizmos.DrawCube(position, size);              // مکعب توپر
Gizmos.DrawWireCube(position, size);          // مکعب توخالی

// 📏 خطوط
Gizmos.DrawLine(from, to);                    // خط
Gizmos.DrawRay(origin, direction);            // پرتو

// 🎨 تنظیمات
Gizmos.color = Color.red;                     // رنگ
Gizmos.matrix = transform.localToWorldMatrix; // Transform
```

---

## ⚙️ Attributes برای دیباگ بهتر

### [Header] - عنوان در Inspector

```csharp
public class FpsGun : MonoBehaviour {
    
    [Header("🔫 Weapon Settings")]
    [SerializeField] private int damagePerShot = 20;
    [SerializeField] private float timeBetweenBullets = 0.2f;
    [SerializeField] private float weaponRange = 100.0f;
    
    [Header("🎮 References")]
    [SerializeField] private TpsGun tpsGun;
    [SerializeField] private ParticleSystem gunParticles;
    
    [Header("🔊 Audio")]
    [SerializeField] private AudioSource gunAudio;
    [SerializeField] private AudioClip shootClip;
}
```

### [Tooltip] - راهنما

```csharp
[Header("Player Settings")]
[Tooltip("سلامت شروع بازیکن. مقدار پیشنهادی: 100")]
[SerializeField] private int startingHealth = 100;

[Tooltip("سرعت غرق شدن بعد از مرگ. مقدار کمتر = آهسته‌تر")]
[Range(0.01f, 1f)]
[SerializeField] private float sinkSpeed = 0.12f;

[Tooltip("زمان انتظار قبل از Respawn (ثانیه)")]
[Range(1f, 30f)]
[SerializeField] private float respawnTime = 8.0f;
```

### [SerializeField] vs public

```csharp
// ❌ نادرست: همه فیلدها public
public class BadExample : MonoBehaviour {
    public int health;           // قابل دسترسی از همه جا!
    public Transform target;     // Encapsulation نقض شده!
}

// ✅ صحیح: استفاده از SerializeField
public class GoodExample : MonoBehaviour {
    [SerializeField] 
    private int health;          // فقط در Inspector قابل تنظیم
    
    [SerializeField] 
    private Transform target;    // Encapsulation حفظ شده
    
    // Property برای دسترسی کنترل‌شده
    public int Health => health;
}
```

### [HideInInspector] - مخفی کردن

```csharp
public class NameTag : MonoBehaviourPunCallbacks {
    
    [HideInInspector]  // Public اما مخفی در Inspector
    public Transform target = null;
    
    [SerializeField]
    private Text nameText;
}
```

---

## 🔀 Conditional Compilation

### #if UNITY_EDITOR

```csharp
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DebugExample : MonoBehaviour {
    
    void Update() {
        // این کد فقط در Editor اجرا می‌شود
        #if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F1)) {
            Debug.Log("Debug Info:");
            Debug.Log($"  Position: {transform.position}");
            Debug.Log($"  Rotation: {transform.rotation.eulerAngles}");
            Debug.Log($"  FPS: {1f / Time.deltaTime:F1}");
        }
        #endif
    }
    
    #if UNITY_EDITOR
    void OnDrawGizmos() {
        Handles.Label(transform.position + Vector3.up * 2, name);
    }
    #endif
}
```

### [System.Diagnostics.Conditional]

```csharp
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class Logger {
    
    // این متد در Build حذف می‌شود (نه فقط غیرفعال!)
    [Conditional("UNITY_EDITOR")]
    public static void EditorLog(string message) {
        Debug.Log($"[EDITOR] {message}");
    }
    
    [Conditional("DEBUG_MODE")]
    public static void DebugLog(string message) {
        Debug.Log($"[DEBUG] {message}");
    }
}

// استفاده:
public class Player : MonoBehaviour {
    void Start() {
        Logger.EditorLog("Player initialized");  // در Build وجود ندارد!
    }
}
```

### Symbols تعریف‌شده در Unity

| Symbol | شرط |
|--------|-----|
| `UNITY_EDITOR` | در Editor |
| `UNITY_STANDALONE` | Windows/Mac/Linux |
| `UNITY_ANDROID` | Android |
| `UNITY_IOS` | iOS |
| `UNITY_WEBGL` | WebGL |
| `DEVELOPMENT_BUILD` | Development Build |
| `DEBUG` | Debug Configuration |

---

## 💡 Best Practices

### ۱. استفاده از Prefix در لاگ‌ها

```csharp
// ✅ پیشنهاد: استفاده از نام کلاس
Debug.Log($"[{GetType().Name}] Player spawned at {spawnPoint}");

// خروجی: [NetworkManager] Player spawned at (10, 0, 5)
```

### ۲. Log Levels ثابت

```csharp
public static class GameLogger {
    
    public static void Info(string category, string message) {
        Debug.Log($"[{category}] {message}");
    }
    
    public static void Warning(string category, string message) {
        Debug.LogWarning($"[{category}] ⚠️ {message}");
    }
    
    public static void Error(string category, string message) {
        Debug.LogError($"[{category}] ❌ {message}");
    }
}

// استفاده:
GameLogger.Info("Network", "Connected to server");
GameLogger.Warning("Combat", "Player took damage while invincible");
GameLogger.Error("Audio", "Sound clip is missing!");
```

### ۳. حذف لاگ‌ها در Build

```csharp
// 📍 Project Settings > Player > Scripting Define Symbols
// اضافه کردن: DISABLE_LOGS

public static class GameLogger {
    
    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    public static void Log(string message) {
        #if !DISABLE_LOGS
        Debug.Log(message);
        #endif
    }
}
```

---

## 🚀 بخش بعدی

در بخش بعدی، با **Console و Profiler** یونیتی آشنا می‌شویم.

<div align="center">

**[⏮️ بخش قبلی](./02-project-structure.md)** | **[⏭️ بخش بعدی: Console و Profiler](./04-console-profiler.md)**

</div>

---

<div align="center">

*Developed by Amin Davodian*

</div>
