using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace BecomeCart;

[BepInPlugin("com.becomecart.repo.plugin", "BecomeCart", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    internal static HarmonyLib.Harmony Harmony;
    internal static Plugin Instance;
        
    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Instance = this;
        
        // Initialize Harmony for patching
        Harmony = new HarmonyLib.Harmony("com.becomecart.repo.plugin");
        Harmony.PatchAll(Assembly.GetExecutingAssembly());
        
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        Logger.LogInfo("Press F5 to dump scene, F6 to dump assemblies, F7 to find player");
        Logger.LogInfo("Press F11 to find and track all connected players");
        Logger.LogInfo("Press F12 to dump player details to file (after using F11)");
        Logger.LogInfo("Press F3 to swap the player model with a cart model");
        Logger.LogInfo("Press F4 to restore the player model back from cart form");
    }
    
    // Helper method for debugging game objects
    public static void DumpGameObject(GameObject obj, int depth = 0)
    {
        if (obj == null) return;
        
        string indent = new string(' ', depth * 2);
        Logger.LogInfo($"{indent}GameObject: {obj.name}, Active: {obj.activeSelf}");
        
        // Log components
        Component[] components = obj.GetComponents<Component>();
        foreach (Component component in components)
        {
            if (component == null) continue;
            Logger.LogInfo($"{indent}  Component: {component.GetType().Name}");
        }
        
        // Recursively log children
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            Transform child = obj.transform.GetChild(i);
            DumpGameObject(child.gameObject, depth + 1);
        }
    }
    
    // Simple key press monitor for debugging - add your own keys
    private void Update()
    {
        // F5: Dump the active scene hierarchy
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Logger.LogInfo("=== DUMPING ACTIVE SCENE HIERARCHY ===");
            GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (GameObject obj in rootObjects)
            {
                DumpGameObject(obj);
            }
        }
        
        // F6: Dump all loaded assemblies
        if (Input.GetKeyDown(KeyCode.F6))
        {
            Logger.LogInfo("=== DUMPING LOADED ASSEMBLIES ===");
            Debugging.DumpAssemblies();
        }
        
        // F7: Try to find the player object and dump its components
        if (Input.GetKeyDown(KeyCode.F7))
        {
            Logger.LogInfo("=== LOOKING FOR PLAYER OBJECT ===");
            
            // Common ways to find the player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                // Try to find by name (common names to check)
                player = GameObject.Find("Player");
                if (player == null) player = GameObject.Find("Character");
                if (player == null) player = GameObject.Find("PlayerCharacter");
            }
            
            if (player != null)
            {
                Logger.LogInfo($"Found player object: {player.name}");
                DumpGameObject(player);
                
                // Additionally, dump detailed field info
                Component[] components = player.GetComponents<Component>();
                foreach (Component component in components)
                {
                    if (component != null)
                    {
                        Debugging.DumpFields(component);
                        Debugging.DumpMethods(component);
                    }
                }
            }
            else
            {
                Logger.LogInfo("Could not find player object by common tags/names");
            }
        }
        
        // F8: Look for UI elements 
        if (Input.GetKeyDown(KeyCode.F8))
        {
            Logger.LogInfo("=== LOOKING FOR UI ELEMENTS ===");
            // Find all Canvas components in the scene without directly referencing UI namespace
            var canvasComponents = GameObject.FindObjectsOfType<Component>()
                .Where(c => c.GetType().Name == "Canvas")
                .ToArray();
                
            Logger.LogInfo($"Found {canvasComponents.Length} Canvas components");
            
            // Find all Button components in the scene without directly referencing UI namespace
            var buttonComponents = GameObject.FindObjectsOfType<Component>()
                .Where(c => c.GetType().Name == "Button")
                .ToArray();
                
            Logger.LogInfo($"Found {buttonComponents.Length} Button components");
        }
        
        // F9: Find the Item Cart Medium and REGISTER it (no mods)
        if (Input.GetKeyDown(KeyCode.F9))
        {
            Logger.LogInfo("=== FINDING ITEM CART MEDIUM AND REGISTERING IT ===");
            
            // Clear previous registrations to avoid confusion
            CartPatch.CartControllers.Clear();
            
            // Look specifically for "Item Cart Medium" objects
            var cartObjects = GameObject.FindObjectsOfType<GameObject>()
                .Where(obj => obj.name.Contains("Item Cart Medium"))
                .ToArray();
                
            if (cartObjects.Length > 0)
            {
                GameObject cart = cartObjects[0];
                Logger.LogInfo($"Found cart: {Debugging.GetGameObjectPath(cart)}");
                
                // Find the PhysGrabCart component
                MonoBehaviour physGrabCart = null;
                foreach (var component in cart.GetComponents<MonoBehaviour>())
                {
                    if (component.GetType().Name == "PhysGrabCart")
                    {
                        physGrabCart = component;
                        break;
                    }
                }
                
                if (physGrabCart != null)
                {
                    // Dump its fields to the console
                    Debugging.DumpFields(physGrabCart);
                    
                    // Register it for modifications but don't apply any mods
                    Logger.LogInfo("Registering cart component for later modification...");
                    CartPatch.RegisterCartComponent(physGrabCart, cart);
                    
                    Logger.LogInfo($"Cart registered! Current cart count: {CartPatch.CartControllers.Count}");
                    Logger.LogInfo("Press F10 to view the cart's complete hierarchy.");
                    Logger.LogInfo("Press 1-4 to apply different physics modes, F12 for boost.");
                }
                else
                {
                    Logger.LogInfo("Could not find PhysGrabCart component on Item Cart Medium.");
                }
            }
            else
            {
                // Direct approach - just look for any PhysGrabCart components
                var physGrabCarts = GameObject.FindObjectsOfType<MonoBehaviour>()
                    .Where(mb => mb.GetType().Name == "PhysGrabCart")
                    .ToArray();
                
                if (physGrabCarts.Length > 0)
                {
                    var physGrabCart = physGrabCarts[0];
                    Logger.LogInfo($"Found PhysGrabCart on: {Debugging.GetGameObjectPath(physGrabCart.gameObject)}");
                    
                    // Dump its fields to the console
                    Debugging.DumpFields(physGrabCart);
                    
                    // Register it for modifications but don't apply any mods
                    Logger.LogInfo("Registering cart component for later modification...");
                    CartPatch.RegisterCartComponent(physGrabCart, physGrabCart.gameObject);
                    
                    Logger.LogInfo($"Cart registered! Current cart count: {CartPatch.CartControllers.Count}");
                    Logger.LogInfo("Press F10 to view the cart's complete hierarchy.");
                    Logger.LogInfo("Press 1-4 to apply different physics modes, F12 for boost.");
                }
                else
                {
                    Logger.LogInfo("No carts found in the scene at all. Try a different level!");
                }
            }
        }
        
        // F10: Dump complete hierarchy of all registered carts
        if (Input.GetKeyDown(KeyCode.F10))
        {
            Logger.LogInfo("=== DUMPING COMPLETE CART HIERARCHY ===");
            
            Logger.LogInfo($"Found Cart Components: {CartPatch.FoundCartComponents.Count}");
            Logger.LogInfo($"Cart Controllers: {CartPatch.CartControllers.Count}");
            
            bool dumped = false;
            
            // First check cart controllers
            if (CartPatch.CartControllers.Count > 0)
            {
                Logger.LogInfo("--- Dumping from CartControllers list ---");
                foreach (var cartComponent in CartPatch.CartControllers)
                {
                    if (cartComponent == null || cartComponent.gameObject == null)
                    {
                        Logger.LogInfo("Found a null cart component or gameObject!");
                        continue;
                    }
                    
                    GameObject cartObj = cartComponent.gameObject;
                    Logger.LogInfo($"Cart: {cartComponent.GetType().Name} on {cartObj.name}");
                    
                    // Dump parent hierarchy
                    Transform parent = cartObj.transform.parent;
                    if (parent != null)
                    {
                        Logger.LogInfo("=== PARENT HIERARCHY ===");
                        while (parent != null)
                        {
                            Logger.LogInfo($"Parent: {parent.name}");
                            // Dump components on parent
                            foreach (var comp in parent.GetComponents<Component>())
                            {
                                if (comp != null)
                                {
                                    Logger.LogInfo($"  Component: {comp.GetType().Name}");
                                }
                            }
                            parent = parent.parent;
                        }
                    }
                    else
                    {
                        Logger.LogInfo("This cart has no parent objects.");
                    }
                    
                    // Dump children recursively with more detail
                    Logger.LogInfo("=== CHILD HIERARCHY ===");
                    DumpGameObjectAndComponents(cartObj, 0);
                    
                    dumped = true;
                }
            }
            
            // If we didn't find any in CartControllers, check FoundCartComponents as fallback
            if (!dumped && CartPatch.FoundCartComponents.Count > 0)
            {
                Logger.LogInfo("--- Dumping from FoundCartComponents list ---");
                foreach (var cartComponent in CartPatch.FoundCartComponents)
                {
                    if (cartComponent == null || cartComponent.gameObject == null)
                    {
                        Logger.LogInfo("Found a null cart component or gameObject!");
                        continue;
                    }
                    
                    GameObject cartObj = cartComponent.gameObject;
                    Logger.LogInfo($"Cart: {cartComponent.GetType().Name} on {cartObj.name}");
                    
                    // Dump children recursively with more detail
                    Logger.LogInfo("=== OBJECT HIERARCHY ===");
                    DumpGameObjectAndComponents(cartObj, 0);
                    
                    dumped = true;
                }
            }
            
            // Final fallback - try to find by name if registration failed
            if (!dumped)
            {
                Logger.LogInfo("No registered carts found. Trying direct search for Item Cart Medium...");
                var cartObjects = GameObject.FindObjectsOfType<GameObject>()
                    .Where(obj => obj.name.Contains("Item Cart Medium"))
                    .ToArray();
                    
                if (cartObjects.Length > 0)
                {
                    Logger.LogInfo($"Found {cartObjects.Length} cart objects by name.");
                    foreach (var cart in cartObjects)
                    {
                        Logger.LogInfo($"Cart: {Debugging.GetGameObjectPath(cart)}");
                        DumpGameObjectAndComponents(cart, 0);
                        dumped = true;
                    }
                }
                else
                {
                    Logger.LogInfo("No carts found by name either. Try F9 first to register a cart.");
                }
            }
            
            if (!dumped)
            {
                Logger.LogInfo("No carts registered or found. Use F9 first to find and register a cart.");
            }
        }
        
        // F11: Find and track all players
        if (Input.GetKeyDown(KeyCode.F11))
        {
            FindAndTrackAllPlayers();
        }
        
        // F12: Dump player details to file
        if (Input.GetKeyDown(KeyCode.F12))
        {
            Logger.LogInfo("=== DUMPING PLAYER DETAILS TO FILE ===");
            PlayerTracker.Instance.DumpPlayersToFile();
        }
        
        // F3: Swap player model with cart
        if (Input.GetKeyDown(KeyCode.F3))
        {
            SwapPlayerModelWithCart();
        }
        
        // F4: Restore player from cart form
        if (Input.GetKeyDown(KeyCode.F4))
        {
            RestorePlayerFromCart();
        }
        
        // 1: Toggle Zero Gravity mode for the cart
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            if (CartPatch.CartControllers.Count > 0)
            {
                CartPatch.ToggleZeroGravity();
                Logger.LogInfo("Zero Gravity mode toggled");
            }
            else
            {
                Logger.LogInfo("No carts registered yet. Find a cart first with F9 or F11.");
            }
        }
        
        // 2: Toggle Floaty Cart mode
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            if (CartPatch.CartControllers.Count > 0)
            {
                CartPatch.ToggleFloatyCart();
                Logger.LogInfo("Floaty Cart mode toggled");
            }
            else
            {
                Logger.LogInfo("No carts registered yet. Find a cart first with F9 or F11.");
            }
        }
        
        // 3: Toggle Super Grip mode
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            if (CartPatch.CartControllers.Count > 0)
            {
                CartPatch.ToggleSuperGrip();
                Logger.LogInfo("Super Grip mode toggled");
            }
            else
            {
                Logger.LogInfo("No carts registered yet. Find a cart first with F9 or F11.");
            }
        }
        
        // 4: Toggle Slippery Cart mode
        if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
        {
            if (CartPatch.CartControllers.Count > 0)
            {
                CartPatch.ToggleSlipperyCart();
                Logger.LogInfo("Slippery Cart mode toggled");
            }
            else
            {
                Logger.LogInfo("No carts registered yet. Find a cart first with F9 or F11.");
            }
        }
    }
    
    private System.Collections.IEnumerator ResetBoostMultiplier(float originalValue, float delay)
    {
        yield return new WaitForSeconds(delay);
        CartPatch.BoostMultiplier = originalValue;
        Logger.LogInfo($"Boost multiplier reset to {originalValue}");
    }
    
    // Helper method to dump a game object and all its components with more detail
    private void DumpGameObjectAndComponents(GameObject obj, int depth)
    {
        if (obj == null) return;
        
        string indent = new string(' ', depth * 2);
        Logger.LogInfo($"{indent}GameObject: {obj.name}, Active: {obj.activeSelf}, Layer: {LayerMask.LayerToName(obj.layer)}");
        
        // Log more detailed component info
        Component[] components = obj.GetComponents<Component>();
        foreach (Component component in components)
        {
            if (component == null) continue;
            
            string typeName = component.GetType().Name;
            Logger.LogInfo($"{indent}  Component: {typeName}");
            
            // Dump important component details based on type
            if (component is Collider collider)
            {
                Logger.LogInfo($"{indent}    Collider: isTrigger={collider.isTrigger}, enabled={collider.enabled}");
                if (collider is BoxCollider box)
                {
                    Logger.LogInfo($"{indent}    BoxCollider size: {box.size}, center: {box.center}");
                }
                else if (collider is SphereCollider sphere)
                {
                    Logger.LogInfo($"{indent}    SphereCollider radius: {sphere.radius}, center: {sphere.center}");
                }
                else if (collider is CapsuleCollider capsule)
                {
                    Logger.LogInfo($"{indent}    CapsuleCollider radius: {capsule.radius}, height: {capsule.height}");
                }
            }
            else if (component is Rigidbody rb)
            {
                Logger.LogInfo($"{indent}    Rigidbody: mass={rb.mass}, drag={rb.drag}, useGravity={rb.useGravity}");
                Logger.LogInfo($"{indent}    Velocity: {rb.velocity}, AngularVelocity: {rb.angularVelocity}");
            }
            else if (component is MeshRenderer renderer)
            {
                Material[] materials = renderer.materials;
                Logger.LogInfo($"{indent}    MeshRenderer: isVisible={renderer.isVisible}, materialCount={materials.Length}");
                foreach (Material mat in materials)
                {
                    if (mat != null)
                    {
                        Logger.LogInfo($"{indent}      Material: {mat.name}, shader: {mat.shader.name}");
                    }
                }
            }
            else if (typeName.Contains("PhysGrab") || typeName.Contains("Cart") || typeName.Contains("Vehicle"))
            {
                // Dump fields for interesting components
                Debugging.DumpFields(component);
            }
        }
        
        // Recursively log children with more depth
        if (obj.transform.childCount > 0)
        {
            Logger.LogInfo($"{indent}  Children ({obj.transform.childCount}):");
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                Transform child = obj.transform.GetChild(i);
                DumpGameObjectAndComponents(child.gameObject, depth + 1);
            }
        }
    }

    /// <summary>
    /// Uses PhotonNetwork to find and track all connected players
    /// </summary>
    private void FindAndTrackAllPlayers()
    {
        Logger.LogInfo("=== FINDING AND TRACKING ALL PLAYERS ===");
        
        // First look for PhotonNetwork class
        Type photonNetworkType = null;
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.Name == "PhotonNetwork")
                {
                    photonNetworkType = type;
                    break;
                }
            }
            if (photonNetworkType != null) break;
        }
        
        if (photonNetworkType == null)
        {
            Logger.LogError("Could not find PhotonNetwork class!");
            return;
        }
        
        Logger.LogInfo($"Found PhotonNetwork class: {photonNetworkType.FullName}");
        
        // Get all player instances
        var playerListInfoProp = photonNetworkType.GetProperty("PlayerList");
        if (playerListInfoProp != null)
        {
            Logger.LogInfo("Found PhotonNetwork.PlayerList property");
            var playerList = playerListInfoProp.GetValue(null);
            
            if (playerList != null)
            {
                Logger.LogInfo("Successfully retrieved player list");
                
                // Convert to array or list for enumeration
                Array playerArray;
                if (playerList is Array arr)
                {
                    playerArray = arr;
                }
                else
                {
                    // Try to get the array via ToArray() method
                    var toArrayMethod = playerList.GetType().GetMethod("ToArray");
                    if (toArrayMethod != null)
                    {
                        playerArray = (Array)toArrayMethod.Invoke(playerList, null);
                    }
                    else
                    {
                        Logger.LogError("Cannot convert player list to array!");
                        return;
                    }
                }
                
                Logger.LogInfo($"Found {playerArray.Length} players in PhotonNetwork.PlayerList");
                
                foreach (var playerInfo in playerArray)
                {
                    // Try to extract player data
                    Type playerInfoType = playerInfo.GetType();
                    
                    // Get player ID and name
                    int playerId = -1;
                    string playerName = "Unknown";
                    bool isLocal = false;
                    
                    var actorNumberProp = playerInfoType.GetProperty("ActorNumber");
                    if (actorNumberProp != null)
                    {
                        playerId = (int)actorNumberProp.GetValue(playerInfo);
                    }
                    
                    var nicknameProp = playerInfoType.GetProperty("NickName");
                    if (nicknameProp != null)
                    {
                        playerName = (string)nicknameProp.GetValue(playerInfo);
                    }
                    
                    var isLocalPlayerProp = playerInfoType.GetProperty("IsLocal");
                    if (isLocalPlayerProp != null)
                    {
                        isLocal = (bool)isLocalPlayerProp.GetValue(playerInfo);
                    }
                    
                    Logger.LogInfo($"Found player: {playerName} (ID: {playerId}) IsLocal: {isLocal}");
                    
                    // Now try to find the player's GameObject in the scene
                    GameObject playerObject = null;
                    
                    // Try to find player avatar by name
                    var allPlayerAvatars = GameObject.FindObjectsOfType<GameObject>()
                        .Where(go => go.name.Contains("Player") || go.name.Contains("Avatar") || go.name.Contains("Character"))
                        .ToArray();
                    
                    Logger.LogInfo($"Found {allPlayerAvatars.Length} potential player avatar objects");
                    
                    foreach (var avatar in allPlayerAvatars)
                    {
                        // Look for PhotonView component on potential avatars
                        var photonViewComp = avatar.GetComponents<Component>()
                            .FirstOrDefault(c => c.GetType().Name == "PhotonView");
                        
                        if (photonViewComp != null)
                        {
                            // Try to get OwnerActorNr or Controller property of PhotonView
                            var ownerProperty = photonViewComp.GetType().GetProperty("OwnerActorNr") ?? 
                                                photonViewComp.GetType().GetProperty("Controller");
                            
                            if (ownerProperty != null)
                            {
                                var owner = ownerProperty.GetValue(photonViewComp);
                                int ownerId = -1;
                                
                                // Owner might be the int directly or it might be another object with ActorNumber
                                if (owner is int id)
                                {
                                    ownerId = id;
                                }
                                else if (owner != null)
                                {
                                    var actorNumProp = owner.GetType().GetProperty("ActorNumber");
                                    if (actorNumProp != null)
                                    {
                                        ownerId = (int)actorNumProp.GetValue(owner);
                                    }
                                }
                                
                                if (ownerId == playerId)
                                {
                                    playerObject = avatar;
                                    Logger.LogInfo($"Found matching avatar GameObject for {playerName}: {Debugging.GetGameObjectPath(avatar)}");
                                    break;
                                }
                            }
                        }
                    }
                    
                    if (playerObject == null)
                    {
                        Logger.LogInfo($"Could not find avatar GameObject for player {playerName}");
                        
                        // For the local player, we can try a different approach
                        if (isLocal)
                        {
                            // Try to find local player by looking for common components
                            var localPlayerCandidates = GameObject.FindObjectsOfType<GameObject>()
                                .Where(go => go.GetComponent<Camera>() != null || 
                                       go.GetComponentInChildren<Camera>() != null)
                                .ToArray();
                            
                            if (localPlayerCandidates.Length > 0)
                            {
                                playerObject = localPlayerCandidates[0];
                                Logger.LogInfo($"Found potential local player object: {Debugging.GetGameObjectPath(playerObject)}");
                            }
                        }
                    }
                    
                    // Track the player using our PlayerTracker
                    if (playerObject != null)
                    {
                        PlayerTracker.Instance.TrackPlayer(playerObject, playerId, playerName, isLocal);
                    }
                    else
                    {
                        // Track player info even without GameObject
                        Logger.LogInfo($"Tracking player {playerName} without GameObject reference");
                        // Create a dummy GameObject just for tracking purposes
                        GameObject dummyObject = new GameObject($"DummyPlayer_{playerId}");
                        PlayerTracker.Instance.TrackPlayer(dummyObject, playerId, playerName, isLocal);
                        GameObject.Destroy(dummyObject); // Immediately destroy it after tracking
                    }
                }
                
                // Dump all player details
                PlayerTracker.Instance.DumpAllPlayerDetails();
            }
            else
            {
                Logger.LogError("PhotonNetwork.PlayerList returned null!");
            }
        }
        else
        {
            Logger.LogError("Could not find PhotonNetwork.PlayerList property!");
        }
    }

    /// <summary>
    /// Class to store information about a player-cart attachment
    /// </summary>
    private class PlayerCartSwap
    {
        public GameObject PlayerObject;
        public GameObject CartObject;
        public Vector3 OriginalPlayerPosition;
        public Quaternion OriginalPlayerRotation;
        public Transform OriginalPlayerParent;
        public Component PlayerController;
        public Camera OriginalCamera;
        public Transform OriginalCameraParent;
        public Vector3 OriginalCameraLocalPosition;
        public Quaternion OriginalCameraLocalRotation;
        public bool CartWasKinematic;
        public Component CartPhysGrabObject;
        public Component CartPhysGrabCart;
        public List<Component> PlayerComponents;
    }

    private PlayerCartSwap _lastPlayerSwap;

    /// <summary>
    /// Restores the player from being attached to a cart
    /// </summary>
    private void RestorePlayerFromCart()
    {
        if (_lastPlayerSwap == null)
        {
            Logger.LogInfo("No player-cart attachment to restore.");
            return;
        }
        
        try
        {
            Logger.LogInfo("=== RESTORING PLAYER FROM CART ===");
            
            // 1. First, re-enable player renderers
            int rendererCount = 0;
            foreach (Renderer renderer in _lastPlayerSwap.PlayerObject.GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = true;
                rendererCount++;
            }
            Logger.LogInfo($"Re-enabled {rendererCount} renderers on player");
            
            // 2. Restore the player's parent, position and rotation
            if (_lastPlayerSwap.OriginalPlayerParent != null)
            {
                _lastPlayerSwap.PlayerObject.transform.SetParent(_lastPlayerSwap.OriginalPlayerParent);
                Logger.LogInfo($"Restored player parent: {_lastPlayerSwap.OriginalPlayerParent.name}");
            }
            else
            {
                _lastPlayerSwap.PlayerObject.transform.SetParent(null);
                Logger.LogInfo("Unparented player (original parent was null)");
            }
            
            // Store new position/rotation values for debugging
            var newPos = _lastPlayerSwap.PlayerObject.transform.position;
            var newRot = _lastPlayerSwap.PlayerObject.transform.rotation;
            
            // Set the player's position and rotation to the original values
            _lastPlayerSwap.PlayerObject.transform.position = _lastPlayerSwap.OriginalPlayerPosition;
            _lastPlayerSwap.PlayerObject.transform.rotation = _lastPlayerSwap.OriginalPlayerRotation;
            Logger.LogInfo($"Restored player position: {_lastPlayerSwap.OriginalPlayerPosition} (was {newPos})");
            
            // 3. Restore camera
            if (_lastPlayerSwap.OriginalCamera != null && _lastPlayerSwap.OriginalCameraParent != null)
            {
                _lastPlayerSwap.OriginalCamera.transform.SetParent(_lastPlayerSwap.OriginalCameraParent);
                _lastPlayerSwap.OriginalCamera.transform.localPosition = _lastPlayerSwap.OriginalCameraLocalPosition;
                _lastPlayerSwap.OriginalCamera.transform.localRotation = _lastPlayerSwap.OriginalCameraLocalRotation;
                Logger.LogInfo("Restored camera to original position");
            }
            
            // 4. Re-enable all player components
            if (_lastPlayerSwap.PlayerComponents != null && _lastPlayerSwap.PlayerComponents.Count > 0)
            {
                int componentsEnabled = 0;
                foreach (var component in _lastPlayerSwap.PlayerComponents)
                {
                    if (component == null) continue;
                    
                    // Try to re-enable the component
                    var enabledProperty = component.GetType().GetProperty("enabled");
                    if (enabledProperty != null)
                    {
                        enabledProperty.SetValue(component, true);
                        componentsEnabled++;
                    }
                    else
                    {
                        // Try with a field
                        var enabledField = component.GetType().GetField("enabled");
                        if (enabledField != null)
                        {
                            enabledField.SetValue(component, true);
                            componentsEnabled++;
                        }
                    }
                }
                Logger.LogInfo($"Re-enabled {componentsEnabled} player components");
            }
            else if (_lastPlayerSwap.PlayerController != null)
            {
                // Fall back to just the controller if we don't have the full list
                var enabledProperty = _lastPlayerSwap.PlayerController.GetType().GetProperty("enabled");
                if (enabledProperty != null)
                {
                    enabledProperty.SetValue(_lastPlayerSwap.PlayerController, true);
                    Logger.LogInfo($"Re-enabled player controller: {_lastPlayerSwap.PlayerController.GetType().Name}");
                }
                else
                {
                    // Try with a field
                    var enabledField = _lastPlayerSwap.PlayerController.GetType().GetField("enabled");
                    if (enabledField != null)
                    {
                        enabledField.SetValue(_lastPlayerSwap.PlayerController, true);
                        Logger.LogInfo($"Re-enabled player controller via field: {_lastPlayerSwap.PlayerController.GetType().Name}");
                    }
                }
            }
            
            // 5. Restore cart's state
            if (_lastPlayerSwap.CartObject != null)
            {
                // Reset Rigidbody state
                Rigidbody cartRb = _lastPlayerSwap.CartObject.GetComponent<Rigidbody>();
                if (cartRb != null)
                {
                    cartRb.isKinematic = _lastPlayerSwap.CartWasKinematic;
                    cartRb.velocity = Vector3.zero;
                    cartRb.angularVelocity = Vector3.zero;
                    Logger.LogInfo($"Restored cart kinematic state to: {_lastPlayerSwap.CartWasKinematic} and zeroed velocity");
                }
                
                // Reset PhysGrabObject state
                if (_lastPlayerSwap.CartPhysGrabObject != null)
                {
                    // Reset grabbed state
                    var grabbedField = _lastPlayerSwap.CartPhysGrabObject.GetType().GetField("grabbed");
                    var grabbedLocalField = _lastPlayerSwap.CartPhysGrabObject.GetType().GetField("grabbedLocal");
                    var heldByLocalPlayerField = _lastPlayerSwap.CartPhysGrabObject.GetType().GetField("heldByLocalPlayer");
                    
                    if (grabbedField != null) grabbedField.SetValue(_lastPlayerSwap.CartPhysGrabObject, false);
                    if (grabbedLocalField != null) grabbedLocalField.SetValue(_lastPlayerSwap.CartPhysGrabObject, false);
                    if (heldByLocalPlayerField != null) heldByLocalPlayerField.SetValue(_lastPlayerSwap.CartPhysGrabObject, false);
                    
                    // Try to call Release method
                    var releaseMethod = _lastPlayerSwap.CartPhysGrabObject.GetType().GetMethod("Release");
                    if (releaseMethod != null)
                    {
                        try {
                            releaseMethod.Invoke(_lastPlayerSwap.CartPhysGrabObject, null);
                            Logger.LogInfo("Called Release method on PhysGrabObject");
                        } catch (Exception ex) {
                            Logger.LogInfo($"Error calling Release method: {ex.Message}");
                        }
                    }
                    
                    // Reset lastPlayerGrabbing
                    var lastPlayerGrabbingField = _lastPlayerSwap.CartPhysGrabObject.GetType().GetField("lastPlayerGrabbing");
                    if (lastPlayerGrabbingField != null)
                    {
                        lastPlayerGrabbingField.SetValue(_lastPlayerSwap.CartPhysGrabObject, null);
                    }
                    
                    Logger.LogInfo("Reset PhysGrabObject state");
                }
                
                // Reset PhysGrabCart state
                if (_lastPlayerSwap.CartPhysGrabCart != null)
                {
                    // Reset cart pulling state
                    var cartBeingPulledField = _lastPlayerSwap.CartPhysGrabCart.GetType().GetField("cartBeingPulled");
                    if (cartBeingPulledField != null) cartBeingPulledField.SetValue(_lastPlayerSwap.CartPhysGrabCart, false);
                    
                    // Reset active state
                    var cartActiveField = _lastPlayerSwap.CartPhysGrabCart.GetType().GetField("cartActive");
                    if (cartActiveField != null) cartActiveField.SetValue(_lastPlayerSwap.CartPhysGrabCart, false);
                    
                    // Try to return to original state
                    var currentStateField = _lastPlayerSwap.CartPhysGrabCart.GetType().GetField("currentState");
                    var previousStateField = _lastPlayerSwap.CartPhysGrabCart.GetType().GetField("previousState");
                    if (currentStateField != null && previousStateField != null)
                    {
                        var previousState = previousStateField.GetValue(_lastPlayerSwap.CartPhysGrabCart);
                        if (previousState != null)
                        {
                            currentStateField.SetValue(_lastPlayerSwap.CartPhysGrabCart, previousState);
                            Logger.LogInfo($"Restored cart to previous state");
                        }
                        else
                        {
                            // Try to find a "Locked" state if previous state is null
                            if (currentStateField.FieldType.IsEnum)
                            {
                                Array enumValues = Enum.GetValues(currentStateField.FieldType);
                                foreach (var value in enumValues)
                                {
                                    string enumName = value.ToString();
                                    if (enumName.Contains("Locked") || enumName.Contains("Idle") || enumName.Contains("Default"))
                                    {
                                        currentStateField.SetValue(_lastPlayerSwap.CartPhysGrabCart, value);
                                        Logger.LogInfo($"Set cart state to: {enumName}");
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    
                    Logger.LogInfo("Reset PhysGrabCart state");
                }
            }
            
            Logger.LogInfo("Player restored from cart successfully.");
            _lastPlayerSwap = null;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error during player restoration: {ex.Message}\n{ex.StackTrace}");
            
            // Even if there was an error, attempt basic cleanup
            if (_lastPlayerSwap?.PlayerObject != null)
            {
                // Re-enable renderers
                foreach (Renderer renderer in _lastPlayerSwap.PlayerObject.GetComponentsInChildren<Renderer>())
                {
                    renderer.enabled = true;
                }
                
                // Restore original position
                _lastPlayerSwap.PlayerObject.transform.position = _lastPlayerSwap.OriginalPlayerPosition;
                _lastPlayerSwap.PlayerObject.transform.rotation = _lastPlayerSwap.OriginalPlayerRotation;
                
                // Restore camera
                if (_lastPlayerSwap.OriginalCamera != null && _lastPlayerSwap.OriginalCameraParent != null)
                {
                    _lastPlayerSwap.OriginalCamera.transform.SetParent(_lastPlayerSwap.OriginalCameraParent);
                }
                
                // Re-enable player controller as minimum fallback
                if (_lastPlayerSwap.PlayerController != null)
                {
                    var enabledProperty = _lastPlayerSwap.PlayerController.GetType().GetProperty("enabled");
                    if (enabledProperty != null)
                    {
                        enabledProperty.SetValue(_lastPlayerSwap.PlayerController, true);
                    }
                }
            }
            
            _lastPlayerSwap = null;
        }
    }

    /// <summary>
    /// Swaps the local player's model with a cart model
    /// </summary>
    private void SwapPlayerModelWithCart()
    {
        Logger.LogInfo("=== ATTEMPTING TO ATTACH PLAYER TO CART ===");
        
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
            // Try to find any "Item Cart Medium(Clone)" in the scene
            var cartsByName = GameObject.FindObjectsOfType<GameObject>()
                .Where(obj => obj.name == "Item Cart Medium(Clone)")
                .ToArray();
                
            if (cartsByName.Length > 0)
            {
                cartObject = cartsByName[0];
                Logger.LogInfo($"Found cart by exact name: {Debugging.GetGameObjectPath(cartObject)}");
            }
            else
            {
                // Check if we have any registered carts as fallback
                if (CartPatch.CartControllers.Count > 0)
                {
                    Component cartComponent = CartPatch.CartControllers[0];
                    if (cartComponent != null && cartComponent.gameObject != null)
                    {
                        cartObject = cartComponent.gameObject;
                        Logger.LogInfo($"Using registered cart: {Debugging.GetGameObjectPath(cartObject)}");
                    }
                }
                else
                {
                    // Last resort - try to find any cart objects
                    var anyCarts = GameObject.FindObjectsOfType<GameObject>()
                        .Where(obj => obj.name.Contains("Cart") || obj.name.Contains("cart"))
                        .ToArray();
                        
                    if (anyCarts.Length > 0)
                    {
                        cartObject = anyCarts[0];
                        Logger.LogInfo($"Found cart by generic name: {Debugging.GetGameObjectPath(cartObject)}");
                    }
                }
            }
        }
        
        if (cartObject == null)
        {
            Logger.LogError("Failed to find a cart to attach to. Make sure you're in a level with carts!");
            return;
        }
        
        // Log detailed info about the cart we found
        Logger.LogInfo($"Selected cart: {Debugging.GetGameObjectPath(cartObject)}");
        Logger.LogInfo($"Cart position: {cartObject.transform.position}");
        Logger.LogInfo($"Cart active: {cartObject.activeSelf}");
        
        // Now find the local player
        GameObject playerObject = FindLocalPlayer();
        
        if (playerObject == null)
        {
            Logger.LogError("Failed to find the player object. Try using F11 first to track players.");
            return;
        }
        
        // Now attach the player to the cart
        AttachPlayerToCart(playerObject, cartObject);
    }
    
    /// <summary>
    /// Attaches the player to an existing cart in the level
    /// </summary>
    private void AttachPlayerToCart(GameObject playerObject, GameObject cartObject)
    {
        Logger.LogInfo($"Attaching player {playerObject.name} to cart {cartObject.name}");
        
        try
        {
            // Debug information about what we're working with
            Logger.LogInfo("=== DETAILED ATTACHMENT INFO ===");
            Logger.LogInfo($"Player position before: {playerObject.transform.position}");
            Logger.LogInfo($"Cart position: {cartObject.transform.position}");
            
            // 1. Find the cart's handle point if it exists
            Transform cartHandlePoint = null;
            Transform cartGrabPoint = null;
            
            // Search for Cart Handle or Grab Point in the cart's children
            foreach (Transform child in cartObject.GetComponentsInChildren<Transform>())
            {
                if (child.name.Contains("Cart Handle"))
                {
                    cartHandlePoint = child;
                    Logger.LogInfo($"Found cart handle: {Debugging.GetGameObjectPath(child.gameObject)}");
                }
                else if (child.name.Contains("Cart Grab Point"))
                {
                    cartGrabPoint = child;
                    Logger.LogInfo($"Found cart grab point: {Debugging.GetGameObjectPath(child.gameObject)}");
                }
            }
            
            // Use the handle point, or grab point, or the cart itself
            Transform attachPoint = cartHandlePoint != null ? cartHandlePoint : 
                                 (cartGrabPoint != null ? cartGrabPoint : cartObject.transform);
            
            // 2. Find the main camera (which is likely the player's first-person camera)
            Camera mainCamera = Camera.main;
            Transform originalCameraParent = null;
            Vector3 originalCameraLocalPos = Vector3.zero;
            Quaternion originalCameraLocalRot = Quaternion.identity;
            
            if (mainCamera != null)
            {
                Logger.LogInfo($"Found main camera: {mainCamera.name}");
                originalCameraParent = mainCamera.transform.parent;
                originalCameraLocalPos = mainCamera.transform.localPosition;
                originalCameraLocalRot = mainCamera.transform.localRotation;
                
                // Set up third-person camera
                mainCamera.transform.parent = cartObject.transform;
                mainCamera.transform.localPosition = new Vector3(0, 3f, -4f);
                mainCamera.transform.localRotation = Quaternion.Euler(20f, 0, 0);
                Logger.LogInfo("Positioned camera behind cart");
            }
            
            // 3. Store original player position/rotation
            Vector3 originalPlayerPos = playerObject.transform.position;
            Quaternion originalPlayerRot = playerObject.transform.rotation;
            Transform originalPlayerParent = playerObject.transform.parent;
            
            // 4. Find the player's controller component and input components
            Component characterController = null;
            var playerComponents = new List<Component>();
            
            foreach (var component in playerObject.GetComponents<Component>())
            {
                var typeName = component.GetType().Name;
                
                // Store all potential control/input components
                if (typeName.Contains("Controller") || 
                    typeName.Contains("Movement") ||
                    typeName.Contains("Motor") ||
                    typeName.Contains("Input") ||
                    typeName.Contains("Player"))
                {
                    playerComponents.Add(component);
                    Logger.LogInfo($"Found potential player component: {typeName}");
                    
                    // Specifically mark the main character controller
                    if (typeName.Contains("Controller"))
                    {
                        characterController = component;
                    }
                }
            }
            
            // 5. Disable player's controller and input components
            foreach (var component in playerComponents)
            {
                // Try to disable the component using reflection
                var enabledProperty = component.GetType().GetProperty("enabled");
                if (enabledProperty != null)
                {
                    enabledProperty.SetValue(component, false);
                    Logger.LogInfo($"Disabled player component: {component.GetType().Name}");
                }
                else
                {
                    // Try with a field
                    var enabledField = component.GetType().GetField("enabled");
                    if (enabledField != null)
                    {
                        enabledField.SetValue(component, false);
                        Logger.LogInfo($"Disabled player component via field: {component.GetType().Name}");
                    }
                }
            }
            
            // 6. Make the player invisible (disable renderers)
            int rendererCount = 0;
            foreach (Renderer renderer in playerObject.GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = false;
                rendererCount++;
            }
            Logger.LogInfo($"Disabled {rendererCount} renderers on player");
            
            // 7. Parent the player to the cart's attachment point
            playerObject.transform.parent = attachPoint;
            playerObject.transform.localPosition = new Vector3(0, 0, 0); // Position at the handle
            playerObject.transform.localRotation = Quaternion.identity;
            Logger.LogInfo($"Attached player to cart at position: {playerObject.transform.position}");
            
            // 8. Make sure the cart's Rigidbody is configured properly
            Rigidbody cartRb = cartObject.GetComponent<Rigidbody>();
            bool wasKinematic = false;
            if (cartRb != null)
            {
                wasKinematic = cartRb.isKinematic;
                cartRb.isKinematic = false; // Make sure the cart can move
                cartRb.constraints = RigidbodyConstraints.None; // Remove constraints
                cartRb.mass = 1.0f; // Lighter mass for easier movement
                cartRb.drag = 1.0f; // Lower drag for smoother movement
                Logger.LogInfo($"Configured cart physics: wasKinematic={wasKinematic}, mass={cartRb.mass}, drag={cartRb.drag}");
            }
            
            // 9. Find the PhysGrabObject component and manually trigger its grabbed state
            Component physGrabObject = null;
            foreach (var component in cartObject.GetComponents<Component>())
            {
                if (component.GetType().Name == "PhysGrabObject")
                {
                    physGrabObject = component;
                    break;
                }
            }
            
            if (physGrabObject != null)
            {
                Logger.LogInfo("Found PhysGrabObject component, configuring grabbed state...");
                
                // Based on the PhysGrabObject fields from cart_details.md
                // Set grabbed and grabbedLocal to true
                var grabbedField = physGrabObject.GetType().GetField("grabbed");
                var grabbedLocalField = physGrabObject.GetType().GetField("grabbedLocal");
                var heldByLocalPlayerField = physGrabObject.GetType().GetField("heldByLocalPlayer");
                
                if (grabbedField != null) grabbedField.SetValue(physGrabObject, true);
                if (grabbedLocalField != null) grabbedLocalField.SetValue(physGrabObject, true);
                if (heldByLocalPlayerField != null) heldByLocalPlayerField.SetValue(physGrabObject, true);
                
                Logger.LogInfo("Set cart grabbed state to true");
                
                // Try to call Grab method directly
                var grabMethod = physGrabObject.GetType().GetMethod("Grab");
                if (grabMethod != null)
                {
                    try {
                        grabMethod.Invoke(physGrabObject, null);
                        Logger.LogInfo("Called Grab method on PhysGrabObject");
                    } catch (Exception ex) {
                        Logger.LogInfo($"Error calling Grab method: {ex.Message}");
                    }
                }
                
                // Also try to set the lastPlayerGrabbing field if possible
                var lastPlayerGrabbingField = physGrabObject.GetType().GetField("lastPlayerGrabbing");
                if (lastPlayerGrabbingField != null)
                {
                    // Look for PlayerAvatar component in the scene to use as reference
                    var playerAvatars = GameObject.FindObjectsOfType<Component>()
                        .Where(c => c.GetType().Name == "PlayerAvatar")
                        .ToArray();
                    
                    if (playerAvatars.Length > 0)
                    {
                        lastPlayerGrabbingField.SetValue(physGrabObject, playerAvatars[0]);
                        Logger.LogInfo("Set lastPlayerGrabbing field to a valid PlayerAvatar");
                    }
                }
            }
            
            // 10. Also look for PhysGrabCart component for additional cart-specific functionality
            Component physGrabCart = null;
            foreach (var component in cartObject.GetComponents<Component>())
            {
                if (component.GetType().Name == "PhysGrabCart")
                {
                    physGrabCart = component;
                    break;
                }
            }
            
            if (physGrabCart != null)
            {
                Logger.LogInfo("Found PhysGrabCart component, configuring...");
                
                // Based on PhysGrabCart fields - set cartBeingPulled to true
                var cartBeingPulledField = physGrabCart.GetType().GetField("cartBeingPulled");
                if (cartBeingPulledField != null) cartBeingPulledField.SetValue(physGrabCart, true);
                
                // Set cart active state
                var cartActiveField = physGrabCart.GetType().GetField("cartActive");
                if (cartActiveField != null) cartActiveField.SetValue(physGrabCart, true);
                
                // Change state if possible
                var currentStateField = physGrabCart.GetType().GetField("currentState");
                if (currentStateField != null && currentStateField.FieldType.IsEnum)
                {
                    // Try to find an "Active" or "Grabbed" state in the enum
                    Array enumValues = Enum.GetValues(currentStateField.FieldType);
                    foreach (var value in enumValues)
                    {
                        string enumName = value.ToString();
                        if (enumName.Contains("Active") || enumName.Contains("Grabbed") || enumName.Contains("InUse"))
                        {
                            currentStateField.SetValue(physGrabCart, value);
                            Logger.LogInfo($"Set cart state to: {enumName}");
                            break;
                        }
                    }
                }
            }
            
            // 11. Store the player-cart relationship for restoration
            _lastPlayerSwap = new PlayerCartSwap
            {
                PlayerObject = playerObject,
                CartObject = cartObject,
                OriginalPlayerPosition = originalPlayerPos,
                OriginalPlayerRotation = originalPlayerRot,
                OriginalPlayerParent = originalPlayerParent,
                PlayerController = characterController,
                PlayerComponents = playerComponents,
                OriginalCamera = mainCamera,
                OriginalCameraParent = originalCameraParent,
                OriginalCameraLocalPosition = originalCameraLocalPos,
                OriginalCameraLocalRotation = originalCameraLocalRot,
                CartWasKinematic = wasKinematic,
                CartPhysGrabObject = physGrabObject,
                CartPhysGrabCart = physGrabCart
            };
            
            // 12. Start continual update to handle cart movement and control
            StartCoroutine(UpdateCartControl());
            
            Logger.LogInfo("Attachment completed successfully! You are now controlling the cart!");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error during player-cart attachment: {ex.Message}\n{ex.StackTrace}");
        }
    }
    
    /// <summary>
    /// Coroutine to handle cart control and movement
    /// </summary>
    private System.Collections.IEnumerator UpdateCartControl()
    {
        if (_lastPlayerSwap == null || _lastPlayerSwap.CartObject == null)
        {
            yield break;
        }
        
        Logger.LogInfo("Starting cart control update coroutine");
        
        // Get references for faster access
        GameObject cartObject = _lastPlayerSwap.CartObject;
        Rigidbody cartRb = cartObject.GetComponent<Rigidbody>();
        
        // Main control loop
        while (_lastPlayerSwap != null && _lastPlayerSwap.CartObject != null)
        {
            // Handle WASD input to control cart movement
            float forwardInput = 0f;
            float sidewaysInput = 0f;
            
            if (Input.GetKey(KeyCode.W)) forwardInput += 1f;
            if (Input.GetKey(KeyCode.S)) forwardInput -= 1f;
            if (Input.GetKey(KeyCode.A)) sidewaysInput -= 1f;
            if (Input.GetKey(KeyCode.D)) sidewaysInput += 1f;
            
            // Only apply force if there's input
            if (forwardInput != 0f || sidewaysInput != 0f)
            {
                // Get forward and right directions from the camera
                Camera mainCamera = Camera.main;
                if (mainCamera != null && cartRb != null)
                {
                    Vector3 forward = mainCamera.transform.forward;
                    Vector3 right = mainCamera.transform.right;
                    
                    // Zero out vertical component to keep movement horizontal
                    forward.y = 0;
                    right.y = 0;
                    forward.Normalize();
                    right.Normalize();
                    
                    // Calculate movement direction and apply force
                    Vector3 moveDirection = (forward * forwardInput + right * sidewaysInput).normalized;
                    float moveForce = 7.5f; // Adjust this value to change movement speed
                    
                    cartRb.AddForce(moveDirection * moveForce, ForceMode.Acceleration);
                    
                    // Apply some downward force to keep the cart grounded
                    cartRb.AddForce(Vector3.down * 1.5f, ForceMode.Acceleration);
                }
            }
            
            // Keep the cart's grabbed state active
            if (_lastPlayerSwap.CartPhysGrabObject != null)
            {
                var grabbedField = _lastPlayerSwap.CartPhysGrabObject.GetType().GetField("grabbed");
                var grabbedLocalField = _lastPlayerSwap.CartPhysGrabObject.GetType().GetField("grabbedLocal");
                
                if (grabbedField != null) grabbedField.SetValue(_lastPlayerSwap.CartPhysGrabObject, true);
                if (grabbedLocalField != null) grabbedLocalField.SetValue(_lastPlayerSwap.CartPhysGrabObject, true);
            }
            
            // Wait until next frame
            yield return null;
        }
        
        Logger.LogInfo("Cart control update coroutine ended");
    }
    
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
        
        // Use reflection to get the tracked players
        var trackedPlayersField = trackerType.GetField("_trackedPlayers", BindingFlags.NonPublic | BindingFlags.Instance);
        if (trackedPlayersField != null)
        {
            var trackedPlayers = trackedPlayersField.GetValue(trackerInstance) as System.Collections.IDictionary;
            if (trackedPlayers != null && trackedPlayers.Count > 0)
            {
                // Find the local player among tracked players
                foreach (System.Collections.DictionaryEntry entry in trackedPlayers)
                {
                    var trackedPlayer = entry.Value;
                    var isLocalField = trackedPlayer.GetType().GetField("IsLocal");
                    var gameObjectField = trackedPlayer.GetType().GetField("GameObject");
                    
                    if (isLocalField != null && gameObjectField != null)
                    {
                        bool isLocal = (bool)isLocalField.GetValue(trackedPlayer);
                        if (isLocal)
                        {
                            GameObject go = (GameObject)gameObjectField.GetValue(trackedPlayer);
                            if (go != null)
                            {
                                playerObject = go;
                                Logger.LogInfo($"Found local player from tracker: {Debugging.GetGameObjectPath(playerObject)}");
                                break;
                            }
                        }
                    }
                }
            }
        }
        
        // If we couldn't find the player through PlayerTracker, try other methods
        if (playerObject == null)
        {
            // Common ways to find the player
            playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                Logger.LogInfo($"Found player by tag: {Debugging.GetGameObjectPath(playerObject)}");
            }
            else
            {
                // Try to find by name
                playerObject = GameObject.Find("Player");
                if (playerObject != null)
                {
                    Logger.LogInfo($"Found player by name 'Player': {Debugging.GetGameObjectPath(playerObject)}");
                }
                else
                {
                    // Look for camera objects as a potential indicator of the player
                    var cameraObjs = GameObject.FindObjectsOfType<Camera>()
                        .Where(c => c.name.Contains("Main") || c.name.Contains("Player") || c.name.Contains("FPS"))
                        .ToArray();
                        
                    if (cameraObjs.Length > 0)
                    {
                        Camera playerCam = cameraObjs[0];
                        // Get the player object by traversing up from the camera
                        Transform parent = playerCam.transform;
                        while (parent != null)
                        {
                            // If we find a parent with a suitable name, or with a CharacterController or Rigidbody, that's likely the player
                            if (parent.GetComponent<CharacterController>() != null || 
                                parent.GetComponent<Rigidbody>() != null ||
                                parent.name.Contains("Player") || 
                                parent.name.Contains("Character"))
                            {
                                playerObject = parent.gameObject;
                                Logger.LogInfo($"Found player by traversing up from camera: {Debugging.GetGameObjectPath(playerObject)}");
                                break;
                            }
                            parent = parent.parent;
                        }
                        
                        // If we still haven't found a player, just use the camera's parent
                        if (playerObject == null && playerCam.transform.parent != null)
                        {
                            playerObject = playerCam.transform.parent.gameObject;
                            Logger.LogInfo($"Using camera parent as player: {Debugging.GetGameObjectPath(playerObject)}");
                        }
                    }
                }
            }
        }
        
        if (playerObject != null)
        {
            Logger.LogInfo($"Player object found: {Debugging.GetGameObjectPath(playerObject)}");
            Logger.LogInfo($"Player position: {playerObject.transform.position}");
        }
        else
        {
            Logger.LogError("Could not find player object through any method");
        }
        
        return playerObject;
    }
}
