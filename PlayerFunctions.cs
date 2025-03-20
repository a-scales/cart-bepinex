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
            if (trackerInstance.LocalPlayer != null)
            {
                playerObject = trackerInstance.LocalPlayer;
                Logger.LogInfo($"Using cached local player from PlayerTracker: {Debugging.GetGameObjectPath(playerObject)}");
                return playerObject;
            }
            
            try
            {
                // Find all PhotonView components in the scene
                MonoBehaviour[] allPhotonViews = GameObject.FindObjectsOfType<MonoBehaviour>();
                Logger.LogInfo($"Found {allPhotonViews.Length} MonoBehaviours to check for PhotonViews");
                
                foreach (MonoBehaviour behavior in allPhotonViews)
                {
                    if (behavior == null) continue;
                    
                    // Check if this is a PhotonView
                    if (behavior.GetType().Name == "PhotonView")
                    {
                        try
                        {
                            // Try to access the IsMine property
                            PropertyInfo isMineProperty = behavior.GetType().GetProperty("IsMine");
                            if (isMineProperty != null)
                            {
                                bool isMine = (bool)isMineProperty.GetValue(behavior);
                                
                                if (isMine)
                                {
                                    // This is a PhotonView owned by the local player
                                    GameObject viewObject = behavior.gameObject;
                                    Logger.LogInfo($"Found PhotonView with IsMine=true: {Debugging.GetGameObjectPath(viewObject)}");
                                    
                                    // Check if this is a player object - it should have PlayerTumble or PlayerAvatar component
                                    // or its path should contain "Player"
                                    bool isPlayerObject = false;
                                    string path = Debugging.GetGameObjectPath(viewObject);
                                    
                                    if (path.Contains("Player") && !path.Contains("Manager") && !path.Contains("UI"))
                                    {
                                        isPlayerObject = true;
                                    }
                                    else
                                    {
                                        // Check for player-related components
                                        Component[] components = viewObject.GetComponents<Component>();
                                        foreach (Component comp in components)
                                        {
                                            if (comp != null && comp.GetType().Name.Contains("Player"))
                                            {
                                                isPlayerObject = true;
                                                break;
                                            }
                                        }
                                    }
                                    
                                    if (isPlayerObject)
                                    {
                                        // Found the player
                                        playerObject = viewObject;
                                        Logger.LogInfo($"Found local player with PhotonView: {Debugging.GetGameObjectPath(playerObject)}");
                                        
                                        // Also register with the tracker
                                        trackerInstance.AddPlayer(playerObject, true);
                                        
                                        break;
                                    }
                                    else
                                    {
                                        // This PhotonView is owned by the local player, but it's not the player object
                                        // Check if its parent might be the player
                                        Transform parent = viewObject.transform.parent;
                                        while (parent != null)
                                        {
                                            string parentPath = Debugging.GetGameObjectPath(parent.gameObject);
                                            if (parentPath.Contains("Player") && !parentPath.Contains("Manager") && !parentPath.Contains("UI"))
                                            {
                                                playerObject = parent.gameObject;
                                                Logger.LogInfo($"Found local player (parent of PhotonView): {Debugging.GetGameObjectPath(playerObject)}");
                                                
                                                // Register with tracker
                                                trackerInstance.AddPlayer(playerObject, true);
                                                break;
                                            }
                                            parent = parent.parent;
                                        }
                                        
                                        if (playerObject != null)
                                            break;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError($"Error checking PhotonView: {ex.Message}");
                        }
                    }
                }
                
                // If we still haven't found the player, try alternative approaches
                if (playerObject == null)
                {
                    Logger.LogInfo("Player not found via PhotonViews, trying alternative methods...");
                    
                    // Try to find objects with "PlayerAvatar" or similar
                    GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
                    
                    // First try to find PlayerAvatar objects
                    foreach (GameObject obj in allObjects)
                    {
                        if (obj.name.Contains("PlayerAvatar") || obj.name.Contains("Player Tumble"))
                        {
                            // Check if this contains a PhysGrabObject component (from player_details.md)
                            if (ComponentUtility.HasComponentByName(obj, "PhysGrabObject"))
                            {
                                playerObject = obj;
                                Logger.LogInfo($"Found player by name and components: {Debugging.GetGameObjectPath(playerObject)}");
                                
                                // Register with tracker
                                trackerInstance.AddPlayer(playerObject, true);
                                break;
                            }
                        }
                    }
                    
                    // If still not found, try Camera.main's parent chain
                    if (playerObject == null && Camera.main != null)
                    {
                        Logger.LogInfo("Trying to find player via Camera.main's parent chain");
                        
                        Transform cameraParent = Camera.main.transform.parent;
                        while (cameraParent != null)
                        {
                            if (cameraParent.name.Contains("Player") || ComponentUtility.HasComponentByName(cameraParent.gameObject, "Player"))
                            {
                                playerObject = cameraParent.gameObject;
                                Logger.LogInfo($"Found player via Camera.main's parent: {Debugging.GetGameObjectPath(playerObject)}");
                                
                                // Register with tracker
                                trackerInstance.AddPlayer(playerObject, true);
                                break;
                            }
                            cameraParent = cameraParent.parent;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error finding local player: {ex.Message}\n{ex.StackTrace}");
            }
            
            if (playerObject == null)
            {
                Logger.LogError("Local player not found after trying multiple methods!");
            }
            else
            {
                Logger.LogInfo($"Successfully found local player: {Debugging.GetGameObjectPath(playerObject)}");
            }
            
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
                
                // If we still don't have players, use direct search as a last resort
                if (PlayerTracker.Instance.GetAllPlayers().Count == 0)
                {
                    Logger.LogInfo("No players found by name, using direct search method");
                    SearchForPlayersDirectly();
                }
                
                // Report results
                int totalPlayers = PlayerTracker.Instance.GetAllPlayers().Count;
                Logger.LogInfo($"Player tracking complete. Found {totalPlayers} players.");
                
                // If we have players but no local player, try to find the local player directly
                if (totalPlayers > 0 && PlayerTracker.Instance.LocalPlayer == null)
                {
                    GameObject localPlayer = FindLocalPlayer();
                    if (localPlayer != null)
                    {
                        Logger.LogInfo($"Found local player: {Debugging.GetGameObjectPath(localPlayer)}");
                    }
                }
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
                Logger.LogInfo($"Searching for player GameObject with ID {playerId} and name '{playerName}'");
                GameObject playerObject = null;
                
                // First try to find by PhotonView's OwnerActorNr
                MonoBehaviour[] allPhotonViews = GameObject.FindObjectsOfType<MonoBehaviour>();
                foreach (MonoBehaviour behavior in allPhotonViews)
                {
                    if (behavior == null || behavior.GetType().Name != "PhotonView") 
                        continue;
                    
                    // Try to get the owner ActorNumber
                    try
                    {
                        // Using reflection to get ownerActorNr field
                        FieldInfo ownerActorNrField = behavior.GetType().GetField("ownerActorNr", 
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        
                        if (ownerActorNrField != null)
                        {
                            int ownerActorNr = (int)ownerActorNrField.GetValue(behavior);
                            
                            if (ownerActorNr == playerId)
                            {
                                // This PhotonView is owned by the player we're looking for
                                GameObject viewObject = behavior.gameObject;
                                Logger.LogInfo($"Found PhotonView owned by player {playerId}: {Debugging.GetGameObjectPath(viewObject)}");
                                
                                // Check if this is actually a player object
                                if (viewObject.name.Contains("Player") || 
                                    viewObject.name.Contains("Avatar") || 
                                    ComponentUtility.HasComponentByName(viewObject, "Player"))
                                {
                                    // This is likely the player
                                    playerObject = viewObject;
                                    Logger.LogInfo($"Found player {playerId} directly: {Debugging.GetGameObjectPath(playerObject)}");
                                    break;
                                }
                                else
                                {
                                    // Check parent hierarchy for player objects
                                    Transform parent = viewObject.transform.parent;
                                    while (parent != null)
                                    {
                                        if (parent.name.Contains("Player") || 
                                            parent.name.Contains("Avatar") || 
                                            ComponentUtility.HasComponentByName(parent.gameObject, "Player"))
                                        {
                                            // Found a parent that looks like a player
                                            playerObject = parent.gameObject;
                                            Logger.LogInfo($"Found player {playerId} (parent): {Debugging.GetGameObjectPath(playerObject)}");
                                            break;
                                        }
                                        parent = parent.parent;
                                    }
                                    
                                    if (playerObject != null)
                                        break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning($"Error accessing PhotonView owner: {ex.Message}");
                    }
                }
                
                // If not found by ID, try by name
                if (playerObject == null && !string.IsNullOrEmpty(playerName))
                {
                    Logger.LogInfo($"Player not found by ID {playerId}, trying by name '{playerName}'");
                    
                    // Look for objects with matching name components
                    GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
                    foreach (GameObject obj in allObjects)
                    {
                        // Player naming in hierarchy often contains the player name
                        if (obj.name.Contains(playerName) && 
                            (obj.name.Contains("Player") || obj.name.Contains("Avatar")))
                        {
                            playerObject = obj;
                            Logger.LogInfo($"Found player by name matching: {Debugging.GetGameObjectPath(playerObject)}");
                            break;
                        }
                        
                        // Check if this is a player object with a nametag or other identifier
                        if (ComponentUtility.HasComponentByName(obj, "Player"))
                        {
                            // Check children for text elements that might have the player name
                            TextMesh[] textMeshes = obj.GetComponentsInChildren<TextMesh>();
                            foreach (TextMesh text in textMeshes)
                            {
                                if (text.text == playerName || text.text.Contains(playerName))
                                {
                                    playerObject = obj;
                                    Logger.LogInfo($"Found player by TextMesh name: {Debugging.GetGameObjectPath(playerObject)}");
                                    break;
                                }
                            }
                            
                            if (playerObject != null)
                                break;
                        }
                    }
                }
                
                if (playerObject == null)
                {
                    Logger.LogWarning($"Could not find GameObject for player {playerId} ({playerName})");
                }
                
                return playerObject;
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
                            if (obj.name.Contains("Cart") && ComponentUtility.HasComponentByName(obj, "PhysGrabCart"))
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
                
                // 1. Stop cart control
                StopCartControl();
                
                // 2. Restore player position and rotation
                GameObject playerObject = _lastPlayerSwap.PlayerObject;
                if (playerObject != null)
                {
                    // Restore transform
                    playerObject.transform.position = _lastPlayerSwap.OriginalPlayerPosition;
                    playerObject.transform.rotation = _lastPlayerSwap.OriginalPlayerRotation;
                    playerObject.transform.parent = _lastPlayerSwap.OriginalPlayerParent;
                    playerObject.layer = _lastPlayerSwap.OriginalPlayerLayer;
                    
                    // Re-enable any disabled components
                    foreach (MonoBehaviour mb in _lastPlayerSwap.DisabledMonoBehaviours)
                    {
                        if (mb != null)
                        {
                            mb.enabled = true;
                            Logger.LogInfo($"Re-enabled player component: {mb.GetType().Name}");
                        }
                    }
                    
                    // Restore rigidbody state if it had one
                    if (_lastPlayerSwap.PlayerHadRigidbody)
                    {
                        Rigidbody rb = playerObject.GetComponent<Rigidbody>();
                        if (rb != null)
                        {
                            rb.isKinematic = _lastPlayerSwap.PlayerRbWasKinematic;
                            Logger.LogInfo($"Restored player rigidbody isKinematic to {_lastPlayerSwap.PlayerRbWasKinematic}");
                        }
                    }
                    
                    // Restore player visibility
                    TogglePlayerVisibility(playerObject, true);
                    
                    // Restore original damage handler state
                    if (_lastPlayerSwap.PlayerDamageHandler != null)
                    {
                        _playerDamageHandler = _lastPlayerSwap.PlayerDamageHandler;
                        Logger.LogInfo($"Restored player damage handler: {_playerDamageHandler.GetType().Name}");
                    }
                }
                
                // 3. Restore camera
                if (_lastPlayerSwap.OriginalCamera != null)
                {
                    Camera mainCamera = _lastPlayerSwap.OriginalCamera;
                    mainCamera.transform.parent = _lastPlayerSwap.OriginalCameraParent;
                    mainCamera.transform.localPosition = _lastPlayerSwap.OriginalCameraLocalPosition;
                    mainCamera.transform.localRotation = _lastPlayerSwap.OriginalCameraLocalRotation;
                    Logger.LogInfo("Restored camera to original state");
                }
                
                // 4. Clean up temporary collider
                if (_temporaryFloorCollider != null)
                {
                    GameObject.Destroy(_temporaryFloorCollider);
                    _temporaryFloorCollider = null;
                    Logger.LogInfo("Removed temporary floor collider");
                }
                
                // 5. Clean up the swap data
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

        /// <summary>
        /// Search for players directly in the scene by common player-related components and names
        /// </summary>
        private void SearchForPlayersDirectly()
        {
            Logger.LogInfo("Searching for players directly in the scene...");
            
            try
            {
                int foundPlayers = 0;
                GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
                
                // Common player-related component names
                string[] playerComponentNames = new string[] 
                { 
                    "PlayerController", "PlayerMovement", "PlayerAvatar", "PlayerTumble", 
                    "PlayerInput", "CharacterController", "PlayerHealth", "PlayerCamera"
                };
                
                // First try to find by specific player components
                foreach (GameObject obj in allObjects)
                {
                    if (obj == null) continue;
                    
                    bool isPlayer = false;
                    
                    // Check if this has a player-related component
                    foreach (string componentName in playerComponentNames)
                    {
                        if (ComponentUtility.HasComponentByName(obj, componentName))
                        {
                            isPlayer = true;
                            break;
                        }
                    }
                    
                    // Check by naming convention
                    string path = Debugging.GetGameObjectPath(obj);
                    if (!isPlayer && (
                        (obj.name.Contains("Player") && !obj.name.Contains("Manager") && !obj.name.Contains("UI")) ||
                        path.Contains("/Player") || 
                        path.Contains("Avatar")))
                    {
                        isPlayer = true;
                    }
                    
                    if (isPlayer)
                    {
                        // We found what appears to be a player object
                        PlayerTracker.Instance.AddPlayer(obj);
                        Logger.LogInfo($"Found potential player: {Debugging.GetGameObjectPath(obj)}");
                        foundPlayers++;
                        
                        // Try to determine if this is the local player
                        // Usually a local player will be a child of the main camera or vice versa
                        bool isLocalPlayer = false;
                        
                        // Check if Camera.main is a child of this player
                        if (Camera.main != null)
                        {
                            Transform parent = Camera.main.transform.parent;
                            while (parent != null)
                            {
                                if (parent.gameObject == obj)
                                {
                                    isLocalPlayer = true;
                                    break;
                                }
                                parent = parent.parent;
                            }
                            
                            // Or check if this player is a child of Camera.main
                            if (!isLocalPlayer)
                            {
                                Transform current = obj.transform;
                                while (current.parent != null)
                                {
                                    if (current.parent == Camera.main.transform)
                                    {
                                        isLocalPlayer = true;
                                        break;
                                    }
                                    current = current.parent;
                                }
                            }
                        }
                        
                        // If we think this is the local player, mark it as such
                        if (isLocalPlayer)
                        {
                            PlayerTracker.Instance.AddPlayer(obj, true);
                            Logger.LogInfo($"Found local player: {Debugging.GetGameObjectPath(obj)}");
                            break; // We found the local player, that's enough
                        }
                    }
                }
                
                // If we still haven't identified a local player but have found players, use the first one
                if (PlayerTracker.Instance.LocalPlayer == null && PlayerTracker.Instance.GetAllPlayers().Count > 0)
                {
                    GameObject firstPlayer = PlayerTracker.Instance.GetAllPlayers()[0];
                    PlayerTracker.Instance.AddPlayer(firstPlayer, true);
                    Logger.LogInfo($"No specific local player identified, using first player as local: {Debugging.GetGameObjectPath(firstPlayer)}");
                }
                
                Logger.LogInfo($"Direct player search complete. Found {foundPlayers} potential players.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error during direct player search: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
} 