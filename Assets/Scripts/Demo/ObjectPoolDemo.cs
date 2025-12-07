/**
 * Project: Multiplayer FPS
 * File: Assets/Scripts/Demo/ObjectPoolDemo.cs
 * Author: Amin Davodian (Mohammadamin Davodian)
 * Website: https://senioramin.com
 * LinkedIn: https://linkedin.com/in/SudoAmin
 * GitHub: https://github.com/SeniorAminam
 * Created: 2025-12-07
 * 
 * Purpose: Object Pooling demo for live presentation
 * Developed by Amin Davodian
 */

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// سیستم Object Pooling برای دمو
/// مقایسه Instantiate vs Pool
/// </summary>
public class ObjectPoolDemo : MonoBehaviour
{
    public static ObjectPoolDemo Instance { get; private set; }

    [Header("🎯 Pool Settings")]
    [SerializeField] private int initialPoolSize = 20;
    
    [Header("📊 Statistics")]
    [SerializeField] private int totalInstantiated = 0;
    [SerializeField] private int totalFromPool = 0;
    [SerializeField] private int currentPoolSize = 0;
    [SerializeField] private int activeObjects = 0;

    // Dictionary برای نگهداری Pool های مختلف
    private Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, GameObject> prefabLookup = new Dictionary<string, GameObject>();
    private HashSet<GameObject> activePoolObjects = new HashSet<GameObject>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[ObjectPoolDemo] 🎱 Object Pool Demo Initialized!");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// ثبت Prefab برای Pooling
    /// </summary>
    public void RegisterPrefab(string name, GameObject prefab, int preWarmCount = 10)
    {
        if (pools.ContainsKey(name))
        {
            Debug.LogWarning($"[ObjectPoolDemo] Prefab '{name}' already registered!");
            return;
        }

        pools[name] = new Queue<GameObject>();
        prefabLookup[name] = prefab;

        // Pre-warm: ایجاد آبجکت‌ها از قبل
        for (int i = 0; i < preWarmCount; i++)
        {
            var obj = CreateNewObject(name);
            obj.SetActive(false);
            pools[name].Enqueue(obj);
        }

        currentPoolSize = GetTotalPoolSize();
        Debug.Log($"[ObjectPoolDemo] Registered '{name}' with {preWarmCount} objects");
    }

    /// <summary>
    /// دریافت آبجکت از Pool یا Instantiate بر اساس Demo Mode
    /// </summary>
    public GameObject Spawn(string prefabName, Vector3 position, Quaternion rotation)
    {
        var demoManager = DebugDemoManager.Instance;
        
        if (demoManager != null && demoManager.useObjectPooling && pools.ContainsKey(prefabName))
        {
            // ✅ استفاده از Pool
            return SpawnFromPool(prefabName, position, rotation);
        }
        else
        {
            // ❌ Instantiate معمولی
            return SpawnInstantiate(prefabName, position, rotation);
        }
    }

    /// <summary>
    /// Spawn با Object Pool (بهینه)
    /// </summary>
    private GameObject SpawnFromPool(string name, Vector3 position, Quaternion rotation)
    {
        if (!pools.ContainsKey(name))
        {
            Debug.LogError($"[ObjectPoolDemo] Pool '{name}' not found!");
            return null;
        }

        GameObject obj;
        
        if (pools[name].Count > 0)
        {
            // از Pool بگیر
            obj = pools[name].Dequeue();
        }
        else
        {
            // Pool خالی است، یکی جدید بساز
            obj = CreateNewObject(name);
        }

        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);
        activePoolObjects.Add(obj);
        
        totalFromPool++;
        activeObjects = activePoolObjects.Count;
        
        if (DebugDemoManager.Instance != null)
        {
            DebugDemoManager.Instance.RegisterPooledObject();
        }

        Debug.Log($"[Pool] ✅ Got '{name}' from pool (Total: {totalFromPool})");
        return obj;
    }

    /// <summary>
    /// Spawn با Instantiate (غیر بهینه)
    /// </summary>
    private GameObject SpawnInstantiate(string name, Vector3 position, Quaternion rotation)
    {
        if (!prefabLookup.ContainsKey(name))
        {
            Debug.LogError($"[ObjectPoolDemo] Prefab '{name}' not registered!");
            return null;
        }

        var obj = Instantiate(prefabLookup[name], position, rotation);
        
        totalInstantiated++;
        
        if (DebugDemoManager.Instance != null)
        {
            DebugDemoManager.Instance.RegisterInstantiate();
        }

        Debug.LogWarning($"[Pool] ❌ Instantiated '{name}' (Total: {totalInstantiated}) - NOT OPTIMIZED!");
        
        // Auto destroy after some time
        Destroy(obj, 2f);
        
        return obj;
    }

    /// <summary>
    /// برگرداندن آبجکت به Pool
    /// </summary>
    public void Return(string name, GameObject obj)
    {
        if (!pools.ContainsKey(name))
        {
            Debug.LogWarning($"[ObjectPoolDemo] Pool '{name}' not found, destroying object");
            Destroy(obj);
            return;
        }

        obj.SetActive(false);
        pools[name].Enqueue(obj);
        activePoolObjects.Remove(obj);
        
        activeObjects = activePoolObjects.Count;
        currentPoolSize = GetTotalPoolSize();
        
        Debug.Log($"[Pool] ♻️ Returned '{name}' to pool");
    }

    /// <summary>
    /// خودکار برگرداندن به Pool بعد از مدتی
    /// </summary>
    public void ReturnAfterDelay(string name, GameObject obj, float delay)
    {
        StartCoroutine(ReturnCoroutine(name, obj, delay));
    }

    private System.Collections.IEnumerator ReturnCoroutine(string name, GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (obj != null && obj.activeInHierarchy)
        {
            Return(name, obj);
        }
    }

    private GameObject CreateNewObject(string name)
    {
        if (!prefabLookup.ContainsKey(name))
        {
            Debug.LogError($"[ObjectPoolDemo] Prefab '{name}' not found!");
            return null;
        }

        var obj = Instantiate(prefabLookup[name], transform);
        obj.name = $"{name}_pooled";
        return obj;
    }

    private int GetTotalPoolSize()
    {
        int total = 0;
        foreach (var pool in pools.Values)
        {
            total += pool.Count;
        }
        return total;
    }

    /// <summary>
    /// دریافت آمار برای نمایش
    /// </summary>
    public string GetStatistics()
    {
        return $"Pool: {totalFromPool} | Instantiate: {totalInstantiated} | Active: {activeObjects}";
    }
}
