# 🧠 بهینه‌سازی حافظه

<div align="center">

**مدیریت هوشمند Memory و جلوگیری از Garbage Collection**

</div>

---

## 🗑️ Garbage Collection چیست؟

```
┌─────────────────────────────────────────────────────────────┐
│                  GARBAGE COLLECTION                          │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  HEAP MEMORY:                                               │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ [Object1] [Object2] [    ] [Object3] [      ]        │  │
│  │     ▲         ▲               ▲                      │  │
│  │     │         │               │                      │  │
│  │  In Use    In Use          In Use                    │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                             │
│  بعد از GC.Collect():                                      │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ [Object1] [Object2] [Object3] [    FREE SPACE     ]  │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                             │
│  ⚠️ مشکل: GC باعث Spike و Stutter می‌شود!                │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### GC Spike در Profiler

```
Frame Time:
         GC.Collect!
            ↓
──────────────────────────────────────
     ╭─╮ ╭─╮ ╭─────────╮ ╭─╮ ╭─╮
     │ │ │ │ │         │ │ │ │ │
     ╰─╯ ╰─╯ │         │ ╰─╯ ╰─╯
             │  50ms!  │
             ╰─────────╯

→ بازیکن Stutter/Lag می‌بیند
```

---

## 🔄 Object Pooling

### مشکل در پروژه فعلی

```csharp
// 📍 Assets/Scripts/FpsGun.cs - خط 80-84

void Shoot() {
    // ...
    switch (hitTag) {
        case "Player":
            // ❌ هر بار Instantiate!
            PhotonNetwork.Instantiate("impactFlesh", ...);
            break;
        default:
            // ❌ هر بار Instantiate!
            PhotonNetwork.Instantiate("impact" + hitTag, ...);
            break;
    }
}

// → هر تیر = ۱ Allocation
// → ۱۰۰ تیر = ۱۰۰ Allocation = GC Spike!
```

### راه‌حل: پیاده‌سازی Object Pool

```csharp
// 📍 Assets/Scripts/Utilities/ObjectPool.cs

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic Object Pool for reusing GameObjects
/// </summary>
public class ObjectPool<T> where T : Component {
    
    private readonly T prefab;
    private readonly Queue<T> available = new Queue<T>();
    private readonly HashSet<T> inUse = new HashSet<T>();
    private readonly Transform parent;
    
    public ObjectPool(T prefab, int initialSize = 10, Transform parent = null) {
        this.prefab = prefab;
        this.parent = parent;
        
        // Pre-warm: ایجاد آبجکت‌ها از قبل
        for (int i = 0; i < initialSize; i++) {
            CreateNew();
        }
    }
    
    private T CreateNew() {
        T obj = Object.Instantiate(prefab, parent);
        obj.gameObject.SetActive(false);
        available.Enqueue(obj);
        return obj;
    }
    
    public T Get() {
        if (available.Count == 0) {
            CreateNew();
        }
        
        T obj = available.Dequeue();
        inUse.Add(obj);
        obj.gameObject.SetActive(true);
        return obj;
    }
    
    public void Return(T obj) {
        if (!inUse.Contains(obj)) {
            Debug.LogWarning("[ObjectPool] Returning object that wasn't from pool!");
            return;
        }
        
        obj.gameObject.SetActive(false);
        inUse.Remove(obj);
        available.Enqueue(obj);
    }
    
    public void Clear() {
        foreach (var obj in available) {
            Object.Destroy(obj.gameObject);
        }
        foreach (var obj in inUse) {
            Object.Destroy(obj.gameObject);
        }
        available.Clear();
        inUse.Clear();
    }
}
```

### پیاده‌سازی Impact Pool Manager

```csharp
// 📍 Assets/Scripts/Utilities/ImpactPoolManager.cs

using System.Collections.Generic;
using UnityEngine;

public class ImpactPoolManager : MonoBehaviour {
    
    public static ImpactPoolManager Instance { get; private set; }
    
    [System.Serializable]
    public class ImpactPrefab {
        public string name;
        public ParticleSystem prefab;
        public int poolSize = 20;
    }
    
    [SerializeField] private ImpactPrefab[] impactPrefabs;
    
    private Dictionary<string, ObjectPool<ParticleSystem>> pools;
    
    void Awake() {
        if (Instance == null) {
            Instance = this;
            InitializePools();
        } else {
            Destroy(gameObject);
        }
    }
    
    void InitializePools() {
        pools = new Dictionary<string, ObjectPool<ParticleSystem>>();
        
        foreach (var impact in impactPrefabs) {
            var pool = new ObjectPool<ParticleSystem>(
                impact.prefab, 
                impact.poolSize, 
                transform
            );
            pools[impact.name] = pool;
        }
        
        Debug.Log($"[ImpactPool] Initialized {pools.Count} pools");
    }
    
    public ParticleSystem Spawn(string impactName, Vector3 position, Quaternion rotation) {
        if (!pools.TryGetValue(impactName, out var pool)) {
            Debug.LogError($"[ImpactPool] Unknown impact: {impactName}");
            return null;
        }
        
        var impact = pool.Get();
        impact.transform.SetPositionAndRotation(position, rotation);
        impact.Play();
        
        // Auto-return after particle finishes
        StartCoroutine(ReturnAfterDelay(impactName, impact, impact.main.duration));
        
        return impact;
    }
    
    private System.Collections.IEnumerator ReturnAfterDelay(
        string name, ParticleSystem ps, float delay) {
        
        yield return new WaitForSeconds(delay);
        pools[name].Return(ps);
    }
}
```

### اصلاح FpsGun.cs

```csharp
// 📍 Assets/Scripts/FpsGun.cs - نسخه بهینه

void Shoot() {
    // ...
    if (Physics.Raycast(shootRay, out shootHit, weaponRange, 
        LayerMask.GetMask("Shootable"))) {
        
        switch (shootHit.transform.gameObject.tag) {
            case "Player":
                shootHit.collider.GetComponent<PhotonView>()
                    .RPC("TakeDamage", RpcTarget.All, damagePerShot, 
                         PhotonNetwork.LocalPlayer.NickName);
                
                // ✅ استفاده از Pool به جای Instantiate
                ImpactPoolManager.Instance.Spawn(
                    "impactFlesh", 
                    shootHit.point, 
                    Quaternion.Euler(shootHit.normal.x - 90, shootHit.normal.y, 
                                     shootHit.normal.z)
                );
                break;
            default:
                // ✅ استفاده از Pool
                ImpactPoolManager.Instance.Spawn(
                    "impact" + hitTag,
                    shootHit.point,
                    Quaternion.Euler(shootHit.normal.x - 90, shootHit.normal.y, 
                                     shootHit.normal.z)
                );
                break;
        }
    }
}
```

---

## 📝 String و GC Allocation

### مشکل

```csharp
// ❌ هر بار String جدید ایجاد می‌شود!
void Update() {
    string status = "HP: " + currentHealth + "/" + maxHealth;  // GC!
    statusText.text = status;
}

// → "HP: " + 100 = "HP: 100" (new string!)
// → "HP: 100" + "/" = "HP: 100/" (new string!)
// → "HP: 100/" + 100 = "HP: 100/100" (new string!)
// = 3 string allocations per frame!
```

### راه‌حل: StringBuilder

```csharp
// ✅ استفاده از StringBuilder
using System.Text;

public class PlayerHUD : MonoBehaviour {
    
    private StringBuilder sb = new StringBuilder(64);  // Pre-allocate
    private int lastHealth = -1;
    
    void Update() {
        // فقط اگر تغییر کرد آپدیت کن!
        if (currentHealth != lastHealth) {
            lastHealth = currentHealth;
            
            sb.Clear();
            sb.Append("HP: ");
            sb.Append(currentHealth);
            sb.Append("/");
            sb.Append(maxHealth);
            
            statusText.text = sb.ToString();
        }
    }
}
```

### String Interpolation (C# 10+)

```csharp
// در C# 10 با .NET 6+ بهتر است:
// (Unity 2021.2+ با .NET Standard 2.1)

// اما در Unity معمولی، StringBuilder بهترین است
```

---

## 🔒 Avoid Allocations in Update

### مثال‌های رایج

```csharp
// ❌ BAD: Allocation در هر فریم
void Update() {
    // LINQ allocation
    var enemies = FindObjectsOfType<Enemy>().Where(e => e.IsAlive);
    
    // Array allocation
    var hits = Physics.RaycastAll(ray, distance);
    
    // Lambda closure allocation
    enemies.ForEach(e => e.TakeDamage(10));
    
    // Boxing allocation
    object health = currentHealth;  // int → object = boxing!
}

// ✅ GOOD: No allocation
private Enemy[] cachedEnemies;
private RaycastHit[] hitBuffer = new RaycastHit[10];
private int aliveCount;

void Start() {
    cachedEnemies = FindObjectsOfType<Enemy>();
}

void Update() {
    // استفاده از array cache شده
    aliveCount = 0;
    for (int i = 0; i < cachedEnemies.Length; i++) {
        if (cachedEnemies[i].IsAlive) {
            cachedEnemies[i].DoSomething();
            aliveCount++;
        }
    }
    
    // استفاده از NonAlloc
    int hitCount = Physics.RaycastNonAlloc(ray, hitBuffer, distance);
    for (int i = 0; i < hitCount; i++) {
        ProcessHit(hitBuffer[i]);
    }
}
```

---

## 🎯 Coroutine Optimization

### مشکل

```csharp
// ❌ هر بار WaitForSeconds جدید!
IEnumerator DisableShootingEffect() {
    yield return new WaitForSeconds(0.05f);  // GC Allocation!
    gunLine.enabled = false;
}
```

### راه‌حل: Cache Yields

```csharp
// ✅ Cache WaitForSeconds
public class FpsGun : MonoBehaviour {
    
    // Cache در field
    private WaitForSeconds disableDelay;
    private WaitForSeconds reloadDelay;
    private WaitForEndOfFrame endOfFrame;
    
    void Awake() {
        disableDelay = new WaitForSeconds(0.05f);
        reloadDelay = new WaitForSeconds(2f);
        endOfFrame = new WaitForEndOfFrame();
    }
    
    IEnumerator DisableShootingEffect() {
        yield return disableDelay;  // No allocation!
        gunLine.enabled = false;
    }
}
```

### جایگزین: Invoke/InvokeRepeating

```csharp
// برای delay ساده، Invoke کافی است
void Shoot() {
    gunLine.enabled = true;
    Invoke(nameof(DisableGunLine), 0.05f);  // No coroutine!
}

void DisableGunLine() {
    gunLine.enabled = false;
}

// یا CancelInvoke اگر لازم باشد
void StopShooting() {
    CancelInvoke(nameof(DisableGunLine));
}
```

---

## 📊 Memory Profiler Tips

### چک لیست GC در Profiler

```
در CPU Profiler:
├── GC Alloc > 0 در Update ها؟ ← ❌ باید 0 باشد
├── GC.Collect calls زیاد؟
└── Scripts با allocation بالا؟

Memory Profiler:
├── Total Allocated growing؟
├── Duplicated textures/meshes؟
└── Leaked objects؟
```

### نمایش GC در Game

```csharp
// Debug UI برای نمایش GC

public class GCMonitor : MonoBehaviour {
    
    [SerializeField] private Text gcText;
    
    private int lastGCCount;
    private float lastGCTime;
    
    void Update() {
        int gcCount = System.GC.CollectionCount(0);
        
        if (gcCount > lastGCCount) {
            lastGCCount = gcCount;
            lastGCTime = Time.time;
            
            if (gcText != null) {
                gcText.text = $"GC #{gcCount} at {Time.time:F1}s";
                gcText.color = Color.red;
            }
        } else if (Time.time - lastGCTime > 1f) {
            if (gcText != null) {
                gcText.color = Color.white;
            }
        }
    }
}
```

---

## 📋 Memory Checklist

```
□ Object Pooling برای spawn/destroy مکرر
□ StringBuilder برای string concat
□ Cache WaitForSeconds
□ No LINQ در Update
□ No FindObjectsOfType در Update  
□ NonAlloc variants برای Physics
□ No boxing (int → object)
□ GC Alloc = 0 در Profiler
```

---

## 🚀 بخش بعدی

در بخش بعدی، با **بهینه‌سازی رندرینگ** آشنا می‌شویم.

<div align="center">

**[⏮️ بخش قبلی](./07-optimization-basics.md)** | **[⏭️ بخش بعدی: بهینه‌سازی رندرینگ](./09-rendering-optimization.md)**

</div>

---

<div align="center">

*Developed by Amin Davodian*

</div>
