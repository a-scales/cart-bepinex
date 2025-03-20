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
    
    // Dictionary mapping player GameObjects to their Photon actor numbers
    private Dictionary<GameObject, int> _playerActorNumbers = new Dictionary<GameObject, int>();
    
    // Dictionary mapping Photon actor numbers to player GameObjects
    private Dictionary<int, GameObject> _actorNumberToPlayer = new Dictionary<int, GameObject>();
    
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
            
            // Try to find PhotonView and get actor number
            MonoBehaviour photonView = player.GetComponentInChildren<MonoBehaviour>();
            if (photonView != null && photonView.GetType().Name == "PhotonView")
            {
                try
                {
                    // Use reflection to get OwnerActorNr
                    FieldInfo ownerActorNrField = photonView.GetType().GetField("ownerActorNr", 
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        
                    if (ownerActorNrField != null)
                    {
                        int actorNumber = (int)ownerActorNrField.GetValue(photonView);
                        _playerActorNumbers[player] = actorNumber;
                        _actorNumberToPlayer[actorNumber] = player;
                        
                        Plugin.Logger.LogInfo($"Player {Debugging.GetGameObjectPath(player)} has Photon actor number: {actorNumber}");
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Logger.LogError($"Error getting PhotonView actor number: {ex.Message}");
                }
            }
        }
    }
    
    /// <summary>
    /// Gets the Photon actor number for a player
    /// </summary>
    /// <param name="player">The player GameObject</param>
    /// <returns>The actor number or -1 if not found</returns>
    public int GetPlayerActorNumber(GameObject player)
    {
        if (player == null || !_playerActorNumbers.ContainsKey(player))
            return -1;
            
        return _playerActorNumbers[player];
    }
    
    /// <summary>
    /// Gets a player by their Photon actor number
    /// </summary>
    /// <param name="actorNumber">The actor number to look for</param>
    /// <returns>The player GameObject or null if not found</returns>
    public GameObject GetPlayerByActorNumber(int actorNumber)
    {
        if (_actorNumberToPlayer.ContainsKey(actorNumber))
            return _actorNumberToPlayer[actorNumber];
            
        return null;
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
        _playerActorNumbers.Clear();
        _actorNumberToPlayer.Clear();
        Plugin.Logger.LogInfo("Cleared all tracked players");
    }
} 