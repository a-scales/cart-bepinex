using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Reflection;
using System.Linq;
using BepInEx.Logging;

namespace BecomeCart;

/// <summary>
/// Singleton class to track and manage player game objects
/// </summary>
public class PlayerTracker
{
    // Singleton instance
    private static PlayerTracker _instance;
    public static PlayerTracker Instance 
    {
        get 
        {
            if (_instance == null)
            {
                _instance = new PlayerTracker();
            }
            return _instance;
        }
    }

    // List of tracked players
    private List<GameObject> _trackedPlayers = new List<GameObject>();
    
    // Local player reference
    private GameObject _localPlayer;
    
    // Public property to access local player
    public GameObject LocalPlayer => _localPlayer;
    
    // Private constructor for singleton
    private PlayerTracker() { }
    
    /// <summary>
    /// Adds a player to the tracking list
    /// </summary>
    /// <param name="player">Player GameObject to track</param>
    /// <param name="isLocalPlayer">Whether this is the local player</param>
    public void AddPlayer(GameObject player, bool isLocalPlayer = false)
    {
        if (player == null)
            return;
            
        if (!_trackedPlayers.Contains(player))
        {
            _trackedPlayers.Add(player);
            Plugin.Logger.LogInfo($"Added player to tracker: {Debugging.GetGameObjectPath(player)}");
            
            if (isLocalPlayer)
            {
                _localPlayer = player;
                Plugin.Logger.LogInfo($"Set as local player: {Debugging.GetGameObjectPath(player)}");
            }
        }
    }
    
    /// <summary>
    /// Gets all tracked players
    /// </summary>
    public List<GameObject> GetAllPlayers()
    {
        return new List<GameObject>(_trackedPlayers);
    }
    
    /// <summary>
    /// Clears all tracked players
    /// </summary>
    public void ClearPlayers()
    {
        _trackedPlayers.Clear();
        _localPlayer = null;
        Plugin.Logger.LogInfo("Cleared all tracked players");
    }
} 