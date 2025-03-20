using HarmonyLib;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace BecomeCart
{
    /// <summary>
    /// This class contains patches that will be applied once you've identified
    /// the cart component in your game.
    /// </summary>
    
    // This patch will be applied to any MonoBehaviour we find that could be a cart controller
    [HarmonyPatch]
    public class CartPatch
    {
        // Storage for any cart components we find during runtime
        public static List<Component> FoundCartComponents = new List<Component>();
        public static GameObject CurrentCartGameObject = null;
        
        // This will become true once we're ready to apply patches
        public static bool PatchesApplied = false;
        
        // Called from Plugin.Update once we've found and confirmed a cart
        public static void RegisterCartComponent(Component cartComponent, GameObject cartObject)
        {
            if (cartComponent == null) return;
            
            FoundCartComponents.Add(cartComponent);
            CurrentCartGameObject = cartObject;
            Plugin.Logger.LogInfo($"Registered cart component: {cartComponent.GetType().Name}");
            
            // Dynamic patching once we've found our target
            if (!PatchesApplied)
            {
                ApplyDynamicPatches();
            }
        }
        
        // Dynamically apply patches once we know the actual types
        private static void ApplyDynamicPatches()
        {
            try
            {
                if (FoundCartComponents.Count == 0) return;
                
                // For demonstration, we'll attempt to patch a cart component's Update method
                foreach (var component in FoundCartComponents)
                {
                    Type componentType = component.GetType();
                    
                    // Don't try to patch Unity's built-in types
                    if (componentType.Namespace == "UnityEngine") continue;
                    
                    MethodInfo updateMethod = componentType.GetMethod("Update", 
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    
                    if (updateMethod != null)
                    {
                        Plugin.Logger.LogInfo($"Found Update method on {componentType.Name}. Attempting to patch...");
                        
                        try 
                        {
                            // Create a dynamic Harmony patch
                            var updatePrefix = typeof(CartPatch).GetMethod("CartUpdate_Prefix", 
                                BindingFlags.Static | BindingFlags.NonPublic);
                                
                            if (updatePrefix != null)
                            {
                                Plugin.Harmony.Patch(updateMethod, 
                                    prefix: new HarmonyMethod(updatePrefix));
                                    
                                Plugin.Logger.LogInfo($"Successfully patched {componentType.Name}.Update()!");
                                PatchesApplied = true;
                            }
                        }
                        catch (Exception e)
                        {
                            Plugin.Logger.LogError($"Failed to patch {componentType.Name}.Update(): {e.Message}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError($"Error in ApplyDynamicPatches: {e}");
            }
        }
        
        // This method will be called before a cart component's Update method
        private static void CartUpdate_Prefix(MonoBehaviour __instance)
        {
            try
            {
                // Example: Log that the cart update is running
                if (Time.frameCount % 300 == 0)  // Log only every 300 frames to avoid spam
                {
                    Plugin.Logger.LogInfo($"Cart update running on {__instance.GetType().Name}");
                    
                    // Here you would add code to modify cart behavior
                    // For example: Increase the cart's speed, make it jump, etc.
                }
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError($"Error in CartUpdate_Prefix: {e.Message}");
            }
        }
    }
} 