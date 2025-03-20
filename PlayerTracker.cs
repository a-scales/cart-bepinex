using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Reflection;
using System.Linq;

namespace BecomeCart;

/// <summary>
/// Tracks player objects to help identify connected players
/// and understand their models in the game world
/// </summary>
public class PlayerTracker : MonoBehaviour
{
    private class TrackedPlayer
    {
        public GameObject GameObject;
        public string Path;
        public int PlayerId;
        public string PlayerName;
        public Vector3 LastPosition;
        public bool IsLocal;
        public bool IsActive;
        public float LastUpdateTime;
        
        public TrackedPlayer(GameObject obj, int id, string name, bool isLocal)
        {
            GameObject = obj;
            Path = Debugging.GetGameObjectPath(obj);
            PlayerId = id;
            PlayerName = name;
            LastPosition = obj.transform.position;
            IsLocal = isLocal;
            IsActive = true;
            LastUpdateTime = Time.time;
        }
    }
    
    private static PlayerTracker _instance;
    private Dictionary<int, TrackedPlayer> _trackedPlayers = new Dictionary<int, TrackedPlayer>();
    private float _reportInterval = 5.0f; // Report every 5 seconds
    private float _lastReportTime = 0f;
    
    public static PlayerTracker Instance 
    {
        get 
        {
            if (_instance == null)
            {
                // Create a new GameObject to host our tracker
                GameObject trackerObject = new GameObject("PlayerTracker");
                DontDestroyOnLoad(trackerObject);
                _instance = trackerObject.AddComponent<PlayerTracker>();
            }
            return _instance;
        }
    }
    
    public void TrackPlayer(GameObject obj, int playerId, string playerName, bool isLocal)
    {
        if (obj == null) return;
        
        if (!_trackedPlayers.ContainsKey(playerId))
        {
            _trackedPlayers.Add(playerId, new TrackedPlayer(obj, playerId, playerName, isLocal));
            Plugin.Logger.LogInfo($"Started tracking player: {playerName} (ID: {playerId}) at {Debugging.GetGameObjectPath(obj)}");
        }
        else
        {
            // Update existing player data
            TrackedPlayer player = _trackedPlayers[playerId];
            player.GameObject = obj;
            player.Path = Debugging.GetGameObjectPath(obj);
            player.PlayerName = playerName;
            player.IsActive = true;
            player.LastUpdateTime = Time.time;
            Plugin.Logger.LogInfo($"Updated player: {playerName} (ID: {playerId}) at {Debugging.GetGameObjectPath(obj)}");
        }
    }
    
    public void RemovePlayer(int playerId)
    {
        if (_trackedPlayers.ContainsKey(playerId))
        {
            string playerName = _trackedPlayers[playerId].PlayerName;
            _trackedPlayers.Remove(playerId);
            Plugin.Logger.LogInfo($"Removed player: {playerName} (ID: {playerId})");
        }
    }
    
    public void ClearAllPlayers()
    {
        _trackedPlayers.Clear();
        Plugin.Logger.LogInfo("Cleared all tracked players");
    }
    
    private void Update()
    {
        // Update tracking for all players
        List<int> inactivePlayers = new List<int>();
        
        foreach (var kvp in _trackedPlayers)
        {
            int id = kvp.Key;
            TrackedPlayer player = kvp.Value;
            
            if (player.GameObject == null)
            {
                player.IsActive = false;
                // Mark for potential removal if inactive for too long
                if (Time.time - player.LastUpdateTime > 10f)
                {
                    inactivePlayers.Add(id);
                }
                continue;
            }
            
            // Update position
            player.LastPosition = player.GameObject.transform.position;
            player.LastUpdateTime = Time.time;
        }
        
        // Optionally remove inactive players
        // Commented out to keep history of players who have connected
        /*
        foreach (int id in inactivePlayers)
        {
            _trackedPlayers.Remove(id);
        }
        */
        
        // Report on tracked players periodically
        if (Time.time - _lastReportTime > _reportInterval)
        {
            ReportPlayers();
            _lastReportTime = Time.time;
        }
    }
    
    public void DumpPlayerDetails(int playerId)
    {
        if (!_trackedPlayers.ContainsKey(playerId))
        {
            Plugin.Logger.LogInfo($"No player found with ID: {playerId}");
            return;
        }
        
        TrackedPlayer player = _trackedPlayers[playerId];
        Plugin.Logger.LogInfo($"=== DETAILED PLAYER INFO: {player.PlayerName} (ID: {playerId}) ===");
        Plugin.Logger.LogInfo($"Path: {player.Path}");
        Plugin.Logger.LogInfo($"Local Player: {player.IsLocal}");
        Plugin.Logger.LogInfo($"Active: {player.IsActive}");
        Plugin.Logger.LogInfo($"Position: {player.LastPosition}");
        
        if (player.GameObject != null)
        {
            Plugin.Logger.LogInfo("Components:");
            Component[] components = player.GameObject.GetComponents<Component>();
            foreach (Component component in components)
            {
                if (component == null) continue;
                Plugin.Logger.LogInfo($"  Component: {component.GetType().Name}");
                Debugging.DumpFields(component);
            }
        }
    }
    
    private void ReportPlayers()
    {
        if (_trackedPlayers.Count == 0)
        {
            return;
        }
        
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== CONNECTED PLAYERS ===");
        sb.AppendLine($"Total players: {_trackedPlayers.Count}");
        
        foreach (var player in _trackedPlayers.Values)
        {
            string status = player.IsActive ? "Active" : "Inactive";
            string localTag = player.IsLocal ? "[LOCAL]" : "";
            sb.AppendLine($"Player: {player.PlayerName} (ID: {player.PlayerId}) {localTag} - {status}");
            sb.AppendLine($"  Path: {player.Path}");
            sb.AppendLine($"  Position: {player.LastPosition}");
        }
        
        Plugin.Logger.LogInfo(sb.ToString());
    }
    
    /// <summary>
    /// Dumps detailed information about all connected players
    /// </summary>
    public void DumpAllPlayerDetails()
    {
        Plugin.Logger.LogInfo("=== DUMPING ALL PLAYER DETAILS ===");
        
        if (_trackedPlayers.Count == 0)
        {
            Plugin.Logger.LogInfo("No players currently tracked");
            return;
        }
        
        foreach (var playerId in _trackedPlayers.Keys)
        {
            DumpPlayerDetails(playerId);
        }
    }
    
    /// <summary>
    /// Dumps all player details to a file
    /// </summary>
    public void DumpPlayersToFile(string filePath = "player_details.md")
    {
        if (_trackedPlayers.Count == 0)
        {
            Plugin.Logger.LogInfo("No players to dump to file");
            return;
        }
        
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("## Player Summary");
        sb.AppendLine($"- Total Players: {_trackedPlayers.Count}");
        sb.AppendLine("- Player IDs: " + string.Join(", ", _trackedPlayers.Keys));
        sb.AppendLine("- Player Names: " + string.Join(", ", _trackedPlayers.Values.Select(p => p.PlayerName)));
        sb.AppendLine();
        
        foreach (var player in _trackedPlayers.Values)
        {
            sb.AppendLine($"### Player: {player.PlayerName} (ID: {player.PlayerId})");
            sb.AppendLine($"- IsLocal: {player.IsLocal}");
            sb.AppendLine($"- IsActive: {player.IsActive}");
            sb.AppendLine($"- Path: {player.Path}");
            sb.AppendLine($"- Position: {player.LastPosition}");
            
            if (player.GameObject != null)
            {
                sb.AppendLine("#### Components:");
                Component[] components = player.GameObject.GetComponents<Component>();
                foreach (Component component in components)
                {
                    if (component == null) continue;
                    sb.AppendLine($"- Component: {component.GetType().Name}");
                    
                    // Dump fields for the component
                    Type type = component.GetType();
                    FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                    foreach (FieldInfo field in fields.OrderBy(f => f.Name))
                    {
                        try
                        {
                            object value = field.GetValue(component);
                            string valueStr = value == null ? "null" : value.ToString();
                            if (valueStr.Length > 100) valueStr = valueStr.Substring(0, 100) + "...";
                            sb.AppendLine($"  - {field.FieldType.Name} {field.Name} = {valueStr}");
                        }
                        catch (Exception ex)
                        {
                            sb.AppendLine($"  - {field.FieldType.Name} {field.Name} = <ERROR: {ex.Message}>");
                        }
                    }
                    
                    sb.AppendLine();
                }
                
                // Also dump children
                sb.AppendLine("#### Children:");
                DumpChildrenToStringBuilder(sb, player.GameObject, 0);
            }
            
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();
        }
        
        // Write to file
        try
        {
            System.IO.File.WriteAllText(filePath, sb.ToString());
            Plugin.Logger.LogInfo($"Successfully dumped player details to {filePath}");
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError($"Error writing player details to file: {ex.Message}");
        }
    }
    
    private void DumpChildrenToStringBuilder(StringBuilder sb, GameObject obj, int depth)
    {
        if (obj == null) return;
        
        string indent = new string(' ', depth * 2);
        sb.AppendLine($"{indent}- GameObject: {obj.name}, Active: {obj.activeSelf}, Layer: {LayerMask.LayerToName(obj.layer)}");
        
        // Log components
        Component[] components = obj.GetComponents<Component>();
        foreach (Component component in components)
        {
            if (component == null) continue;
            sb.AppendLine($"{indent}  - Component: {component.GetType().Name}");
        }
        
        // Recursively log children
        if (depth < 3) // Limit recursion depth to avoid huge files
        {
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                Transform child = obj.transform.GetChild(i);
                DumpChildrenToStringBuilder(sb, child.gameObject, depth + 1);
            }
        }
        else if (obj.transform.childCount > 0)
        {
            sb.AppendLine($"{indent}  - ... ({obj.transform.childCount} more children, not shown due to depth limit)");
        }
    }
} 