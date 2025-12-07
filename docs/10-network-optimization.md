# 🌐 بهینه‌سازی شبکه

<div align="center">

**کاهش Bandwidth و بهبود Sync در Multiplayer**

</div>

---

## 📡 مشکلات شبکه

```
┌─────────────────────────────────────────────────────────────┐
│                  NETWORK PROBLEMS                            │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ⏱️ Latency (Ping)                                         │
│     └── فاصله زمانی بین ارسال و دریافت                     │
│                                                             │
│  📦 Bandwidth                                               │
│     └── حجم داده ارسالی/دریافتی در ثانیه                  │
│                                                             │
│  📉 Packet Loss                                             │
│     └── بسته‌های گم‌شده                                    │
│                                                             │
│  🔄 Jitter                                                  │
│     └── نوسان در زمان دریافت                               │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 📊 کاهش حجم داده

### مشکل در PlayerNetworkMover.cs

```csharp
// 📍 فعلی: ارسال Position و Rotation کامل

public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
    if (stream.IsWriting) {
        stream.SendNext(transform.position);      // 12 bytes (3 floats)
        stream.SendNext(transform.rotation);      // 16 bytes (4 floats)
    }
    // Total: 28 bytes per update
    // 20 updates/sec = 560 bytes/sec per player
}
```

### بهینه‌سازی ۱: فشرده‌سازی Position

```csharp
// ✅ ارسال فقط تغییرات (Delta Compression)

private Vector3 lastSentPosition;
private Quaternion lastSentRotation;
private const float POSITION_THRESHOLD = 0.01f;
private const float ROTATION_THRESHOLD = 1f;

public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
    if (stream.IsWriting) {
        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;
        
        // فقط اگر تغییر کافی داشت ارسال کن
        bool posChanged = Vector3.Distance(pos, lastSentPosition) > POSITION_THRESHOLD;
        bool rotChanged = Quaternion.Angle(rot, lastSentRotation) > ROTATION_THRESHOLD;
        
        stream.SendNext(posChanged);  // 1 bit
        if (posChanged) {
            stream.SendNext(pos);
            lastSentPosition = pos;
        }
        
        stream.SendNext(rotChanged);  // 1 bit
        if (rotChanged) {
            stream.SendNext(rot);
            lastSentRotation = rot;
        }
    } else {
        bool posChanged = (bool)stream.ReceiveNext();
        if (posChanged) {
            position = (Vector3)stream.ReceiveNext();
        }
        
        bool rotChanged = (bool)stream.ReceiveNext();
        if (rotChanged) {
            rotation = (Quaternion)stream.ReceiveNext();
        }
    }
}
```

### بهینه‌سازی ۲: Quantization

```csharp
// تبدیل float به short برای کاهش حجم

public static class NetworkCompression {
    
    // Position: محدوده -500 تا +500 متر، دقت 1cm
    public static short CompressPosition(float value) {
        return (short)(Mathf.Clamp(value, -500f, 500f) * 100f);
    }
    
    public static float DecompressPosition(short value) {
        return value / 100f;
    }
    
    // Rotation: 0-360 درجه، دقت ~0.3 درجه
    public static short CompressAngle(float angle) {
        return (short)((angle / 360f) * short.MaxValue);
    }
    
    public static float DecompressAngle(short value) {
        return (value / (float)short.MaxValue) * 360f;
    }
}

// استفاده:
public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
    if (stream.IsWriting) {
        // 6 bytes به جای 12 bytes!
        stream.SendNext(NetworkCompression.CompressPosition(transform.position.x));
        stream.SendNext(NetworkCompression.CompressPosition(transform.position.y));
        stream.SendNext(NetworkCompression.CompressPosition(transform.position.z));
    }
}
```

---

## 🎯 Interest Management

### مفهوم

```
بدون Interest Management:
┌─────────────────────────────────────┐
│ Player A ←───► همه بازیکنان       │
│ Player B ←───► همه بازیکنان       │  = داده زیاد!
│ Player C ←───► همه بازیکنان       │
└─────────────────────────────────────┘

با Interest Management:
┌─────────────────────────────────────┐
│ Player A ←──► فقط نزدیک‌ها        │
│              (B در محدوده)         │  = داده کم!
│ Player C بعید است = Sync نمی‌شود  │
└─────────────────────────────────────┘
```

### پیاده‌سازی ساده

```csharp
// در Photon: استفاده از Interest Groups

public class PlayerInterestManager : MonoBehaviour {
    
    private PhotonView pv;
    private byte currentGroup = 0;
    
    void Start() {
        pv = GetComponent<PhotonView>();
    }
    
    void Update() {
        if (!pv.IsMine) return;
        
        // تعیین گروه بر اساس موقعیت
        byte newGroup = GetGroupFromPosition(transform.position);
        
        if (newGroup != currentGroup) {
            currentGroup = newGroup;
            
            // فقط از گروه خود و همسایه‌ها دریافت کن
            PhotonNetwork.SetInterestGroups(
                new byte[] { 0 },  // Disable all
                new byte[] { currentGroup, GetAdjacentGroup(currentGroup) }
            );
        }
    }
    
    byte GetGroupFromPosition(Vector3 pos) {
        // تقسیم نقشه به مناطق
        int x = Mathf.FloorToInt(pos.x / 50f);
        int z = Mathf.FloorToInt(pos.z / 50f);
        return (byte)((x + 10) * 20 + (z + 10));
    }
}
```

---

## ⏱️ Lag Compensation

### مشکل

```
┌─────────────────────────────────────────────────────────────┐
│               WITHOUT LAG COMPENSATION                       │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Time 0ms:    Player A shoots at Enemy position             │
│               Enemy is HERE: ●                              │
│                             ↓                               │
│  Time 100ms:  Server receives shot                          │
│               Enemy moved: ──────► ●                        │
│                                   Miss!                     │
│                                                             │
│  نتیجه: A می‌زند اما miss می‌شود!                          │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### راه‌حل: Server Reconciliation

```csharp
// ارسال timestamp با هر عمل

[PunRPC]
void Shoot(Vector3 position, Vector3 direction, double timestamp) {
    if (PhotonNetwork.IsMasterClient) {
        // محاسبه موقعیت دشمنان در زمان تیراندازی
        float lag = (float)(PhotonNetwork.Time - timestamp);
        
        foreach (var enemy in enemies) {
            // برگرداندن موقعیت به زمان تیراندازی
            Vector3 pastPosition = enemy.GetPositionAtTime(timestamp);
            
            // بررسی برخورد با موقعیت گذشته
            if (CheckHit(position, direction, pastPosition)) {
                enemy.TakeDamage(damage);
                break;
            }
        }
    }
}
```

### Client-Side Prediction

```csharp
// پیش‌بینی حرکت محلی

public class PredictedMovement : MonoBehaviour {
    
    private Queue<InputFrame> pendingInputs = new Queue<InputFrame>();
    private Vector3 serverPosition;
    
    void Update() {
        if (!photonView.IsMine) return;
        
        // ۱. ثبت input
        var input = new InputFrame {
            id = frameId++,
            horizontal = Input.GetAxis("Horizontal"),
            vertical = Input.GetAxis("Vertical"),
            timestamp = Time.time
        };
        pendingInputs.Enqueue(input);
        
        // ۲. اعمال فوری محلی (Prediction)
        ApplyInput(input);
        
        // ۳. ارسال به سرور
        photonView.RPC("ServerMove", RpcTarget.MasterClient, input);
    }
    
    [PunRPC]
    void ServerConfirm(int confirmedId, Vector3 confirmedPosition) {
        serverPosition = confirmedPosition;
        
        // حذف input های تأیید شده
        while (pendingInputs.Count > 0 && 
               pendingInputs.Peek().id <= confirmedId) {
            pendingInputs.Dequeue();
        }
        
        // اگر اختلاف زیاد بود، اصلاح کن
        if (Vector3.Distance(transform.position, serverPosition) > 0.5f) {
            transform.position = serverPosition;
            
            // Re-apply pending inputs
            foreach (var input in pendingInputs) {
                ApplyInput(input);
            }
        }
    }
}
```

---

## 📦 Serialization Rate

### تنظیم مناسب

```csharp
// PhotonServerSettings:
// Serialization Rate: 20 (پیش‌فرض)

// تنظیم داینامیک بر اساس اهمیت:
void SetUpdateRate(PhotonView pv, bool isVisible) {
    if (isVisible) {
        // بازیکن قابل دیدن = آپدیت بیشتر
        pv.ObservedComponents[0].Synchronization = 
            ViewSynchronization.UnreliableOnChange;
    } else {
        // بازیکن دور = آپدیت کمتر
        pv.ObservedComponents[0].Synchronization = 
            ViewSynchronization.Off;
    }
}
```

### تنظیم در کد

```csharp
void Start() {
    // نرخ ارسال کمتر = bandwidth کمتر
    PhotonNetwork.SerializationRate = 15;  // packets/sec
    
    // نرخ ارسال RPC
    PhotonNetwork.SendRate = 20;  // messages/sec
}
```

---

## 🔄 Interpolation vs Extrapolation

### Interpolation (Smoothing)

```csharp
// نمایش روان موقعیت دیگران

void Update() {
    if (!photonView.IsMine) {
        // Lerp به موقعیت دریافتی
        transform.position = Vector3.Lerp(
            transform.position, 
            targetPosition, 
            Time.deltaTime * smoothing
        );
    }
}
```

### Extrapolation (Prediction)

```csharp
// پیش‌بینی موقعیت بر اساس velocity

private Vector3 velocity;
private float lastReceiveTime;

void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
    if (stream.IsReading) {
        Vector3 newPos = (Vector3)stream.ReceiveNext();
        Vector3 newVel = (Vector3)stream.ReceiveNext();
        
        // محاسبه velocity
        float lag = (float)(PhotonNetwork.Time - info.SentServerTime);
        
        // Extrapolate به زمان حال
        targetPosition = newPos + newVel * lag;
        velocity = newVel;
        lastReceiveTime = Time.time;
    }
}

void Update() {
    if (!photonView.IsMine) {
        // ادامه حرکت بر اساس velocity
        targetPosition += velocity * Time.deltaTime;
        transform.position = Vector3.Lerp(
            transform.position, 
            targetPosition, 
            Time.deltaTime * smoothing
        );
    }
}
```

---

## 📋 Network Optimization Checklist

```
□ Delta Compression فعال؟
□ Quantization برای کاهش حجم؟
□ Interest Groups تنظیم شده؟
□ Serialization Rate مناسب (10-20)?
□ Lag Compensation پیاده شده؟
□ Interpolation برای smoothness؟
□ بررسی Bandwidth در Photon Stats؟
```

---

## 🚀 بخش بعدی

در بخش بعدی، با **بهینه‌سازی کد** آشنا می‌شویم.

<div align="center">

**[⏮️ بخش قبلی](./09-rendering-optimization.md)** | **[⏭️ بخش بعدی: بهینه‌سازی کد](./11-code-optimization.md)**

</div>

---

<div align="center">

*Developed by Amin Davodian*

</div>
