# 💻 بهینه‌سازی کد

<div align="center">

**بهترین شیوه‌های کدنویسی برای عملکرد بهتر**

</div>

---

## 🔧 Caching Components

### مشکل رایج

```csharp
// ❌ GetComponent هر فریم = کند!
void Update() {
    GetComponent<Rigidbody>().AddForce(Vector3.up);
    GetComponent<Animator>().SetTrigger("Jump");
    GetComponent<AudioSource>().Play();
}
```

### راه‌حل: Cache در Awake/Start

```csharp
// ✅ Cache یکبار، استفاده همیشه
public class Player : MonoBehaviour {
    
    private Rigidbody rb;
    private Animator animator;
    private AudioSource audioSource;
    
    void Awake() {
        // Cache در Awake
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }
    
    void Update() {
        rb.AddForce(Vector3.up);
        animator.SetTrigger("Jump");
        audioSource.Play();
    }
}
```

### در این پروژه: PlayerHealth.cs

```csharp
// 📍 Assets/Scripts/PlayerHealth.cs

// ❌ فعلی: Find هر Start
void Start() {
    damageImage = GameObject.FindGameObjectWithTag("Screen")
        .transform.Find("DamageImage").GetComponent<Image>();
    healthSlider = GameObject.FindGameObjectWithTag("Screen")
        .GetComponentInChildren<Slider>();
}

// ✅ بهتر: Cache در Awake با SerializeField
[SerializeField] private Image damageImage;    // از Inspector
[SerializeField] private Slider healthSlider;  // از Inspector

void Awake() {
    // اگر تنظیم نشده، Find کن
    if (damageImage == null) {
        var screen = GameObject.FindGameObjectWithTag("Screen");
        if (screen != null) {
            damageImage = screen.transform.Find("DamageImage")?.GetComponent<Image>();
        }
    }
}
```

---

## ⏱️ Update vs FixedUpdate vs LateUpdate

### تفاوت‌ها

```
┌─────────────────────────────────────────────────────────────┐
│                   UPDATE METHODS                             │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Update()                                                   │
│  ├── هر فریم یکبار                                         │
│  ├── وابسته به FPS                                         │
│  └── برای: Input, UI, Logic                                │
│                                                             │
│  FixedUpdate()                                              │
│  ├── هر 0.02 ثانیه (50 بار در ثانیه)                       │
│  ├── مستقل از FPS                                          │
│  └── برای: Physics, Rigidbody                              │
│                                                             │
│  LateUpdate()                                               │
│  ├── بعد از همه Update ها                                  │
│  └── برای: Camera Follow, IK                               │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### مثال در پروژه

```csharp
// 📍 Assets/Scripts/PlayerNetworkMover.cs

// فعلی:
void FixedUpdate() {
    if (photonView.IsMine) {
        animator.SetFloat("Horizontal", 
            CrossPlatformInputManager.GetAxis("Horizontal"));
        animator.SetFloat("Vertical", 
            CrossPlatformInputManager.GetAxis("Vertical"));
        animator.SetBool("Running", Input.GetKey(KeyCode.LeftShift));
    }
}

// ✅ بهتر: Input در Update، Physics در FixedUpdate
private float horizontal, vertical;
private bool running;

void Update() {
    if (photonView.IsMine) {
        // Input در Update (پاسخ‌گویی بهتر)
        horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
        vertical = CrossPlatformInputManager.GetAxis("Vertical");
        running = Input.GetKey(KeyCode.LeftShift);
    }
}

void FixedUpdate() {
    if (photonView.IsMine) {
        // Animation و Physics در FixedUpdate
        animator.SetFloat("Horizontal", horizontal);
        animator.SetFloat("Vertical", vertical);
        animator.SetBool("Running", running);
    }
}
```

---

## 🔄 Coroutines بهینه

### مشکلات رایج

```csharp
// ❌ yield return new (Allocation!)
IEnumerator BadCoroutine() {
    while (true) {
        yield return new WaitForSeconds(1f);  // GC هر ثانیه!
    }
}

// ❌ StartCoroutine زیاد
void Update() {
    if (shouldDoSomething) {
        StartCoroutine(DoSomething());  // Coroutine جدید هر فریم!
    }
}
```

### راه‌حل‌ها

```csharp
// ✅ Cache WaitForSeconds
private WaitForSeconds waitOneSecond;

void Awake() {
    waitOneSecond = new WaitForSeconds(1f);
}

IEnumerator GoodCoroutine() {
    while (true) {
        yield return waitOneSecond;  // بدون Allocation!
    }
}

// ✅ استفاده از bool به جای Coroutine زیاد
private Coroutine activeCoroutine;

void Update() {
    if (shouldDoSomething && activeCoroutine == null) {
        activeCoroutine = StartCoroutine(DoSomething());
    }
}

IEnumerator DoSomething() {
    // کار
    yield return waitOneSecond;
    activeCoroutine = null;
}
```

### جایگزین: Invoke

```csharp
// برای تأخیر ساده، Invoke کافی است
void Start() {
    Invoke(nameof(DoSomething), 1f);     // یکبار
    InvokeRepeating(nameof(Tick), 0, 1f);  // تکرار
}

void DoSomething() {
    Debug.Log("Done!");
}

void Tick() {
    Debug.Log("Tick!");
}

void OnDisable() {
    CancelInvoke();  // پاکسازی
}
```

---

## 🎯 LINQ vs For Loop

### مقایسه عملکرد

```csharp
// ❌ LINQ - زیبا اما کند در Update (Allocation!)
void Update() {
    var aliveEnemies = enemies.Where(e => e.IsAlive).ToList();
    var closest = aliveEnemies.OrderBy(e => 
        Vector3.Distance(e.position, transform.position)).FirstOrDefault();
}

// ✅ For Loop - سریع و بدون Allocation
void Update() {
    Enemy closest = null;
    float closestDist = float.MaxValue;
    
    for (int i = 0; i < enemies.Length; i++) {
        if (!enemies[i].IsAlive) continue;
        
        float dist = Vector3.SqrMagnitude(
            enemies[i].position - transform.position);  // بدون Sqrt!
        
        if (dist < closestDist) {
            closestDist = dist;
            closest = enemies[i];
        }
    }
}
```

### قانون

```
LINQ استفاده کن در:
✅ Start/Awake (یکبار)
✅ Editor scripts
✅ جاهایی که خوانایی مهم‌تر است

For Loop استفاده کن در:
✅ Update/FixedUpdate
✅ Hot paths (مسیرهای پرتردد)
✅ جاهایی که عملکرد مهم است
```

---

## 📏 Vector Operations

### اشتباهات رایج

```csharp
// ❌ Vector3.Distance در مقایسه (شامل Sqrt!)
if (Vector3.Distance(a, b) < 10f) { }

// ✅ SqrMagnitude برای مقایسه (بدون Sqrt!)
if ((a - b).sqrMagnitude < 100f) { }  // 10 * 10 = 100

// ❌ new Vector3 زیاد
void Update() {
    transform.position += new Vector3(1, 0, 0) * Time.deltaTime;
}

// ✅ استفاده از Vector3.right
void Update() {
    transform.position += Vector3.right * Time.deltaTime;
}
```

### Cache Screen Dimensions

```csharp
// 📍 در FpsGun.cs

// ❌ هر بار محاسبه
void Shoot() {
    Ray shootRay = raycastCamera.ScreenPointToRay(
        new Vector3(Screen.width/2, Screen.height/2, 0f));  // new!
}

// ✅ Cache در Start
private Vector3 screenCenter;

void Start() {
    screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
}

void OnApplicationFocus(bool hasFocus) {
    // آپدیت اگر Resolution عوض شد
    screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
}

void Shoot() {
    Ray shootRay = raycastCamera.ScreenPointToRay(screenCenter);
}
```

---

## 🔍 Physics Optimization

### NonAlloc Methods

```csharp
// ❌ RaycastAll allocation
RaycastHit[] hits = Physics.RaycastAll(ray, distance);

// ✅ RaycastNonAlloc (no allocation)
private RaycastHit[] hitBuffer = new RaycastHit[10];

void Shoot() {
    int hitCount = Physics.RaycastNonAlloc(ray, hitBuffer, distance);
    for (int i = 0; i < hitCount; i++) {
        ProcessHit(hitBuffer[i]);
    }
}
```

### Layer Mask Cache

```csharp
// ❌ هر بار parse string
void Update() {
    int mask = LayerMask.GetMask("Player", "Enemy");  // string parse!
}

// ✅ Cache mask
private int shootableMask;

void Awake() {
    shootableMask = LayerMask.GetMask("Player", "Enemy");
}

void Update() {
    Physics.Raycast(ray, out hit, distance, shootableMask);
}
```

---

## 💾 Struct vs Class

### تفاوت

```csharp
// Class: Reference Type → Heap → GC
class PositionData {
    public float x, y, z;
}

// Struct: Value Type → Stack → No GC
struct PositionData {
    public float x, y, z;
}

// استفاده برای داده‌های کوچک و موقت
```

### مثال در Network

```csharp
// ✅ Struct برای Network Data
public struct NetworkInputPacket {
    public float horizontal;
    public float vertical;
    public bool jump;
    public bool shoot;
    public double timestamp;
}
```

---

## 📋 Code Optimization Checklist

```
□ GetComponent در Awake/Start cache شده؟
□ Find/FindObjectsOfType در Update نیست؟
□ WaitForSeconds cache شده؟
□ For loop به جای LINQ در Update؟
□ SqrMagnitude به جای Distance؟
□ NonAlloc methods برای Physics؟
□ Layer masks cache شده؟
□ Struct برای داده‌های کوچک؟
```

---

## 🚀 بخش بعدی

در بخش بعدی، **دمو لایو** را انجام می‌دهیم!

<div align="center">

**[⏮️ بخش قبلی](./10-network-optimization.md)** | **[⏭️ بخش بعدی: دمو لایو](./12-live-demo.md)**

</div>

---

<div align="center">

*Developed by Amin Davodian*

</div>
