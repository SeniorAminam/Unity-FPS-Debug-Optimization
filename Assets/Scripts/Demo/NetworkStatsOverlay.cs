/**
 * Project: Multiplayer FPS
 * File: Assets/Scripts/Demo/NetworkStatsOverlay.cs
 * Author: Amin Davodian (Mohammadamin Davodian)
 * Website: https://senioramin.com
 * LinkedIn: https://linkedin.com/in/SudoAmin
 * GitHub: https://github.com/SeniorAminam
 * Created: 2025-12-07
 * 
 * Purpose: Network statistics overlay for debugging Photon connection
 * Developed by Amin Davodian
 */

using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Text;

/// <summary>
/// نمایش آمار شبکه Photon در صفحه
/// فعال/غیرفعال با F2
/// </summary>
public class NetworkStatsOverlay : MonoBehaviour
{
    [Header("📊 Settings")]
    [SerializeField] private bool showDetailedStats = true;
    [SerializeField] private float updateInterval = 0.5f;

    [Header("📈 Current Stats")]
    [SerializeField] private int currentPing = 0;
    [SerializeField] private string connectionState = "";
    [SerializeField] private string roomName = "";
    [SerializeField] private int playerCount = 0;
    [SerializeField] private int sentPackets = 0;
    [SerializeField] private int receivedPackets = 0;

    private float updateTimer = 0;
    private StringBuilder statsBuilder = new StringBuilder(256);
    private GUIStyle boxStyle;
    private GUIStyle labelStyle;
    private GUIStyle valueStyle;

    void Update()
    {
        updateTimer += Time.unscaledDeltaTime;
        
        if (updateTimer >= updateInterval)
        {
            UpdateStats();
            updateTimer = 0;
        }
    }

    void UpdateStats()
    {
        if (!PhotonNetwork.IsConnected)
        {
            connectionState = "Disconnected";
            currentPing = 0;
            return;
        }

        currentPing = PhotonNetwork.GetPing();
        connectionState = PhotonNetwork.NetworkClientState.ToString();
        
        if (PhotonNetwork.InRoom)
        {
            roomName = PhotonNetwork.CurrentRoom?.Name ?? "N/A";
            playerCount = PhotonNetwork.CurrentRoom?.PlayerCount ?? 0;
        }
        else
        {
            roomName = "Not in room";
            playerCount = 0;
        }

        // دریافت آمار Peer
        var peer = PhotonNetwork.NetworkingClient?.LoadBalancingPeer;
        if (peer != null)
        {
            sentPackets = peer.TrafficStatsGameLevel?.TotalOutgoingMessageCount ?? 0;
            receivedPackets = peer.TrafficStatsGameLevel?.TotalIncomingMessageCount ?? 0;
        }
    }

    void OnGUI()
    {
        var demoManager = DebugDemoManager.Instance;
        if (demoManager == null || !demoManager.showNetworkStats) return;

        InitStyles();
        DrawNetworkPanel();
    }

    void InitStyles()
    {
        if (boxStyle == null)
        {
            boxStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(10, 10, 10, 10)
            };
        }

        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 14
            };
        }

        if (valueStyle == null)
        {
            valueStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12
            };
        }
    }

    void DrawNetworkPanel()
    {
        float panelWidth = 280;
        float panelHeight = showDetailedStats ? 220 : 140;
        float x = Screen.width - panelWidth - 10;
        float y = 120;

        // پس‌زمینه
        GUI.color = new Color(0, 0, 0, 0.85f);
        GUI.Box(new Rect(x, y, panelWidth, panelHeight), "", boxStyle);
        GUI.color = Color.white;

        GUILayout.BeginArea(new Rect(x + 10, y + 10, panelWidth - 20, panelHeight - 20));

        // عنوان
        GUILayout.Label("🌐 Network Stats", labelStyle);
        GUILayout.Space(5);

        // وضعیت اتصال
        DrawStatusRow("Status:", connectionState, GetStatusColor());
        
        // Ping
        DrawStatusRow("Ping:", $"{currentPing} ms", GetPingColor(currentPing));

        if (PhotonNetwork.InRoom)
        {
            DrawStatusRow("Room:", roomName, Color.white);
            DrawStatusRow("Players:", playerCount.ToString(), Color.cyan);
        }

        if (showDetailedStats)
        {
            GUILayout.Space(10);
            GUILayout.Label("📊 Packets", labelStyle);
            DrawStatusRow("Sent:", sentPackets.ToString(), Color.green);
            DrawStatusRow("Received:", receivedPackets.ToString(), Color.yellow);
            
            // Server info
            if (PhotonNetwork.IsConnected)
            {
                GUILayout.Space(5);
                DrawStatusRow("Server:", PhotonNetwork.ServerAddress, Color.gray);
                DrawStatusRow("Region:", PhotonNetwork.CloudRegion ?? "N/A", Color.gray);
            }
        }

        GUILayout.EndArea();
    }

    void DrawStatusRow(string label, string value, Color valueColor)
    {
        GUILayout.BeginHorizontal();
        GUI.color = Color.gray;
        GUILayout.Label(label, valueStyle, GUILayout.Width(80));
        GUI.color = valueColor;
        GUILayout.Label(value, valueStyle);
        GUI.color = Color.white;
        GUILayout.EndHorizontal();
    }

    Color GetStatusColor()
    {
        if (!PhotonNetwork.IsConnected) return Color.red;
        if (PhotonNetwork.InRoom) return Color.green;
        if (PhotonNetwork.InLobby) return Color.yellow;
        return Color.white;
    }

    Color GetPingColor(int ping)
    {
        if (ping < 50) return Color.green;
        if (ping < 100) return Color.yellow;
        if (ping < 150) return new Color(1f, 0.5f, 0f); // Orange
        return Color.red;
    }

    /// <summary>
    /// لاگ کردن آمار شبکه در Console
    /// </summary>
    public void LogNetworkStats()
    {
        statsBuilder.Clear();
        statsBuilder.AppendLine("=== Network Stats ===");
        statsBuilder.AppendLine($"State: {connectionState}");
        statsBuilder.AppendLine($"Ping: {currentPing}ms");
        statsBuilder.AppendLine($"Room: {roomName}");
        statsBuilder.AppendLine($"Players: {playerCount}");
        statsBuilder.AppendLine($"Sent: {sentPackets} | Received: {receivedPackets}");
        
        Debug.Log(statsBuilder.ToString());
    }
}
