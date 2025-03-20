using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace BecomeCart
{
    /// <summary>
    /// Main plugin class that enables becoming a cart in the game
    /// </summary>
    [BepInPlugin("com.becomecart.repo.plugin", "BecomeCart", "1.0.0")]
    public partial class Plugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;
        internal static HarmonyLib.Harmony Harmony;
        internal static Plugin Instance;

        private void Awake()
        {
            // Plugin startup logic
            Logger = base.Logger;
            Instance = this;
            Harmony = new HarmonyLib.Harmony("com.becomecart.repo.plugin");
            
            try
            {
                Harmony.PatchAll();
                Logger.LogInfo($"Plugin BecomeCart is loaded!");
                Logger.LogInfo("Press F3 to swap the player model with a cart");
                Logger.LogInfo("Press F4 to restore the player model back from cart form");
                Logger.LogInfo("Press F5 to toggle player model visibility");
                
                // Initialize NetworkManager for multiplayer synchronization
                NetworkManager.Instance.name = "BecomeCartNetworkManager";
                Logger.LogInfo("Initialized network manager for multiplayer synchronization");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error during plugin startup: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void Update()
        {
            // Handle key presses
            try
            {
                // Check for F3: Swap player model with cart
                if (Input.GetKeyDown(KeyCode.F3))
                {
                    SwapPlayerModelWithCart();
                }
                
                // Check for F4: Restore player from cart
                if (Input.GetKeyDown(KeyCode.F4))
                {
                    RestorePlayerFromCart();
                }
                
                // Check for F5: Toggle player model visibility in cart mode
                if (Input.GetKeyDown(KeyCode.F5))
                {
                    _debugShowPlayerModel = !_debugShowPlayerModel;
                    Logger.LogInfo($"Player model visibility toggled to {(_debugShowPlayerModel ? "visible" : "hidden")}");
                    
                    // Update visibility if player is currently swapped
                    if (_lastPlayerSwap != null && _lastPlayerSwap.PlayerObject != null)
                    {
                        foreach (Renderer renderer in _lastPlayerSwap.PlayerObject.GetComponentsInChildren<Renderer>(true))
                        {
                            string rendererPath = Debugging.GetGameObjectPath(renderer.gameObject);
                            if (_playerVisibilityState.ContainsKey(rendererPath))
                            {
                                renderer.enabled = _debugShowPlayerModel;
                            }
                        }
                    }
                }
                
                // Update cart control if active
                if (_isControllingCart && _lastPlayerSwap != null)
                {
                    UpdateCartControl();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error in Update: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
} 