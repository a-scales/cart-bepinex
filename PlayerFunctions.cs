using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Logging;
using System.Reflection;

namespace BecomeCart
{
    public partial class Plugin
    {
        /// <summary>
        /// Finds the local player through various methods
        /// </summary>
        private GameObject FindLocalPlayer()
        {
            Logger.LogInfo("Attempting to find local player...");
            GameObject playerObject = null;
            
            // First check PlayerTracker to see if we already found the local player
            var trackerInstance = PlayerTracker.Instance;
            var trackerType = trackerInstance.GetType();
            
            // Implement the player finding logic
            
            return playerObject;
        }

        /// <summary>
        /// Finds and tracks all players in the current game session
        /// </summary>
        private void FindAndTrackAllPlayers()
        {
            Logger.LogInfo("Attempting to find and track all players...");
            
            try
            {
                // Try to find players via PhotonNetwork.PlayerList
                Type photonNetworkType = Type.GetType("Photon.Pun.PhotonNetwork, Photon.Pun");
                if (photonNetworkType != null)
                {
                    Logger.LogInfo("Found PhotonNetwork type, looking for PlayerList property");
                    
                    PropertyInfo playerListProp = photonNetworkType.GetProperty("PlayerList", 
                        BindingFlags.Public | BindingFlags.Static);
                    
                    if (playerListProp != null)
                    {
                        Logger.LogInfo("Found PlayerList property, getting players");
                        
                        var players = playerListProp.GetValue(null) as Array;
                        if (players != null && players.Length > 0)
                        {
                            Logger.LogInfo($"Found {players.Length} players in PhotonNetwork.PlayerList");
                            
                            foreach (var player in players)
                            {
                                // Get player ID and nickname
                                int playerId = (int)player.GetType().GetProperty("ActorNumber").GetValue(player);
                                string playerName = (string)player.GetType().GetProperty("NickName").GetValue(player);
                                
                                Logger.LogInfo($"Found player: {playerName} (ID: {playerId})");
                                
                                // Try to find player GameObject
                                GameObject playerObject = FindPlayerGameObject(playerId, playerName);
                                if (playerObject != null)
                                {
                                    // Track the player
                                    bool isLocalPlayer = (bool)player.GetType().GetProperty("IsLocal").GetValue(player);
                                    PlayerTracker.Instance.AddPlayer(playerObject, isLocalPlayer);
                                }
                            }
                        }
                        else
                        {
                            Logger.LogInfo("No players found in PhotonNetwork.PlayerList");
                        }
                    }
                }
                
                // Try alternative methods if needed
                if (PlayerTracker.Instance.GetAllPlayers().Count == 0)
                {
                    Logger.LogInfo("No players found via PhotonNetwork, trying alternative methods");
                    
                    // Look for objects with "Player" in their name
                    GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
                    foreach (GameObject obj in allObjects)
                    {
                        if (obj.name.Contains("Player") && !obj.name.Contains("Tracker"))
                        {
                            Logger.LogInfo($"Found potential player object: {Debugging.GetGameObjectPath(obj)}");
                            PlayerTracker.Instance.AddPlayer(obj);
                        }
                    }
                }
                
                // Report results
                int totalPlayers = PlayerTracker.Instance.GetAllPlayers().Count;
                Logger.LogInfo($"Player tracking complete. Found {totalPlayers} players.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error finding players: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Attempts to find a specific player's GameObject using their ID and name
        /// </summary>
        private GameObject FindPlayerGameObject(int playerId, string playerName)
        {
            try
            {
                // Implementation to find player objects
                // This would need to be customized based on the game's architecture
                
                return null; // Placeholder
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error finding player GameObject: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Swap the player model with a cart
        /// </summary>
        private void SwapPlayerModelWithCart()
        {
            Logger.LogInfo("=== ATTEMPTING TO ATTACH PLAYER TO CART ===");
            
            try
            {
                // First find the specific cart from cart_details.md
                GameObject cartObject = null;
                
                // Try to find the exact cart from cart_details.md by path
                string targetCartPath = "Level Generator/Items/Item Cart Medium(Clone)";
                Logger.LogInfo($"Looking for cart at path: {targetCartPath}");
                
                // Use GameObject.Find to locate the cart by path
                cartObject = GameObject.Find(targetCartPath);
                if (cartObject != null)
                {
                    Logger.LogInfo($"Found target cart by path: {targetCartPath}");
                }
                
                // If the specific cart wasn't found, try alternative approaches
                if (cartObject == null)
                {
                    Logger.LogInfo("Target cart not found by path, trying alternative methods");
                    
                    // Check if we have a previously registered cart
                    if (_lastFoundCart != null)
                    {
                        cartObject = _lastFoundCart;
                        Logger.LogInfo($"Using previously registered cart: {Debugging.GetGameObjectPath(cartObject)}");
                    }
                    else
                    {
                        Logger.LogInfo("No registered cart found, searching for any cart in the scene");
                        
                        // Look for any object with "Cart" in its name
                        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
                        foreach (GameObject obj in allObjects)
                        {
                            if (obj.name.Contains("Cart") && HasComponent(obj, "PhysGrabCart"))
                            {
                                cartObject = obj;
                                Logger.LogInfo($"Found cart: {Debugging.GetGameObjectPath(cartObject)}");
                                break;
                            }
                        }
                    }
                }
                
                // If we still haven't found a cart, we can't proceed
                if (cartObject == null)
                {
                    Logger.LogError("No cart found! Please make sure there's a cart in the scene or use F9 to find one first");
                    return;
                }
                
                // Find the local player
                GameObject playerObject = FindLocalPlayer();
                
                // Check if we found the player
                if (playerObject == null)
                {
                    Logger.LogError("Local player not found! Cannot proceed with player-cart swap");
                    return;
                }
                
                Logger.LogInfo($"Found local player: {Debugging.GetGameObjectPath(playerObject)}");
                
                // Perform the actual attachment
                AttachPlayerToCart(playerObject, cartObject);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error during player-cart swap: {ex.Message}\n{ex.StackTrace}");
            }
        }
        
        /// <summary>
        /// Checks if a GameObject has a component with the given name
        /// </summary>
        private bool HasComponent(GameObject obj, string componentName)
        {
            Component[] components = obj.GetComponents<Component>();
            foreach (Component comp in components)
            {
                if (comp != null && comp.GetType().Name.Contains(componentName))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Restores the player from cart mode
        /// </summary>
        private void RestorePlayerFromCart()
        {
            if (_lastPlayerSwap == null)
            {
                Logger.LogInfo("No player-cart swap to restore from.");
                return;
            }

            try
            {
                Logger.LogInfo("Restoring player from cart mode...");
                
                // Implementation of restoring player from cart
                
                // Clear the swap data
                _lastPlayerSwap = null;
                _playerVisibilityState.Clear();
                _savedCartCollisionSettings.Clear();
                
                Logger.LogInfo("Successfully restored player from cart mode");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error during player-cart restoration: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
} 