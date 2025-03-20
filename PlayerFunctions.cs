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
    }
} 