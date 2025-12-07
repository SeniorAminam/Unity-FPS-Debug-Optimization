# 🐛 باگ‌های رایج و روش‌های شناسایی

<div align="center">

**شناسایی، تحلیل و رفع مشکلات متداول در پروژه‌های Unity**

</div>

---

## 🔴 NullReferenceException

### شایع‌ترین خطا در Unity!

```
NullReferenceException: Object reference not set to an instance of an object
PlayerHealth.TakeDamage (Int32 amount, String enemyName) (at Assets/Scripts/PlayerHealth.cs:91)
```

### چرا رخ می‌دهد؟

```csharp
// ❌ مثال: Reference تنظیم نشده در Inspector
public class PlayerHealth : MonoBehaviour {
    [SerializeField] private Slider healthSlider;  // اگر ست نشود = NULL!
    
    void Start() {
        healthSlider.value = 100;  // 💥 NullReferenceException!
    }
}
```

### روش‌های جلوگیری

```csharp
// ✅ روش 1: Null Check
void Start() {
    if (healthSlider != null) {
        healthSlider.value = 100;
    } else {
        Debug.LogError($"[PlayerHealth] healthSlider is not assigned on {name}!");
    }
}

// ✅ روش 2: TryGetComponent
void Start() {
    if (TryGetComponent<Slider>(out var slider)) {
        slider.value = 100;
    } else {
        Debug.LogError($"[PlayerHealth] Slider component missing on {name}!");
    }
}

// ✅ روش 3: RequireComponent
[RequireComponent(typeof(Rigidbody))]
public class PlayerHealth : MonoBehaviour {
    private Rigidbody rb;  // تضمین وجود!
    
    void Awake() {
        rb = GetComponent<Rigidbody>();  // همیشه موجود
    }
}

// ✅ روش 4: Null-Conditional Operator (C# 6+)
void Update() {
    healthSlider?.SetValueWithoutNotify(currentHealth);
}
```

### در این پروژه

```csharp
// 📍 Assets/Scripts/PlayerHealth.cs - خط 56-57
// مشکل احتمالی:

void Start() {
    // این خطوط می‌توانند NullReference بدهند!
    damageImage = GameObject.FindGameObjectWithTag("Screen")
        .transform.Find("DamageImage")
        .GetComponent<Image>();
    
    healthSlider = GameObject.FindGameObjectWithTag("Screen")
        .GetComponentInChildren<Slider>();
}

// ✅ نسخه اصلاح‌شده:
void Start() {
    var screen = GameObject.FindGameObjectWithTag("Screen");
    if (screen == null) {
        Debug.LogError("[PlayerHealth] Screen object not found!");
        enabled = false;
        return;
    }
    
    var damageTransform = screen.transform.Find("DamageImage");
    if (damageTransform != null) {
        damageImage = damageTransform.GetComponent<Image>();
    }
    
    healthSlider = screen.GetComponentInChildren<Slider>();
    if (healthSlider == null) {
        Debug.LogWarning("[PlayerHealth] Health slider not found!");
    }
}
```

---

## 📦 Missing Prefab / Reference

### شناسایی در Console

```
The referenced script on this Behaviour (Game Object 'Player') is missing!
Missing Prefab with guid: abc123456...
```

### علل رایج

```
۱. حذف فایل Script از پروژه
۲. تغییر نام Script/Class
۳. Meta file مشکل دارد
۴. Prefab خراب شده
```

### روش رفع

```csharp
// ابزار Editor برای پیدا کردن Missing Scripts

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class MissingScriptFinder : EditorWindow {
    
    [MenuItem("Tools/Find Missing Scripts")]
    static void FindMissing() {
        int count = 0;
        
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>()) {
            foreach (var component in go.GetComponents<Component>()) {
                if (component == null) {
                    Debug.LogWarning($"Missing script on: {GetFullPath(go)}", go);
                    count++;
                }
            }
        }
        
        Debug.Log($"Found {count} missing scripts.");
    }
    
    static string GetFullPath(GameObject go) {
        string path = go.name;
        Transform parent = go.transform.parent;
        while (parent != null) {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }
}
#endif
```

---

## 🔄 Race Conditions در Multiplayer

### مشکل

```csharp
// ❌ Race Condition: هر دو بازیکن همزمان تیر می‌زنند

// Player A در Frame 100:
otherPlayer.TakeDamage(50);  // HP → 50

// Player B در Frame 100 (همزمان):
otherPlayer.TakeDamage(50);  // HP → 50 یا 0 ؟!
```

### نتیجه

```
Player A می‌بیند: HP = 50
Player B می‌بیند: HP = 50
سرور می‌بیند: HP = 0 (مرده!)

→ Desync!
```

### راه‌حل: Master Client Authority

```csharp
// ✅ فقط Master Client تصمیم می‌گیرد

[PunRPC]
public void TakeDamage(int amount, string enemyName) {
    // همه می‌شنوند
    playerAudio.Play();
    
    // فقط صاحب پردازش می‌کند
    if (!photonView.IsMine) return;
    
    currentHealth -= amount;
    
    // فقط Master تصمیم مرگ می‌گیرد
    if (currentHealth <= 0 && PhotonNetwork.IsMasterClient) {
        photonView.RPC("ConfirmDeath", RpcTarget.All, enemyName);
    }
}

[PunRPC]
void ConfirmDeath(string killerName) {
    // همه این را اجرا می‌کنند = Sync!
    isDead = true;
    // ...
}
```

---

## 🎭 Animation Sync Issues

### مشکل

```
بازیکن A: انیمیشن "Running" می‌بیند
بازیکن B: انیمیشن "Walking" می‌بیند برای همان بازیکن!
```

### علت در این پروژه

```csharp
// 📍 Assets/Scripts/PlayerNetworkMover.cs - خط 95

void FixedUpdate() {
    if (photonView.IsMine) {
        animator.SetFloat("Horizontal", CrossPlatformInputManager.GetAxis("Horizontal"));
        animator.SetFloat("Vertical", CrossPlatformInputManager.GetAxis("Vertical"));
        animator.SetBool("Running", Input.GetKey(KeyCode.LeftShift));
    }
    // ❌ مشکل: انیمیشن Sync نمی‌شود!
}
```

### راه‌حل

```csharp
// ✅ استفاده از PhotonAnimatorView

// در Inspector:
// 1. Add Component > PhotonAnimatorView
// 2. Synchronized Parameters تنظیم شود
// 3. Horizontal, Vertical, Running اضافه شوند

// یا به صورت Manual:
public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
    if (stream.IsWriting) {
        stream.SendNext(transform.position);
        stream.SendNext(transform.rotation);
        // ✅ اضافه کردن Animation State
        stream.SendNext(animator.GetFloat("Horizontal"));
        stream.SendNext(animator.GetFloat("Vertical"));
        stream.SendNext(animator.GetBool("Running"));
    } else {
        position = (Vector3)stream.ReceiveNext();
        rotation = (Quaternion)stream.ReceiveNext();
        // ✅ تنظیم Animation
        animator.SetFloat("Horizontal", (float)stream.ReceiveNext());
        animator.SetFloat("Vertical", (float)stream.ReceiveNext());
        animator.SetBool("Running", (bool)stream.ReceiveNext());
    }
}
```

---

## 💥 Physics و Collision Problems

### مشکل ۱: Raycast از داخل Collider

```csharp
// ❌ مشکل در FpsGun.cs
void Shoot() {
    Ray shootRay = raycastCamera.ScreenPointToRay(...);
    
    // اگر دوربین داخل Collider بازیکن باشد:
    // → Raycast ممکن است به خود بازیکن برخورد کند!
}
```

### راه‌حل: Layer Mask

```csharp
// ✅ راه‌حل: استفاده از Layer مناسب
void Shoot() {
    Ray shootRay = raycastCamera.ScreenPointToRay(...);
    
    // فقط به لایه "Shootable" برخورد کن
    int layerMask = LayerMask.GetMask("Shootable");
    
    if (Physics.Raycast(shootRay, out hit, range, layerMask)) {
        // ...
    }
}

// یا Ignore لایه بازیکن محلی:
int layerMask = ~LayerMask.GetMask("FPSPlayer");
```

### مشکل ۲: Trigger نمی‌خورد

```csharp
// ❌ در DoorAnimation.cs: OnTriggerStay فراخوانی نمی‌شود!

// Checklist:
// □ یکی باید Rigidbody داشته باشد
// □ Is Trigger فعال است؟
// □ Collider ها Overlap دارند؟
// □ Layer ها در Physics Settings تنظیم شده؟
```

### دیباگ Collision

```csharp
// اضافه کردن به DoorAnimation.cs برای دیباگ

void OnTriggerEnter(Collider other) {
    Debug.Log($"[Door] Trigger Enter: {other.name} (Tag: {other.tag})");
}

void OnTriggerStay(Collider other) {
    Debug.Log($"[Door] Trigger Stay: {other.name}");
    if (other.gameObject.tag == "Player") {
        animator.SetBool("Trigger", true);
    }
}

void OnTriggerExit(Collider other) {
    Debug.Log($"[Door] Trigger Exit: {other.name}");
}

// در Gizmos نمایش Trigger Zone
void OnDrawGizmos() {
    var col = GetComponent<Collider>();
    if (col != null) {
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawCube(col.bounds.center, col.bounds.size);
    }
}
```

---

## 🔊 Audio Problems

### مشکل: صدا پخش نمی‌شود

```csharp
// Checklist صدا:
// □ AudioSource موجود است؟
// □ AudioClip تنظیم شده؟
// □ Volume > 0 است؟
// □ Mute نیست؟
// □ AudioListener در Scene هست؟
// □ Spatial Blend برای 3D صدا درست است؟
```

### دیباگ

```csharp
// در TpsGun.cs

[PunRPC]
void Shoot() {
    if (gunAudio == null) {
        Debug.LogError($"[TpsGun] AudioSource is null on {name}!");
        return;
    }
    
    if (gunAudio.clip == null) {
        Debug.LogError($"[TpsGun] AudioClip is null on {name}!");
        return;
    }
    
    Debug.Log($"[TpsGun] Playing sound: {gunAudio.clip.name}");
    gunAudio.Play();
    
    if (!gunAudio.isPlaying) {
        Debug.LogWarning($"[TpsGun] Sound didn't start! Volume: {gunAudio.volume}");
    }
}
```

---

## 🧠 Memory Leaks

### شناسایی

```csharp
// مشکل در ImpactLifeCycle.cs اگر Destroy فراموش شود

public class ImpactLifeCycle : MonoBehaviour {
    void Start() {
        GetComponent<ParticleSystem>().Play();
        // Destroy(gameObject, lifespan);  // ❌ اگر فراموش شود!
    }
}

// نتیجه: هزاران impact در Scene باقی می‌مانند!
```

### روش شناسایی

```csharp
// ابزار شمارش آبجکت‌ها

#if UNITY_EDITOR
[ContextMenu("Count Impacts")]
void CountImpacts() {
    var impacts = FindObjectsOfType<ImpactLifeCycle>();
    Debug.Log($"Active impacts: {impacts.Length}");
    
    if (impacts.Length > 50) {
        Debug.LogWarning("TOO MANY IMPACTS! Possible memory leak!");
    }
}
#endif
```

---

## 📋 Debugging Checklist

```
🔴 NullReferenceException
□ Reference در Inspector ست شده؟
□ GetComponent result بررسی شده؟
□ Find موفق بوده؟

🔄 Sync Issues
□ photonView.IsMine بررسی شده؟
□ RPC Target درست است؟
□ Serialization فعال است؟

💥 Physics Issues
□ Rigidbody موجود است؟
□ Layer درست است؟
□ Collider Is Trigger درست است؟

🎭 Animation Issues
□ Animator Controller متصل است؟
□ Parameter نام‌گذاری درست است؟
□ Transition شرایط درست دارد؟

🔊 Audio Issues
□ AudioSource موجود است؟
□ AudioClip تنظیم شده؟
□ AudioListener در Scene هست؟
```

---

## 🚀 بخش بعدی

در بخش بعدی، با **اصول بهینه‌سازی** آشنا می‌شویم.

<div align="center">

**[⏮️ بخش قبلی](./05-debugging-network.md)** | **[⏭️ بخش بعدی: اصول بهینه‌سازی](./07-optimization-basics.md)**

</div>

---

<div align="center">

*Developed by Amin Davodian*

</div>
