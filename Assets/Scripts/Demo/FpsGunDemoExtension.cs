/**
 * Project: Multiplayer FPS
 * File: Assets/Scripts/Demo/FpsGunDemoExtension.cs
 * Author: Amin Davodian (Mohammadamin Davodian)
 * Website: https://senioramin.com
 * LinkedIn: https://linkedin.com/in/SudoAmin
 * GitHub: https://github.com/SeniorAminam
 * Created: 2025-12-07
 * 
 * Purpose: Extension for FpsGun to add demo/debug features
 * Developed by Amin Davodian
 */

using UnityEngine;

/// <summary>
/// افزونه دمو برای FpsGun
/// این کلاس قابلیت‌های دمو و دیباگ به سیستم تیراندازی اضافه می‌کند
/// </summary>
public class FpsGunDemoExtension : MonoBehaviour
{
    [Header("📊 Demo Statistics")]
    [SerializeField] private int totalShots = 0;
    [SerializeField] private int totalHits = 0;
    [SerializeField] private int totalMisses = 0;
    [SerializeField] private float hitPercentage = 0;
    
    [Header("🎯 Last Shot Info")]
    [SerializeField] private string lastHitObject = "";
    [SerializeField] private Vector3 lastHitPoint;
    [SerializeField] private float lastHitDistance;
    
    [Header("📍 References")]
    [SerializeField] private Camera raycastCamera;

    private Vector3 cachedScreenCenter;
    private bool screenCenterCached = false;

    void Awake()
    {
        CacheScreenCenter();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            CacheScreenCenter();
        }
    }

    void CacheScreenCenter()
    {
        cachedScreenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        screenCenterCached = true;
        Debug.Log($"[FpsGunDemo] Screen center cached: {cachedScreenCenter}");
    }

    /// <summary>
    /// دریافت مرکز صفحه - مقایسه Cached vs Non-Cached
    /// </summary>
    public Vector3 GetScreenCenter()
    {
        var demoManager = DebugDemoManager.Instance;
        
        if (demoManager != null && demoManager.cacheScreenDimensions && screenCenterCached)
        {
            // ✅ OPTIMIZED: از Cache استفاده می‌کنیم
            // در Profiler: 0 GC Alloc
            return cachedScreenCenter;
        }
        else
        {
            // ❌ NOT OPTIMIZED: هر بار new Vector3
            // در Profiler: 12 bytes GC Alloc
            return new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        }
    }

    /// <summary>
    /// ثبت نتیجه تیراندازی برای آمار
    /// </summary>
    public void RecordShot(bool hit, RaycastHit hitInfo = default)
    {
        totalShots++;
        
        if (hit)
        {
            totalHits++;
            lastHitObject = hitInfo.transform?.name ?? "Unknown";
            lastHitPoint = hitInfo.point;
            lastHitDistance = hitInfo.distance;
            
            // رسم Debug در Scene View
            DrawHitDebug(hitInfo);
        }
        else
        {
            totalMisses++;
        }
        
        hitPercentage = (totalShots > 0) ? (totalHits / (float)totalShots) * 100f : 0;
    }

    /// <summary>
    /// رسم Debug برای برخورد
    /// </summary>
    void DrawHitDebug(RaycastHit hit)
    {
        var demoManager = DebugDemoManager.Instance;
        if (demoManager == null || !demoManager.showRaycastDebug) return;

        // رسم Normal
        Debug.DrawLine(hit.point, hit.point + hit.normal * 2f, Color.blue, 1f);
        
        // رسم علامت X در نقطه برخورد
        float size = 0.3f;
        Debug.DrawLine(hit.point + Vector3.up * size, hit.point - Vector3.up * size, Color.yellow, 1f);
        Debug.DrawLine(hit.point + Vector3.right * size, hit.point - Vector3.right * size, Color.yellow, 1f);
        Debug.DrawLine(hit.point + Vector3.forward * size, hit.point - Vector3.forward * size, Color.yellow, 1f);
        
        // لاگ با جزئیات
        Debug.Log($"[FpsGunDemo] 🎯 Hit: {hit.transform.name} | Distance: {hit.distance:F1}m | Point: {hit.point}");
    }

    /// <summary>
    /// رسم Raycast در Scene View
    /// </summary>
    public void DrawRayDebug(Ray ray, float distance, bool hit)
    {
        var demoManager = DebugDemoManager.Instance;
        if (demoManager == null || !demoManager.showRaycastDebug) return;

        Color lineColor = hit ? Color.green : Color.red;
        Debug.DrawRay(ray.origin, ray.direction * distance, lineColor, 0.5f);
        
        // نمایش مبدأ
        Debug.DrawLine(ray.origin + Vector3.up * 0.1f, ray.origin - Vector3.up * 0.1f, Color.cyan, 0.5f);
    }

    /// <summary>
    /// دریافت آمار برای نمایش
    /// </summary>
    public string GetStats()
    {
        return $"Shots: {totalShots} | Hits: {totalHits} | Accuracy: {hitPercentage:F1}%";
    }

    void OnGUI()
    {
        var demoManager = DebugDemoManager.Instance;
        if (demoManager == null || !demoManager.showPerformanceStats) return;

        // نمایش آمار تیراندازی
        GUILayout.BeginArea(new Rect(Screen.width - 250, 10, 240, 100));
        GUI.color = new Color(0, 0, 0, 0.7f);
        GUILayout.Box("", GUILayout.Width(230), GUILayout.Height(90));
        GUI.color = Color.white;
        
        GUILayout.BeginArea(new Rect(Screen.width - 240, 20, 220, 80));
        GUILayout.Label("🎯 Shooting Stats");
        GUILayout.Label($"Total Shots: {totalShots}");
        GUILayout.Label($"Hits: {totalHits} | Misses: {totalMisses}");
        GUILayout.Label($"Accuracy: {hitPercentage:F1}%");
        GUILayout.EndArea();
        
        GUILayout.EndArea();
    }
}
