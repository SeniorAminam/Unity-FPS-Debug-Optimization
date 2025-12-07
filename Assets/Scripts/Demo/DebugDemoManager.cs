/**
 * Project: Multiplayer FPS
 * File: Assets/Scripts/Demo/DebugDemoManager.cs
 * Author: Amin Davodian (Mohammadamin Davodian)
 * Website: https://senioramin.com
 * LinkedIn: https://linkedin.com/in/SudoAmin
 * GitHub: https://github.com/SeniorAminam
 * Created: 2025-12-07
 * 
 * Purpose: Manager for live debugging and optimization demos during presentation
 * Developed by Amin Davodian
 */

using System;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Collections.Generic;

/// <summary>
/// مدیریت دموهای لایو برای ارائه دانشگاه
/// این کلاس امکان Toggle کردن بین حالت‌های مختلف دیباگ و بهینه‌سازی را فراهم می‌کند
/// </summary>
public class DebugDemoManager : MonoBehaviour
{
    public static DebugDemoManager Instance { get; private set; }

    [Header("🎮 Demo Settings")]
    [Tooltip("فعال/غیرفعال کردن کل سیستم دمو")]
    public bool enableDemoMode = true;
    
    [Tooltip("نمایش/مخفی کردن پنل دیباگ (F12)")]
    public bool showDebugPanel = true;
    
    [Header("📊 Debug Toggles")]
    [Tooltip("نمایش خطوط Raycast تیراندازی")]
    public bool showRaycastDebug = false;
    
    [Tooltip("نمایش اطلاعات شبکه")]
    public bool showNetworkStats = false;
    
    [Tooltip("نمایش اطلاعات FPS و Memory")]
    public bool showPerformanceStats = false;
    
    [Tooltip("نمایش Gizmos های Spawn Points")]
    public bool showSpawnPointGizmos = false;
    
    [Header("⚡ Optimization Toggles")]
    [Tooltip("استفاده از Object Pooling به جای Instantiate")]
    public bool useObjectPooling = false;
    
    [Tooltip("فعال کردن String Optimization")]
    public bool useStringOptimization = false;
    
    [Tooltip("Cache کردن Screen dimensions")]
    public bool cacheScreenDimensions = false;
    
    [Tooltip("استفاده از SqrMagnitude به جای Distance")]
    public bool useSqrMagnitude = false;
    
    [Header("🐛 Bug Simulation")]
    [Tooltip("شبیه‌سازی باگ NullReference")]
    public bool simulateNullBug = false;
    
    [Tooltip("ایجاد GC Allocation زیاد")]
    public bool simulateGCSpikes = false;
    
    [Header("📈 Statistics")]
    [SerializeField] private int raycastsThisFrame = 0;
    [SerializeField] private int gcAllocThisFrame = 0;
    [SerializeField] private float currentFPS = 0;
    [SerializeField] private int objectsPooled = 0;
    [SerializeField] private int objectsInstantiated = 0;

    // Private fields
    private StringBuilder statsBuilder = new StringBuilder(512);
    private float fpsUpdateInterval = 0.5f;
    private float fpsTimer = 0;
    private int frameCount = 0;
    private Vector3 cachedScreenCenter;
    private List<string> gcTestList = new List<string>();
    
    // Demo labels
    private Dictionary<string, string> demoDescriptions = new Dictionary<string, string>
    {
        { "RaycastDebug", "نمایش خط قرمز Raycast هنگام تیراندازی در Scene View" },
        { "NetworkStats", "نمایش Ping, Packet Loss و Bandwidth در صفحه" },
        { "PerformanceStats", "نمایش FPS, Memory و GC Alloc" },
        { "ObjectPooling", "استفاده مجدد از Impact Effects به جای Instantiate" },
        { "NullBug", "شبیه‌سازی خطای NullReferenceException" },
        { "GCSpikes", "ایجاد GC Spikes برای نمایش در Profiler" }
    };

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CacheScreenDimensions();
            Debug.Log("[DebugDemoManager] 🎮 Demo Manager Initialized!");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (!enableDemoMode) return;

        HandleInput();
        UpdateFPS();
        
        if (simulateGCSpikes)
        {
            SimulateGCAllocation();
        }
        
        if (simulateNullBug)
        {
            SimulateNullBug();
        }
    }

    void OnGUI()
    {
        if (!enableDemoMode) return;

        if (showDebugPanel)
        {
            DrawDemoUI();
        }
        else
        {
            DrawMinimalHint();
        }
    }

    #region Input Handling

    void HandleInput()
    {
        // F1-F6 برای Toggle های مختلف
        if (Input.GetKeyDown(KeyCode.F1))
        {
            showRaycastDebug = !showRaycastDebug;
            LogToggle("Raycast Debug", showRaycastDebug);
        }
        
        if (Input.GetKeyDown(KeyCode.F2))
        {
            showNetworkStats = !showNetworkStats;
            LogToggle("Network Stats", showNetworkStats);
        }
        
        if (Input.GetKeyDown(KeyCode.F3))
        {
            showPerformanceStats = !showPerformanceStats;
            LogToggle("Performance Stats", showPerformanceStats);
        }
        
        if (Input.GetKeyDown(KeyCode.F4))
        {
            useObjectPooling = !useObjectPooling;
            LogToggle("Object Pooling", useObjectPooling);
        }
        
        if (Input.GetKeyDown(KeyCode.F5))
        {
            simulateNullBug = !simulateNullBug;
            LogToggle("Null Bug Simulation", simulateNullBug);
        }
        
        if (Input.GetKeyDown(KeyCode.F6))
        {
            simulateGCSpikes = !simulateGCSpikes;
            LogToggle("GC Spikes Simulation", simulateGCSpikes);
        }
        
        if (Input.GetKeyDown(KeyCode.F7))
        {
            useSqrMagnitude = !useSqrMagnitude;
            LogToggle("SqrMagnitude Optimization", useSqrMagnitude);
        }
        
        if (Input.GetKeyDown(KeyCode.F8))
        {
            cacheScreenDimensions = !cacheScreenDimensions;
            LogToggle("Cache Screen Dimensions", cacheScreenDimensions);
        }
        
        // F12: Toggle Debug Panel visibility
        if (Input.GetKeyDown(KeyCode.F12))
        {
            showDebugPanel = !showDebugPanel;
            LogToggle("Debug Panel", showDebugPanel);
        }
    }

    void LogToggle(string feature, bool state)
    {
        string status = state ? "✅ ON" : "❌ OFF";
        Debug.Log($"[Demo] {feature}: {status}");
    }

    #endregion

    #region UI Drawing

    void DrawDemoUI()
    {
        // راهنمای کلیدها
        GUILayout.BeginArea(new Rect(10, 10, 350, 500));
        
        GUI.color = new Color(0, 0, 0, 0.7f);
        GUILayout.Box("", GUILayout.Width(340), GUILayout.Height(GetUIHeight()));
        GUI.color = Color.white;
        
        GUILayout.BeginArea(new Rect(20, 20, 320, 480));
        
        // Title
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        GUILayout.Label("🎮 Debug Demo Panel", titleStyle);
        GUILayout.Space(10);
        
        // Toggle Buttons
        DrawToggleRow("F1: Raycast Debug", showRaycastDebug);
        DrawToggleRow("F2: Network Stats", showNetworkStats);
        DrawToggleRow("F3: Performance Stats", showPerformanceStats);
        DrawToggleRow("F4: Object Pooling", useObjectPooling);
        DrawToggleRow("F5: Null Bug Sim", simulateNullBug);
        DrawToggleRow("F6: GC Spikes Sim", simulateGCSpikes);
        DrawToggleRow("F7: SqrMagnitude", useSqrMagnitude);
        DrawToggleRow("F8: Cache Screen", cacheScreenDimensions);
        
        GUILayout.Space(15);
        
        // Performance Stats
        if (showPerformanceStats)
        {
            GUILayout.Label($"📊 FPS: {currentFPS:F1}");
            GUILayout.Label($"📦 Memory: {GetMemoryUsage():F1} MB");
            GUILayout.Label($"🗑️ GC Alloc: {GC.GetTotalMemory(false) / 1024:F0} KB");
            GUILayout.Label($"🎯 Pooled: {objectsPooled} | Spawned: {objectsInstantiated}");
        }
        
        GUILayout.EndArea();
        GUILayout.EndArea();
    }

    void DrawToggleRow(string label, bool isOn)
    {
        GUILayout.BeginHorizontal();
        GUI.color = isOn ? Color.green : Color.gray;
        GUILayout.Label(isOn ? "●" : "○", GUILayout.Width(20));
        GUI.color = Color.white;
        GUILayout.Label(label);
        GUILayout.EndHorizontal();
    }

    float GetUIHeight()
    {
        float height = 250;
        if (showPerformanceStats) height += 80;
        return height;
    }

    void DrawMinimalHint()
    {
        // نمایش hint کوچک در گوشه صفحه وقتی پنل مخفی است
        GUIStyle hintStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            alignment = TextAnchor.MiddleCenter
        };
        
        GUI.color = new Color(0, 0, 0, 0.5f);
        GUI.Box(new Rect(10, 10, 150, 25), "");
        GUI.color = new Color(1, 1, 1, 0.7f);
        GUI.Label(new Rect(15, 10, 140, 25), "F12: Show Debug Panel", hintStyle);
        GUI.color = Color.white;
    }

    #endregion

    #region Performance Tracking

    void UpdateFPS()
    {
        frameCount++;
        fpsTimer += Time.unscaledDeltaTime;
        
        if (fpsTimer >= fpsUpdateInterval)
        {
            currentFPS = frameCount / fpsTimer;
            frameCount = 0;
            fpsTimer = 0;
        }
    }

    float GetMemoryUsage()
    {
        return System.GC.GetTotalMemory(false) / (1024f * 1024f);
    }

    #endregion

    #region Bug Simulations

    void SimulateNullBug()
    {
        // این متد عمداً NullReferenceException ایجاد می‌کند
        try
        {
            GameObject nullObj = null;
            // این خط خطا می‌دهد!
            nullObj.transform.position = Vector3.zero;
        }
        catch (System.NullReferenceException e)
        {
            Debug.LogError($"[Demo] 🐛 NullReferenceException simulated!\n{e.Message}");
            simulateNullBug = false; // فقط یکبار
        }
    }

    void SimulateGCAllocation()
    {
        // ایجاد Allocation زیاد برای نمایش در Profiler
        for (int i = 0; i < 100; i++)
        {
            gcTestList.Add($"GC Test String {i} at {Time.time}");
        }
        gcTestList.Clear();
        
        // String concatenation (GC heavy)
        string heavyString = "";
        for (int i = 0; i < 50; i++)
        {
            heavyString += "x";  // هر بار string جدید!
        }
    }

    #endregion

    #region Optimization Helpers

    public void CacheScreenDimensions()
    {
        cachedScreenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
    }

    /// <summary>
    /// دریافت مرکز صفحه - با یا بدون Cache
    /// </summary>
    public Vector3 GetScreenCenter()
    {
        if (cacheScreenDimensions)
        {
            return cachedScreenCenter;  // ✅ Optimized
        }
        else
        {
            return new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);  // ❌ Allocation
        }
    }

    /// <summary>
    /// محاسبه فاصله - با یا بدون SqrMagnitude
    /// </summary>
    public bool IsInRange(Vector3 a, Vector3 b, float range)
    {
        if (useSqrMagnitude)
        {
            // ✅ Optimized: بدون Sqrt
            return (a - b).sqrMagnitude < range * range;
        }
        else
        {
            // ❌ Slow: شامل Sqrt
            return Vector3.Distance(a, b) < range;
        }
    }

    /// <summary>
    /// ثبت Raycast برای آمار
    /// </summary>
    public void RegisterRaycast()
    {
        raycastsThisFrame++;
    }

    /// <summary>
    /// ثبت Object Pool usage
    /// </summary>
    public void RegisterPooledObject()
    {
        objectsPooled++;
    }

    /// <summary>
    /// ثبت Instantiate usage
    /// </summary>
    public void RegisterInstantiate()
    {
        objectsInstantiated++;
    }

    #endregion

    #region Raycast Visualization

    /// <summary>
    /// رسم خط Raycast در Scene View
    /// </summary>
    public void DrawRaycastDebug(Ray ray, float distance, bool hit)
    {
        if (!showRaycastDebug) return;

        Color color = hit ? Color.green : Color.red;
        Debug.DrawRay(ray.origin, ray.direction * distance, color, 0.5f);
    }

    /// <summary>
    /// رسم نقطه برخورد
    /// </summary>
    public void DrawHitPoint(Vector3 point, Vector3 normal)
    {
        if (!showRaycastDebug) return;

        Debug.DrawLine(point, point + normal * 2f, Color.blue, 1f);
        
        // رسم X در نقطه برخورد
        Debug.DrawLine(point + Vector3.up * 0.5f, point - Vector3.up * 0.5f, Color.yellow, 1f);
        Debug.DrawLine(point + Vector3.right * 0.5f, point - Vector3.right * 0.5f, Color.yellow, 1f);
    }

    #endregion

    #region Gizmos

    void OnDrawGizmos()
    {
        if (!showSpawnPointGizmos) return;

        // نمایش Spawn Points
        var networkManager = FindFirstObjectByType<NetworkManager>();
        if (networkManager != null)
        {
            // این بخش در Editor قابل مشاهده است
            Gizmos.color = Color.cyan;
            // ... draw spawn points
        }
    }

    #endregion
}
