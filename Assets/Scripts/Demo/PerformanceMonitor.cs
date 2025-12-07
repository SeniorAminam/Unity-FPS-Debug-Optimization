/**
 * Project: Multiplayer FPS
 * File: Assets/Scripts/Demo/PerformanceMonitor.cs
 * Author: Amin Davodian (Mohammadamin Davodian)
 * Website: https://senioramin.com
 * LinkedIn: https://linkedin.com/in/SudoAmin
 * GitHub: https://github.com/SeniorAminam
 * Created: 2025-12-07
 * 
 * Purpose: Performance monitoring overlay for debugging
 * Developed by Amin Davodian
 */

using UnityEngine;
using UnityEngine.Profiling;
using System.Text;

/// <summary>
/// مانیتورینگ عملکرد در زمان اجرا
/// نمایش FPS, Memory, Draw Calls و GC
/// </summary>
public class PerformanceMonitor : MonoBehaviour
{
    [Header("📊 Display Settings")]
    [SerializeField] private bool showGraph = true;
    [SerializeField] private int graphWidth = 150;
    [SerializeField] private int graphHeight = 50;

    [Header("📈 Current Stats")]
    [SerializeField] private float currentFPS = 0;
    [SerializeField] private float averageFPS = 0;
    [SerializeField] private float minFPS = float.MaxValue;
    [SerializeField] private float maxFPS = 0;
    [SerializeField] private long usedMemory = 0;
    [SerializeField] private long totalMemory = 0;
    [SerializeField] private int gcCollectionCount = 0;

    // FPS calculation
    private float fpsUpdateInterval = 0.5f;
    private float fpsTimer = 0;
    private int frameCount = 0;
    private float totalFPS = 0;
    private int fpsReadings = 0;

    // FPS Graph
    private float[] fpsHistory = new float[150];
    private int fpsHistoryIndex = 0;
    private Texture2D graphTexture;
    private Color[] graphColors;

    // GC tracking
    private int lastGCCount = 0;
    private float lastGCTime = 0;
    private bool gcHappened = false;

    private StringBuilder statsBuilder = new StringBuilder(512);

    void Start()
    {
        InitGraphTexture();
        lastGCCount = System.GC.CollectionCount(0);
    }

    void InitGraphTexture()
    {
        graphTexture = new Texture2D(graphWidth, graphHeight);
        graphColors = new Color[graphWidth * graphHeight];
        
        // پس‌زمینه مشکی
        for (int i = 0; i < graphColors.Length; i++)
        {
            graphColors[i] = new Color(0, 0, 0, 0.7f);
        }
        graphTexture.SetPixels(graphColors);
        graphTexture.Apply();
    }

    void Update()
    {
        UpdateFPS();
        UpdateMemory();
        CheckGC();
        
        if (showGraph)
        {
            UpdateGraph();
        }
    }

    void UpdateFPS()
    {
        frameCount++;
        fpsTimer += Time.unscaledDeltaTime;

        if (fpsTimer >= fpsUpdateInterval)
        {
            currentFPS = frameCount / fpsTimer;
            
            // Track min/max
            if (currentFPS < minFPS) minFPS = currentFPS;
            if (currentFPS > maxFPS) maxFPS = currentFPS;
            
            // Calculate average
            totalFPS += currentFPS;
            fpsReadings++;
            averageFPS = totalFPS / fpsReadings;
            
            // Add to history
            fpsHistory[fpsHistoryIndex] = currentFPS;
            fpsHistoryIndex = (fpsHistoryIndex + 1) % fpsHistory.Length;
            
            // Reset
            frameCount = 0;
            fpsTimer = 0;
        }
    }

    void UpdateMemory()
    {
        usedMemory = Profiler.GetTotalAllocatedMemoryLong() / (1024 * 1024); // MB
        totalMemory = Profiler.GetTotalReservedMemoryLong() / (1024 * 1024); // MB
    }

    void CheckGC()
    {
        int currentGCCount = System.GC.CollectionCount(0);
        if (currentGCCount > lastGCCount)
        {
            gcCollectionCount = currentGCCount;
            gcHappened = true;
            lastGCTime = Time.time;
            lastGCCount = currentGCCount;
            
            // GC event tracked internally - no console spam
            // The UI will show the warning icon when GC happens
        }
        else if (Time.time - lastGCTime > 2f)
        {
            gcHappened = false;
        }
    }

    void UpdateGraph()
    {
        // Clear graph
        for (int i = 0; i < graphColors.Length; i++)
        {
            graphColors[i] = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        }

        // Draw 60 FPS line
        int line60 = (int)(60f / 120f * graphHeight);
        for (int x = 0; x < graphWidth; x++)
        {
            graphColors[line60 * graphWidth + x] = new Color(0.3f, 0.3f, 0.3f, 1f);
        }

        // Draw 30 FPS line
        int line30 = (int)(30f / 120f * graphHeight);
        for (int x = 0; x < graphWidth; x++)
        {
            graphColors[line30 * graphWidth + x] = new Color(0.2f, 0.2f, 0.2f, 1f);
        }

        // Draw FPS history
        for (int i = 0; i < fpsHistory.Length; i++)
        {
            int x = i;
            int histIndex = (fpsHistoryIndex + i) % fpsHistory.Length;
            float fps = fpsHistory[histIndex];
            int y = Mathf.Clamp((int)(fps / 120f * graphHeight), 0, graphHeight - 1);
            
            Color color = GetFPSColor(fps);
            graphColors[y * graphWidth + x] = color;
            
            // Fill below
            for (int fillY = 0; fillY < y; fillY++)
            {
                graphColors[fillY * graphWidth + x] = color * 0.3f;
            }
        }

        graphTexture.SetPixels(graphColors);
        graphTexture.Apply();
    }

    Color GetFPSColor(float fps)
    {
        if (fps >= 55) return Color.green;
        if (fps >= 30) return Color.yellow;
        return Color.red;
    }

    void OnGUI()
    {
        var demoManager = DebugDemoManager.Instance;
        if (demoManager == null || !demoManager.showPerformanceStats) return;

        DrawPerformancePanel();
    }

    void DrawPerformancePanel()
    {
        float x = 10;
        float y = Screen.height - 180;
        float width = 200;
        float height = 170;

        // پس‌زمینه
        GUI.color = new Color(0, 0, 0, 0.85f);
        GUI.Box(new Rect(x, y, width, height), "");
        GUI.color = Color.white;

        GUILayout.BeginArea(new Rect(x + 10, y + 10, width - 20, height - 20));

        // عنوان
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 14
        };
        GUILayout.Label("📊 Performance", titleStyle);

        // FPS با رنگ
        GUI.color = GetFPSColor(currentFPS);
        GUILayout.Label($"FPS: {currentFPS:F1} (Avg: {averageFPS:F1})");
        GUI.color = Color.white;

        GUILayout.Label($"Min: {minFPS:F0} | Max: {maxFPS:F0}");

        // Memory
        float memPercent = (usedMemory / (float)totalMemory) * 100f;
        GUI.color = memPercent > 80 ? Color.red : Color.white;
        GUILayout.Label($"Memory: {usedMemory} / {totalMemory} MB");
        GUI.color = Color.white;

        // GC
        GUI.color = gcHappened ? Color.red : Color.white;
        GUILayout.Label($"GC Count: {gcCollectionCount} {(gcHappened ? "⚠️" : "")}");
        GUI.color = Color.white;

        GUILayout.EndArea();

        // Graph
        if (showGraph && graphTexture != null)
        {
            GUI.DrawTexture(new Rect(x, y - graphHeight - 5, graphWidth, graphHeight), graphTexture);
        }
    }

    /// <summary>
    /// ریست آمار
    /// </summary>
    public void ResetStats()
    {
        minFPS = float.MaxValue;
        maxFPS = 0;
        totalFPS = 0;
        fpsReadings = 0;
        
        for (int i = 0; i < fpsHistory.Length; i++)
        {
            fpsHistory[i] = 0;
        }
        
        Debug.Log("[Performance] Stats Reset");
    }

    void OnDestroy()
    {
        if (graphTexture != null)
        {
            Destroy(graphTexture);
        }
    }
}
