using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace BecomeCart
{
    public partial class Plugin
    {
        /// <summary>
        /// Attaches the player to a cart, implementing cart control functionality
        /// </summary>
        private void AttachPlayerToCart(GameObject playerObject, GameObject cartObject)
        {
            Logger.LogInfo($"Attaching player {Debugging.GetGameObjectPath(playerObject)} to cart {Debugging.GetGameObjectPath(cartObject)}");
            
            try
            {
                // Create a data structure to store the original state for later restoration
                _lastPlayerSwap = new PlayerCartSwap
                {
                    PlayerObject = playerObject,
                    CartObject = cartObject,
                    OriginalPlayerPosition = playerObject.transform.position,
                    OriginalPlayerRotation = playerObject.transform.rotation,
                    OriginalPlayerParent = playerObject.transform.parent,
                    OriginalPlayerLayer = playerObject.layer,
                    PlayerComponents = new List<Component>(),
                    DisabledMonoBehaviours = new List<MonoBehaviour>()
                };

                // 1. Find handle points or grab areas if they exist
                Transform handlePoint = null;
                Transform grabPoint = null;
                
                // Check for Cart Handle or similar objects
                foreach (Transform child in cartObject.GetComponentsInChildren<Transform>())
                {
                    if (child.name.Contains("Handle") || child.name.Contains("Grab Point"))
                    {
                        Logger.LogInfo($"Found potential handle/grab point: {Debugging.GetGameObjectPath(child.gameObject)}");
                        
                        if (child.name.Contains("Handle"))
                            handlePoint = child;
                        else if (child.name.Contains("Grab Point"))
                            grabPoint = child;
                    }
                }
                
                // 2. Handle camera separately to avoid movement issues
                Camera mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    Logger.LogInfo($"Detaching main camera from player for cart view");
                    
                    // Save original camera state
                    _lastPlayerSwap.OriginalCamera = mainCamera;
                    _lastPlayerSwap.OriginalCameraParent = mainCamera.transform.parent;
                    _lastPlayerSwap.OriginalCameraLocalPosition = mainCamera.transform.localPosition;
                    _lastPlayerSwap.OriginalCameraLocalRotation = mainCamera.transform.localRotation;
                    
                    // Then parent the camera to the cart for a third-person view
                    mainCamera.transform.parent = cartObject.transform;
                    mainCamera.transform.localPosition = new Vector3(0, 1.5f, -2f);
                    mainCamera.transform.localRotation = Quaternion.Euler(15f, 0, 0);
                    Logger.LogInfo("Positioned camera closer behind cart");
                }
                
                // 3. Capture important cart components
                Component physGrabObject = null;
                Component physGrabCart = null;
                
                foreach (Component comp in cartObject.GetComponents<Component>())
                {
                    if (comp == null) continue;
                    
                    string typeName = comp.GetType().Name;
                    if (typeName.Contains("PhysGrabObject"))
                        physGrabObject = comp;
                    else if (typeName.Contains("PhysGrabCart"))
                        physGrabCart = comp;
                }
                
                if (physGrabObject != null)
                    _lastPlayerSwap.CartPhysGrabObject = physGrabObject;
                
                if (physGrabCart != null)
                    _lastPlayerSwap.CartPhysGrabCart = physGrabCart;
                
                // 4. Handle player components, especially movement and control
                foreach (Component comp in playerObject.GetComponents<Component>())
                {
                    if (comp == null) continue;
                    
                    // Add to tracked components list
                    _lastPlayerSwap.PlayerComponents.Add(comp);
                    
                    // Disable MonoBehaviours that control player movement
                    string typeName = comp.GetType().Name;
                    if (comp is MonoBehaviour mb && 
                        (typeName.Contains("Controller") || 
                         typeName.Contains("Movement") || 
                         typeName.Contains("Motor") ||
                         typeName.Contains("Input")))
                    {
                        if (mb.enabled)
                        {
                            mb.enabled = false;
                            _lastPlayerSwap.DisabledMonoBehaviours.Add(mb);
                            Logger.LogInfo($"Disabled player component: {typeName}");
                        }
                    }
                    
                    // Check for rigidbody
                    if (comp is Rigidbody rb)
                    {
                        _lastPlayerSwap.PlayerHadRigidbody = true;
                        _lastPlayerSwap.PlayerRbWasKinematic = rb.isKinematic;
                        rb.isKinematic = true;
                        Logger.LogInfo("Set player rigidbody to kinematic");
                    }
                    
                    // Check for damage handlers
                    if (typeName.Contains("Health") || typeName.Contains("Damage"))
                    {
                        _lastPlayerSwap.PlayerDamageHandler = comp;
                        Logger.LogInfo($"Found player damage handler: {typeName}");
                    }
                }
                
                // 5. Make player invisible but keep camera functioning
                _playerVisibilityState.Clear();
                TogglePlayerVisibility(playerObject, false);
                
                // 6. Position player inside or near the cart
                if (grabPoint != null)
                {
                    playerObject.transform.position = grabPoint.position;
                    Logger.LogInfo("Positioned player at cart grab point");
                }
                else if (handlePoint != null)
                {
                    playerObject.transform.position = handlePoint.position;
                    Logger.LogInfo("Positioned player at cart handle");
                }
                else
                {
                    // Default positioning inside the cart
                    playerObject.transform.position = cartObject.transform.position + Vector3.up * 0.5f;
                    Logger.LogInfo("Positioned player inside cart");
                }
                
                // 7. Initialize cart control system
                InitializeCartControl(cartObject);
                
                // 8. Create a temporary floor collider if needed to prevent falling through
                CreateTemporaryFloorCollider(cartObject);
                
                // 9. Send network event to other players
                NetworkManager.Instance.SendPlayerCartSwap(playerObject, cartObject);
                
                // 10. Transfer ownership of the cart to the local player for physics sync
                NetworkManager.Instance.TransferCartOwnership(cartObject);
                
                Logger.LogInfo("Now driving the cart! Use mouse to turn, W/S for forward/back, A/D to strafe.");
                Logger.LogInfo($"Player is {(_playerDamageImmune ? "immune to damage" : "vulnerable to damage")}");
                Logger.LogInfo($"Player model is {(_debugShowPlayerModel ? "visible (debug mode)" : "hidden")}");
                Logger.LogInfo("Press F5 to toggle player model visibility");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error during player-cart attachment: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Internal implementation of visual attachment for remote players
        /// This is the implementation that NetworkManager should call
        /// </summary>
        public void AttachPlayerToCartVisuallyInternal(GameObject playerObject, GameObject cartObject)
        {
            if (playerObject == null || cartObject == null)
                return;
                
            Logger.LogInfo($"Visually attaching remote player {Debugging.GetGameObjectPath(playerObject)} to cart {Debugging.GetGameObjectPath(cartObject)}");
            
            try
            {
                // Store player-cart mapping for restoration later
                _remotePlayerCarts[playerObject] = new RemotePlayerCartInfo
                {
                    CartObject = cartObject,
                    OriginalPlayerVisibility = new Dictionary<string, bool>()
                };
                
                // Make the player invisible
                Renderer[] renderers = playerObject.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    if (renderer == null) continue;
                    
                    string path = Debugging.GetGameObjectPath(renderer.gameObject);
                    
                    // Store current state
                    _remotePlayerCarts[playerObject].OriginalPlayerVisibility[path] = renderer.enabled;
                    
                    // Hide the renderer
                    renderer.enabled = false;
                }
                
                // Position the player at the cart
                playerObject.transform.position = cartObject.transform.position + Vector3.up * 0.5f;
                
                // Note: We do not:
                // - Change camera position (as this is a remote player)
                // - Disable player control components (they're already controlled by the network)
                // - Create floor colliders or initialize cart control
                
                Logger.LogInfo($"Remote player {Debugging.GetGameObjectPath(playerObject)} is now visually a cart");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error during remote player-cart visual attachment: {ex.Message}\n{ex.StackTrace}");
            }
        }
        
        /// <summary>
        /// Internal implementation of visual restoration for remote players
        /// This is the implementation that NetworkManager should call
        /// </summary>
        public void RestorePlayerFromCartVisuallyInternal(GameObject playerObject)
        {
            if (playerObject == null || !_remotePlayerCarts.ContainsKey(playerObject))
                return;
                
            Logger.LogInfo($"Visually restoring remote player {Debugging.GetGameObjectPath(playerObject)} from cart");
            
            try
            {
                // Restore player visibility
                RemotePlayerCartInfo info = _remotePlayerCarts[playerObject];
                
                foreach (Renderer renderer in playerObject.GetComponentsInChildren<Renderer>())
                {
                    if (renderer == null) continue;
                    
                    string path = Debugging.GetGameObjectPath(renderer.gameObject);
                    
                    if (info.OriginalPlayerVisibility.ContainsKey(path))
                    {
                        renderer.enabled = info.OriginalPlayerVisibility[path];
                    }
                    else
                    {
                        // Default to visible if we don't have stored state
                        renderer.enabled = true;
                    }
                }
                
                // Remove from our tracking
                _remotePlayerCarts.Remove(playerObject);
                
                Logger.LogInfo($"Remote player {Debugging.GetGameObjectPath(playerObject)} visual state restored");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error restoring remote player visual state: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Public method to attach a player to a cart visually for network synchronization
        /// Used by NetworkManager when a remote player becomes a cart
        /// </summary>
        public void AttachPlayerToCartVisually(GameObject playerObject, GameObject cartObject)
        {
            // Call the internal implementation
            AttachPlayerToCartVisuallyInternal(playerObject, cartObject);
        }
        
        /// <summary>
        /// Public method to restore a player's visual state for network synchronization
        /// Used by NetworkManager when a remote player stops being a cart
        /// </summary>
        public void RestorePlayerFromCartVisually(GameObject playerObject)
        {
            // Call the internal implementation
            RestorePlayerFromCartVisuallyInternal(playerObject);
        }

        /// <summary>
        /// Toggles the visibility of the player model
        /// </summary>
        private void TogglePlayerVisibility(GameObject playerObject, bool visible)
        {
            if (playerObject == null) return;
            
            try
            {
                // Store visibility state of each renderer
                Renderer[] renderers = playerObject.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    if (renderer == null) continue;
                    
                    string path = Debugging.GetGameObjectPath(renderer.gameObject);
                    
                    // Store current state if we don't have it yet
                    if (!_playerVisibilityState.ContainsKey(path))
                        _playerVisibilityState[path] = renderer.enabled;
                    
                    // Set visibility based on parameter
                    renderer.enabled = visible;
                }
                
                Logger.LogInfo($"Set player visibility to {visible}");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error toggling player visibility: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a temporary floor collider under the cart to prevent falling through the ground
        /// </summary>
        private void CreateTemporaryFloorCollider(GameObject cartObject)
        {
            try
            {
                // Check if we already have a floor collider
                if (_temporaryFloorCollider != null)
                {
                    GameObject.Destroy(_temporaryFloorCollider);
                }
                
                // Create a new floor collider
                _temporaryFloorCollider = new GameObject("TemporaryFloorCollider");
                _temporaryFloorCollider.transform.position = cartObject.transform.position - Vector3.up * 0.5f;
                
                // Add box collider sized to fit under the cart
                BoxCollider boxCollider = _temporaryFloorCollider.AddComponent<BoxCollider>();
                boxCollider.size = new Vector3(3f, 0.1f, 3f);
                boxCollider.isTrigger = false;
                
                Logger.LogInfo("Created temporary floor collider under cart");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error creating floor collider: {ex.Message}");
            }
        }
    }
} 