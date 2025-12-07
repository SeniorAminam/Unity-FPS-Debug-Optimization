/**
 * Project: Multiplayer FPS
 * File: Assets/Scripts/Demo/DemoAutoInitializer.cs
 * Author: Amin Davodian (Mohammadamin Davodian)
 * Website: https://senioramin.com
 * LinkedIn: https://linkedin.com/in/SudoAmin
 * GitHub: https://github.com/SeniorAminam
 * Created: 2025-12-07
 * 
 * Purpose: Automatically initializes Demo system without manual scene setup
 * Developed by Amin Davodian
 */

using UnityEngine;

/// <summary>
/// این کلاس به صورت خودکار Demo Manager را در شروع بازی ایجاد می‌کند
/// بدون نیاز به اضافه کردن دستی به Scene
/// </summary>
public static class DemoAutoInitializer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        // Check if demo manager already exists
        if (DebugDemoManager.Instance != null)
        {
            Debug.Log("[DemoAutoInit] DebugDemoManager already exists.");
            return;
        }
        
        // Create Demo Manager GameObject
        GameObject demoManager = new GameObject("--- DEMO MANAGER (Auto) ---");
        demoManager.AddComponent<DebugDemoManager>();
        
        // Try to add other components
        TryAddComponent<NetworkStatsOverlay>(demoManager);
        TryAddComponent<PerformanceMonitor>(demoManager);
        
        Debug.Log("[DemoAutoInit] ✅ Demo system initialized automatically!");
        Debug.Log("[DemoAutoInit] 🎮 Press F1-F8 to toggle demo features");
    }
    
    private static void TryAddComponent<T>(GameObject go) where T : Component
    {
        try
        {
            go.AddComponent<T>();
        }
        catch
        {
            // Component not found, skip
        }
    }
}
