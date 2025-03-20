using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using BepInEx.Logging;

namespace BecomeCart
{
    /// <summary>
    /// Handles network synchronization using Photon
    /// </summary>
    public class NetworkManager : MonoBehaviour
    {
        // Constants for custom event codes
        private const byte EVENT_PLAYER_CART_SWAP = 111;
        private const byte EVENT_PLAYER_CART_RESTORE = 112;
        
        // Singleton instance
        private static NetworkManager _instance;
        public static NetworkManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("BecomeCartNetworkManager");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<NetworkManager>();
                }
                return _instance;
            }
        }

        // Reference to the MonoBehaviour subclass that represents PhotonNetwork
        private static Type _photonNetworkType;
        
        // Cached methods and properties for Photon
        private static MethodInfo _raiseEventMethod;
        private static PropertyInfo _localPlayerProperty;
        private static MethodInfo _getPhotonViewMethod;
        
        // Event handler for Photon events
        private MonoBehaviour _eventHandler;
        private MethodInfo _addCallbackMethod;
        private MethodInfo _removeCallbackMethod;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this);
                return;
            }
            
            _instance = this;
            
            // Try to find Photon types via reflection
            InitializePhotonReferences();
        }

        /// <summary>
        /// Initializes references to Photon types and methods via reflection
        /// </summary>
        private void InitializePhotonReferences()
        {
            try
            {
                // Find PhotonNetwork type
                _photonNetworkType = Type.GetType("Photon.Pun.PhotonNetwork, Photon.Pun");
                if (_photonNetworkType == null)
                {
                    // Try with assembly-qualified name
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        var type = assembly.GetType("Photon.Pun.PhotonNetwork");
                        if (type != null)
                        {
                            _photonNetworkType = type;
                            break;
                        }
                    }
                }
                
                if (_photonNetworkType != null)
                {
                    Plugin.Logger.LogInfo("Found PhotonNetwork type");
                    
                    // Get RaiseEvent method
                    _raiseEventMethod = _photonNetworkType.GetMethod("RaiseEvent", 
                        BindingFlags.Public | BindingFlags.Static);
                    
                    if (_raiseEventMethod != null)
                        Plugin.Logger.LogInfo("Found RaiseEvent method");
                    
                    // Get LocalPlayer property
                    _localPlayerProperty = _photonNetworkType.GetProperty("LocalPlayer", 
                        BindingFlags.Public | BindingFlags.Static);
                    
                    if (_localPlayerProperty != null)
                        Plugin.Logger.LogInfo("Found LocalPlayer property");
                    
                    // Get GetPhotonView method
                    _getPhotonViewMethod = _photonNetworkType.GetMethod("GetPhotonView", 
                        BindingFlags.Public | BindingFlags.Static);
                    
                    if (_getPhotonViewMethod != null)
                        Plugin.Logger.LogInfo("Found GetPhotonView method");
                    
                    // Set up event callbacks
                    SetupEventCallbacks();
                }
                else
                {
                    Plugin.Logger.LogError("Could not find PhotonNetwork type!");
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Error initializing Photon references: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Sets up the callbacks for Photon events
        /// </summary>
        private void SetupEventCallbacks()
        {
            try
            {
                // Try to find NetworkEventCallbacks component
                Type networkCallbacksType = null;
                
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var type = assembly.GetType("Photon.Pun.IOnEventCallback") ?? 
                               assembly.GetType("ExitGames.Client.Photon.IOnEventCallback");
                    if (type != null)
                    {
                        networkCallbacksType = type;
                        break;
                    }
                }
                
                if (networkCallbacksType != null)
                {
                    Plugin.Logger.LogInfo($"Found network callbacks interface: {networkCallbacksType.FullName}");
                    
                    // Create a dynamic implementation of IOnEventCallback
                    // Since we can't directly implement it at runtime, we'll hook into an existing implementation
                    
                    // Find a MonoBehaviour that handles AddCallback
                    Type networkingCallbacksType = null;
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        var types = assembly.GetTypes().Where(t => 
                            typeof(MonoBehaviour).IsAssignableFrom(t) && 
                            t.Name.Contains("NetworkingCallbacks")).ToArray();
                        
                        if (types.Length > 0)
                        {
                            networkingCallbacksType = types[0];
                            break;
                        }
                    }
                    
                    if (networkingCallbacksType != null)
                    {
                        Plugin.Logger.LogInfo($"Found networking callbacks class: {networkingCallbacksType.FullName}");
                        
                        // Find AddCallback method
                        _addCallbackMethod = networkingCallbacksType.GetMethod("AddCallback", 
                            BindingFlags.Public | BindingFlags.Static);
                            
                        if (_addCallbackMethod != null)
                            Plugin.Logger.LogInfo("Found AddCallback method");
                            
                        // Find RemoveCallback method
                        _removeCallbackMethod = networkingCallbacksType.GetMethod("RemoveCallback", 
                            BindingFlags.Public | BindingFlags.Static);
                            
                        if (_removeCallbackMethod != null)
                            Plugin.Logger.LogInfo("Found RemoveCallback method");
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Error setting up event callbacks: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Called when a Photon event is received
        /// </summary>
        public void OnEvent(object eventData)
        {
            try
            {
                // Extract event code and player data from the event
                byte eventCode = (byte)eventData.GetType().GetProperty("Code").GetValue(eventData);
                object customData = eventData.GetType().GetProperty("CustomData").GetValue(eventData);
                
                switch (eventCode)
                {
                    case EVENT_PLAYER_CART_SWAP:
                        HandlePlayerCartSwap(customData);
                        break;
                        
                    case EVENT_PLAYER_CART_RESTORE:
                        HandlePlayerCartRestore(customData);
                        break;
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Error handling network event: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles a player-cart swap event from the network
        /// </summary>
        private void HandlePlayerCartSwap(object customData)
        {
            try
            {
                // Extract player ID and cart ID from custom data
                object[] data = customData as object[];
                if (data == null || data.Length < 2)
                    return;
                
                int playerActorNumber = (int)data[0];
                string cartPath = (string)data[1];
                
                // Skip if this is our own player
                if (IsLocalPlayerActor(playerActorNumber))
                    return;
                
                Plugin.Logger.LogInfo($"Received cart swap event for player {playerActorNumber}, cart: {cartPath}");
                
                // Find the player and cart
                GameObject playerObj = PlayerTracker.Instance.GetPlayerByActorNumber(playerActorNumber);
                GameObject cartObject = GameObject.Find(cartPath);
                
                if (playerObj != null && cartObject != null)
                {
                    // Apply visual changes for remote player
                    Plugin.Instance.AttachPlayerToCartVisuallyInternal(playerObj, cartObject);
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Error handling cart swap event: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles a player-cart restore event from the network
        /// </summary>
        private void HandlePlayerCartRestore(object customData)
        {
            try
            {
                // Extract player ID from custom data
                object[] data = customData as object[];
                if (data == null || data.Length < 1)
                    return;
                
                int playerActorNumber = (int)data[0];
                
                // Skip if this is our own player
                if (IsLocalPlayerActor(playerActorNumber))
                    return;
                
                Plugin.Logger.LogInfo($"Received cart restore event for player {playerActorNumber}");
                
                // Find the player
                GameObject playerObj = PlayerTracker.Instance.GetPlayerByActorNumber(playerActorNumber);
                
                if (playerObj != null)
                {
                    // Restore visual state for remote player
                    Plugin.Instance.RestorePlayerFromCartVisuallyInternal(playerObj);
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Error handling cart restore event: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if the given actor number is the local player
        /// </summary>
        private bool IsLocalPlayerActor(int actorNumber)
        {
            if (_localPlayerProperty != null)
            {
                try
                {
                    object localPlayer = _localPlayerProperty.GetValue(null);
                    if (localPlayer != null)
                    {
                        int localActorNumber = (int)localPlayer.GetType().GetProperty("ActorNumber").GetValue(localPlayer);
                        return localActorNumber == actorNumber;
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Logger.LogError($"Error checking local player actor: {ex.Message}");
                }
            }
            
            // Fall back to checking PlayerTracker
            GameObject localPlayerObj = PlayerTracker.Instance.LocalPlayer;
            if (localPlayerObj != null)
            {
                int localActorNumber = PlayerTracker.Instance.GetPlayerActorNumber(localPlayerObj);
                return localActorNumber == actorNumber;
            }
            
            return false;
        }

        /// <summary>
        /// Sends a player-cart swap event to other players
        /// </summary>
        public void SendPlayerCartSwap(GameObject playerObject, GameObject cartObject)
        {
            try
            {
                int playerActorNumber = PlayerTracker.Instance.GetPlayerActorNumber(playerObject);
                string cartPath = Debugging.GetGameObjectPath(cartObject);
                
                if (playerActorNumber == -1)
                {
                    Plugin.Logger.LogWarning("Cannot send cart swap event: Player has no actor number");
                    return;
                }
                
                // Create event data
                object[] eventContent = new object[] { playerActorNumber, cartPath };
                
                // Send the event
                SendPhotonEvent(EVENT_PLAYER_CART_SWAP, eventContent);
                
                Plugin.Logger.LogInfo($"Sent cart swap event: Player={playerActorNumber}, Cart={cartPath}");
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Error sending cart swap event: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a player-cart restore event to other players
        /// </summary>
        public void SendPlayerCartRestore(GameObject playerObject)
        {
            try
            {
                int playerActorNumber = PlayerTracker.Instance.GetPlayerActorNumber(playerObject);
                
                if (playerActorNumber == -1)
                {
                    Plugin.Logger.LogWarning("Cannot send cart restore event: Player has no actor number");
                    return;
                }
                
                // Create event data
                object[] eventContent = new object[] { playerActorNumber };
                
                // Send the event
                SendPhotonEvent(EVENT_PLAYER_CART_RESTORE, eventContent);
                
                Plugin.Logger.LogInfo($"Sent cart restore event: Player={playerActorNumber}");
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Error sending cart restore event: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a Photon event using reflection
        /// </summary>
        private void SendPhotonEvent(byte eventCode, object eventContent)
        {
            if (_raiseEventMethod == null)
            {
                Plugin.Logger.LogError("Cannot send Photon event: RaiseEvent method not found");
                return;
            }
            
            try
            {
                // Create necessary parameters for RaiseEvent
                // RaiseEvent(byte eventCode, object eventContent, RaiseEventOptions raiseEventOptions, SendOptions sendOptions)
                
                // Create RaiseEventOptions with Receivers = Others
                Type raiseEventOptionsType = Type.GetType("ExitGames.Client.Photon.Hashtable, Photon.Realtime");
                if (raiseEventOptionsType == null)
                {
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        var type = assembly.GetType("ExitGames.Client.Photon.Hashtable") ?? 
                                   assembly.GetType("Photon.Realtime.RaiseEventOptions");
                        if (type != null)
                        {
                            raiseEventOptionsType = type;
                            break;
                        }
                    }
                }
                
                object raiseEventOptions = Activator.CreateInstance(raiseEventOptionsType);
                
                // Set Receivers = Others
                // First find the ReceiverGroup enum
                Type receiverGroupType = null;
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var type = assembly.GetType("ExitGames.Client.Photon.ReceiverGroup") ?? 
                               assembly.GetType("Photon.Realtime.ReceiverGroup");
                    if (type != null)
                    {
                        receiverGroupType = type;
                        break;
                    }
                }
                
                if (receiverGroupType != null)
                {
                    // Get the Others value from the enum
                    object othersValue = Enum.Parse(receiverGroupType, "Others");
                    
                    // Set the Receivers property
                    raiseEventOptionsType.GetProperty("Receivers").SetValue(raiseEventOptions, othersValue);
                }
                
                // Create SendOptions with Reliability = true
                Type sendOptionsType = null;
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var type = assembly.GetType("ExitGames.Client.Photon.SendOptions") ?? 
                               assembly.GetType("Photon.Realtime.SendOptions");
                    if (type != null)
                    {
                        sendOptionsType = type;
                        break;
                    }
                }
                
                object sendOptions = sendOptionsType.GetProperty("SendReliable", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
                
                // Call RaiseEvent
                _raiseEventMethod.Invoke(null, new object[] { eventCode, eventContent, raiseEventOptions, sendOptions });
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Error sending Photon event: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Transfers ownership of a GameObject to the local player
        /// </summary>
        public void TransferCartOwnership(GameObject cartObject)
        {
            if (cartObject == null)
                return;
                
            try
            {
                // Find the PhotonView on the cart
                MonoBehaviour photonView = cartObject.GetComponentInChildren<MonoBehaviour>();
                if (photonView != null && photonView.GetType().Name == "PhotonView")
                {
                    // Get the TransferOwnership method
                    MethodInfo transferMethod = photonView.GetType().GetMethod("TransferOwnership");
                    if (transferMethod != null)
                    {
                        // Get local player's actor number
                        if (_localPlayerProperty != null)
                        {
                            object localPlayer = _localPlayerProperty.GetValue(null);
                            if (localPlayer != null)
                            {
                                int localActorNumber = (int)localPlayer.GetType().GetProperty("ActorNumber").GetValue(localPlayer);
                                
                                // Transfer ownership
                                transferMethod.Invoke(photonView, new object[] { localActorNumber });
                                Plugin.Logger.LogInfo($"Transferred ownership of cart to local player (Actor: {localActorNumber})");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Error transferring cart ownership: {ex.Message}");
            }
        }
    }
} 