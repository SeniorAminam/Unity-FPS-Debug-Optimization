# 🌐 دیباگ شبکه و Photon

<div align="center">

**بررسی و رفع مشکلات شبکه در بازی‌های Multiplayer**

</div>

---

## 📡 Photon PUN2 چیست؟

```
┌─────────────────────────────────────────────────────────────┐
│                    PHOTON PUN2 ARCHITECTURE                  │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│     Client 1          Photon Cloud         Client 2         │
│    ┌────────┐        ┌──────────┐        ┌────────┐        │
│    │ Unity  │◄──────►│  Master  │◄──────►│ Unity  │        │
│    │  Game  │        │  Server  │        │  Game  │        │
│    └────────┘        └────┬─────┘        └────────┘        │
│         │                 │                   │             │
│         └─────────────────┼───────────────────┘             │
│                           ▼                                 │
│                    ┌──────────┐                            │
│                    │   Game   │                            │
│                    │  Server  │                            │
│                    └──────────┘                            │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 🔍 ابزارهای دیباگ Photon

### ۱. PhotonNetwork Stats

```csharp
// نمایش آمار شبکه در UI

public class NetworkDebugUI : MonoBehaviour {
    
    [SerializeField] private Text statsText;
    
    void Update() {
        if (!PhotonNetwork.IsConnected) {
            statsText.text = "Disconnected";
            return;
        }
        
        var stats = PhotonNetwork.NetworkingClient.LoadBalancingPeer.Stats;
        
        statsText.text = $@"
🌐 Network Stats
────────────────
Ping: {PhotonNetwork.GetPing()} ms
Room: {PhotonNetwork.CurrentRoom?.Name ?? "N/A"}
Players: {PhotonNetwork.CurrentRoom?.PlayerCount ?? 0}
────────────────
Sent: {stats.OutgoingPackagesCount} packets
Recv: {stats.IncomingPackagesCount} packets
Lost: {stats.PackagesLostBySendInterval}
────────────────
Outgoing: {stats.OutgoingBandwidth} bytes/s
Incoming: {stats.IncomingBandwidth} bytes/s";
    }
}
```

### ۲. PhotonView Debugging

```csharp
// بررسی PhotonView در Inspector

public class PhotonViewDebugger : MonoBehaviour {
    
    private PhotonView pv;
    
    void Awake() {
        pv = GetComponent<PhotonView>();
    }
    
    void OnGUI() {
        #if UNITY_EDITOR
        if (pv == null) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label($"ViewID: {pv.ViewID}");
        GUILayout.Label($"Owner: {pv.Owner?.NickName ?? "Scene"}");
        GUILayout.Label($"IsMine: {pv.IsMine}");
        GUILayout.Label($"CreatorActorNr: {pv.CreatorActorNr}");
        GUILayout.Label($"Observed Components: {pv.ObservedComponents?.Count ?? 0}");
        GUILayout.EndArea();
        #endif
    }
}
```

---

## 🎯 RPC Debugging

### بررسی RPC در PlayerHealth.cs

```csharp
// 📍 Assets/Scripts/PlayerHealth.cs

[PunRPC]
public void TakeDamage(int amount, string enemyName) {
    // 🔍 DEBUG: لاگ RPC
    #if UNITY_EDITOR
    Debug.Log($"[RPC] TakeDamage called on {photonView.Owner.NickName}");
    Debug.Log($"  ├── IsMine: {photonView.IsMine}");
    Debug.Log($"  ├── Amount: {amount}");
    Debug.Log($"  ├── From: {enemyName}");
    Debug.Log($"  └── Current Health: {currentHealth}");
    #endif
    
    if (isDead) {
        Debug.LogWarning($"[RPC] TakeDamage ignored - player is dead");
        return;
    }
    
    if (photonView.IsMine) {
        damaged = true;
        currentHealth -= amount;
        
        if (currentHealth <= 0) {
            Debug.Log($"[RPC] Player died! Calling Death RPC");
            photonView.RPC("Death", RpcTarget.All, enemyName);
        }
    }
}
```

### RPC Targets

```
┌─────────────────────────────────────────────────────────────┐
│                      RPC TARGETS                             │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  RpcTarget.All          ──►  همه کلاینت‌ها (+ خودم)        │
│  RpcTarget.Others       ──►  همه کلاینت‌ها (- خودم)        │
│  RpcTarget.MasterClient ──►  فقط Master Client             │
│  RpcTarget.AllBuffered  ──►  همه + Buffer برای جوین‌کننده  │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### مثال: مشکل رایج RPC

```csharp
// ❌ مشکل: RPC روی همه فراخوانی می‌شود
photonView.RPC("TakeDamage", RpcTarget.All, 20, shooterName);
// → همه بازیکنان TakeDamage را دریافت می‌کنند!

// ✅ راه‌حل: استفاده از IsMine
[PunRPC]
void TakeDamage(int amount, string enemyName) {
    if (photonView.IsMine) {  // فقط صاحب اصلی پردازش می‌کند
        currentHealth -= amount;
    }
    // افکت‌ها برای همه
    playerAudio.Play();
}
```

---

## 🔄 IPunObservable Debugging

### بررسی Serialization

```csharp
// 📍 Assets/Scripts/PlayerNetworkMover.cs

public class PlayerNetworkMover : MonoBehaviourPunCallbacks, IPunObservable {
    
    // === DEBUG ===
    #if UNITY_EDITOR
    [Header("🔍 Debug Info")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private int packetsReceived = 0;
    [SerializeField] private int packetsSent = 0;
    [SerializeField] private float lastReceiveTime;
    #endif
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            // ارسال داده
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            
            #if UNITY_EDITOR
            packetsSent++;
            #endif
        } else {
            // دریافت داده
            position = (Vector3)stream.ReceiveNext();
            rotation = (Quaternion)stream.ReceiveNext();
            
            #if UNITY_EDITOR
            packetsReceived++;
            lastReceiveTime = Time.time;
            
            // بررسی تأخیر
            float lag = (float)(PhotonNetwork.Time - info.SentServerTime);
            if (lag > 0.1f) {
                Debug.LogWarning($"[Network] High lag detected: {lag * 1000:F0}ms");
            }
            #endif
        }
    }
}
```

### Serialization Rate

```csharp
// تنظیم نرخ ارسال
// PhotonServerSettings > Serialization Rate: 20 (پیش‌فرض)

// در کد:
void Start() {
    // تغییر نرخ برای این PhotonView
    photonView.Synchronization = ViewSynchronization.Unreliable;
    PhotonNetwork.SerializationRate = 20;  // بسته در ثانیه
}
```

---

## ⚠️ مشکلات رایج شبکه

### ۱. Sync Issues

```csharp
// ❌ مشکل: تغییر Position بدون Sync
void Update() {
    if (Input.GetKeyDown(KeyCode.T)) {
        transform.position = new Vector3(0, 0, 0);  // فقط محلی!
    }
}

// ✅ راه‌حل: استفاده از RPC
void Update() {
    if (Input.GetKeyDown(KeyCode.T)) {
        if (photonView.IsMine) {
            photonView.RPC("Teleport", RpcTarget.All, Vector3.zero);
        }
    }
}

[PunRPC]
void Teleport(Vector3 position) {
    transform.position = position;
}
```

### ۲. Ownership Issues

```csharp
// بررسی مالکیت قبل از تغییرات
void TryModifyObject() {
    if (!photonView.IsMine) {
        Debug.LogWarning($"Cannot modify {name} - not owner!");
        
        // درخواست مالکیت (اگر مجاز باشد)
        if (photonView.OwnershipTransfer == OwnershipOption.Request) {
            photonView.RequestOwnership();
        }
        return;
    }
    
    // ادامه تغییرات...
}
```

### ۳. Late Join Sync

```csharp
// مشکل: بازیکن جدید وضعیت را نمی‌بیند

// راه‌حل 1: AllBuffered
photonView.RPC("SetRoomState", RpcTarget.AllBuffered, state);

// راه‌حل 2: OnPlayerEnteredRoom
public override void OnPlayerEnteredRoom(Player newPlayer) {
    if (PhotonNetwork.IsMasterClient) {
        photonView.RPC("SyncState", newPlayer, currentHealth, transform.position);
    }
}
```

---

## 📊 Network Lag Visualization

```csharp
// نمایش Lag Compensation

public class LagCompensation : MonoBehaviour {
    
    [SerializeField] private LineRenderer lagLine;
    
    private Vector3 lastReceivedPosition;
    private Vector3 serverPosition;
    
    void Update() {
        // نمایش فاصله بین موقعیت واقعی و موقعیت سرور
        if (lagLine != null && !photonView.IsMine) {
            lagLine.SetPosition(0, transform.position);  // موقعیت فعلی (interpolated)
            lagLine.SetPosition(1, serverPosition);       // موقعیت دریافتی از سرور
            
            float distance = Vector3.Distance(transform.position, serverPosition);
            lagLine.startColor = distance > 1f ? Color.red : Color.green;
        }
    }
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsReading) {
            serverPosition = (Vector3)stream.ReceiveNext();
        }
    }
}
```

---

## 🔧 Network خاص در NetworkManager.cs

```csharp
// 📍 Assets/Scripts/NetworkManager.cs - بررسی Connection

public class NetworkManager : MonoBehaviourPunCallbacks {
    
    void Start() {
        // 🔍 DEBUG: وضعیت اتصال
        Debug.Log("[NetworkManager] Starting connection...");
        Debug.Log($"  ├── AppId: {PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime}");
        Debug.Log($"  └── Region: {PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion}");
        
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
    }
    
    public override void OnConnectedToMaster() {
        Debug.Log("[NetworkManager] Connected to Master!");
        Debug.Log($"  ├── Server: {PhotonNetwork.ServerAddress}");
        Debug.Log($"  ├── Ping: {PhotonNetwork.GetPing()} ms");
        Debug.Log($"  └── Client Version: {PhotonNetwork.NetworkClientState}");
        
        PhotonNetwork.JoinLobby();
    }
    
    public override void OnDisconnected(DisconnectCause cause) {
        Debug.LogError($"[NetworkManager] Disconnected! Cause: {cause}");
        
        // تحلیل علت قطعی
        switch (cause) {
            case DisconnectCause.ServerTimeout:
                Debug.LogError("  └── Server did not respond in time");
                break;
            case DisconnectCause.ClientTimeout:
                Debug.LogError("  └── Client connection timed out");
                break;
            case DisconnectCause.MaxCcuReached:
                Debug.LogError("  └── Max concurrent users reached");
                break;
            case DisconnectCause.InvalidAuthentication:
                Debug.LogError("  └── Invalid AppId or authentication");
                break;
        }
    }
}
```

---

## 📋 Network Debugging Checklist

```
□ PhotonNetwork.IsConnected == true?
□ PhotonNetwork.InRoom == true?
□ photonView.IsMine درست بررسی شده؟
□ RpcTarget مناسب استفاده شده؟
□ Serialization Rate کافی است؟
□ Lag قابل قبول است (< 150ms)؟
□ Packet Loss نداریم؟
```

---

## 🚀 بخش بعدی

در بخش بعدی، با **باگ‌های رایج** و روش‌های شناسایی آن‌ها آشنا می‌شویم.

<div align="center">

**[⏮️ بخش قبلی](./04-console-profiler.md)** | **[⏭️ بخش بعدی: باگ‌های رایج](./06-common-bugs.md)**

</div>

---

<div align="center">

*Developed by Amin Davodian*

</div>
