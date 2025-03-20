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
    
    [HarmonyPatch]
    public class CartPatch
    {
        // Storage for any cart components we find during runtime
        public static List<Component> FoundCartComponents = new List<Component>();
        public static List<Component> CartControllers = new List<Component>();
        public static GameObject CurrentCartGameObject = null;
        
        // This will become true once we're ready to apply patches
        public static bool PatchesApplied = false;
        
        // Boost settings
        public static float BoostMultiplier = 5.0f;
        public static bool BoostActive = false;
        
        // Fun cart modification flags
        public static bool ZeroGravityMode = false;
        public static bool FloatyCartMode = false;
        public static bool SuperGripMode = false;
        public static bool SlipperyCartMode = false;
        
        // Register a component for patching
        public static void RegisterCartComponent(Component cartComponent, GameObject cartObject)
        {
            if (cartComponent == null) return;
            
            FoundCartComponents.Add(cartComponent);
            CurrentCartGameObject = cartObject;
            Plugin.Logger.LogInfo($"Registered cart component: {cartComponent.GetType().Name}");
            
            // Specifically track the PhysGrabCart components
            if (cartComponent.GetType().Name == "PhysGrabCart")
            {
                if (!CartControllers.Contains(cartComponent))
                {
                    CartControllers.Add(cartComponent);
                    Plugin.Logger.LogInfo($"Found a PhysGrabCart component! Ready to modify! Total carts: {CartControllers.Count}");
                }
            }
            else
            {
                // If it's not a PhysGrabCart but has "Cart" in the name, also add it
                if (cartComponent.GetType().Name.Contains("Cart") && !CartControllers.Contains(cartComponent))
                {
                    CartControllers.Add(cartComponent);
                    Plugin.Logger.LogInfo($"Found a cart-like component: {cartComponent.GetType().Name}. Total carts: {CartControllers.Count}");
                }
            }
            
            // Dynamic patching once we've found our target
            if (!PatchesApplied)
            {
                ApplyDynamicPatches();
            }
        }
        
        // Apply patches to intercept cart methods
        private static void ApplyDynamicPatches()
        {
            try
            {
                if (FoundCartComponents.Count == 0) return;
                
                foreach (var component in FoundCartComponents)
                {
                    Type componentType = component.GetType();
                    
                    // Don't try to patch Unity's built-in types
                    if (componentType.Namespace == "UnityEngine") continue;
                    
                    // We're specifically interested in PhysGrabCart
                    if (componentType.Name == "PhysGrabCart")
                    {
                        Plugin.Logger.LogInfo($"Found PhysGrabCart. Attempting to patch...");
                        
                        // Find the Update or FixedUpdate method to patch
                        MethodInfo updateMethod = componentType.GetMethod("Update", 
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                            
                        if (updateMethod == null)
                        {
                            updateMethod = componentType.GetMethod("FixedUpdate", 
                                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        }
                        
                        if (updateMethod != null)
                        {
                            try 
                            {
                                // Create a dynamic Harmony patch
                                var updatePrefix = typeof(CartPatch).GetMethod("CartUpdate_Prefix", 
                                    BindingFlags.Static | BindingFlags.NonPublic);
                                    
                                if (updatePrefix != null)
                                {
                                    Plugin.Harmony.Patch(updateMethod, 
                                        prefix: new HarmonyMethod(updatePrefix));
                                        
                                    Plugin.Logger.LogInfo($"Successfully patched {componentType.Name}.{updateMethod.Name}()!");
                                    PatchesApplied = true;
                                }
                            }
                            catch (Exception e)
                            {
                                Plugin.Logger.LogError($"Failed to patch {componentType.Name}.{updateMethod.Name}(): {e.Message}");
                            }
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
                // Check if this is our PhysGrabCart
                if (__instance.GetType().Name == "PhysGrabCart")
                {
                    // Get the rigidbody from the cart
                    var rigidbodyField = __instance.GetType().GetField("rb", 
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    
                    if (rigidbodyField != null)
                    {
                        Rigidbody rb = (Rigidbody)rigidbodyField.GetValue(__instance);
                        
                        if (rb != null)
                        {
                            // Apply velocity boost if active
                            if (BoostActive)
                            {
                                Vector3 currentVelocity = rb.velocity;
                                
                                // Only boost if we're already moving (preserve direction)
                                if (currentVelocity.magnitude > 0.1f)
                                {
                                    rb.velocity = currentVelocity * BoostMultiplier;
                                    Plugin.Logger.LogInfo($"Boosting cart velocity to {rb.velocity.magnitude}!");
                                }
                            }
                            
                            // Zero gravity mode
                            if (ZeroGravityMode)
                            {
                                rb.useGravity = false;
                                rb.drag = 0.2f; // Light drag to prevent infinite acceleration
                            }
                            else
                            {
                                rb.useGravity = true;
                            }
                            
                            // Floaty cart mode
                            if (FloatyCartMode)
                            {
                                // Add constant upward force to make the cart float
                                rb.AddForce(Vector3.up * 9.81f * rb.mass * 1.1f); // Slightly more than gravity
                                rb.drag = 1.0f; // More drag to prevent flying away
                            }
                            
                            // Apply physics material changes
                            if (SuperGripMode || SlipperyCartMode)
                            {
                                // Get the physics materials
                                var stickyMatField = __instance.GetType().GetField("physMaterialSticky", 
                                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                    
                                var slipperyMatField = __instance.GetType().GetField("physMaterialSlippery", 
                                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                
                                if (SuperGripMode && stickyMatField != null)
                                {
                                    // Apply sticky material to all colliders
                                    PhysicMaterial stickyMat = (PhysicMaterial)stickyMatField.GetValue(__instance);
                                    if (stickyMat != null)
                                    {
                                        ApplyPhysicsMaterial(__instance.gameObject, stickyMat);
                                    }
                                }
                                else if (SlipperyCartMode && slipperyMatField != null)
                                {
                                    // Apply slippery material to all colliders
                                    PhysicMaterial slipperyMat = (PhysicMaterial)slipperyMatField.GetValue(__instance);
                                    if (slipperyMat != null)
                                    {
                                        ApplyPhysicsMaterial(__instance.gameObject, slipperyMat);
                                    }
                                }
                            }
                        }
                    }
                    
                    // Modify cart state if needed
                    ModifyCartState(__instance);
                }
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError($"Error in CartUpdate_Prefix: {e.Message}");
            }
        }
        
        // Apply a physics material to all colliders on an object
        private static void ApplyPhysicsMaterial(GameObject obj, PhysicMaterial material)
        {
            Collider[] colliders = obj.GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders)
            {
                collider.material = material;
            }
        }
        
        // Modify the cart's state
        private static void ModifyCartState(MonoBehaviour cart)
        {
            try
            {
                // Get the state field
                var stateField = cart.GetType().GetField("currentState", 
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                
                if (stateField != null)
                {
                    // Get the State enum type
                    Type stateType = stateField.FieldType;
                    
                    // We can change cart state to influence behavior
                    // For example, we could set it to "Dragged" state to prevent it from auto-deactivating
                    
                    // Only log state changes every 100 frames to reduce spam
                    if (Time.frameCount % 100 == 0)
                    {
                        var currentState = stateField.GetValue(cart);
                        Plugin.Logger.LogInfo($"Current cart state: {currentState}");
                    }
                }
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError($"Error in ModifyCartState: {e.Message}");
            }
        }
        
        // Add a method to directly boost any carts we know about
        public static void BoostAllCarts()
        {
            try
            {
                BoostActive = true;
                Plugin.Logger.LogInfo($"CART BOOST ACTIVATED! Multiplier: {BoostMultiplier}x");
                
                // Directly boost velocity for all found cart controllers
                foreach (var component in CartControllers)
                {
                    if (component == null) continue;
                    
                    // Try to get the rigidbody
                    var rigidbodyField = component.GetType().GetField("rb", 
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    
                    if (rigidbodyField != null)
                    {
                        Rigidbody rb = (Rigidbody)rigidbodyField.GetValue(component);
                        
                        if (rb != null)
                        {
                            // Apply an immediate impulse for a dramatic boost
                            Vector3 currentVelocity = rb.velocity;
                            
                            // If we have minimal velocity, give an initial push forward
                            if (currentVelocity.magnitude < 0.5f)
                            {
                                rb.AddForce(component.transform.forward * 20f, ForceMode.Impulse);
                                Plugin.Logger.LogInfo("Applied initial impulse to stationary cart!");
                            }
                            else
                            {
                                // Boost existing velocity
                                rb.velocity = currentVelocity * BoostMultiplier;
                                Plugin.Logger.LogInfo($"Direct boosting cart velocity to {rb.velocity.magnitude}!");
                            }
                        }
                    }
                }
                
                // Turn off boost after 5 seconds
                Plugin.Instance.StartCoroutine(DisableBoostAfterDelay(5f));
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError($"Error in BoostAllCarts: {e}");
                BoostActive = false;
            }
        }
        
        // Make the cart float in the air
        public static void ToggleZeroGravity()
        {
            ZeroGravityMode = !ZeroGravityMode;
            FloatyCartMode = false; // Turn off conflicting mode
            Plugin.Logger.LogInfo($"Zero Gravity Mode: {(ZeroGravityMode ? "ENABLED" : "DISABLED")}");
        }
        
        // Make the cart float slightly above the ground
        public static void ToggleFloatyCart()
        {
            FloatyCartMode = !FloatyCartMode;
            ZeroGravityMode = false; // Turn off conflicting mode
            Plugin.Logger.LogInfo($"Floaty Cart Mode: {(FloatyCartMode ? "ENABLED" : "DISABLED")}");
        }
        
        // Make the cart have super grip (no sliding)
        public static void ToggleSuperGrip()
        {
            SuperGripMode = !SuperGripMode;
            SlipperyCartMode = false; // Turn off conflicting mode
            Plugin.Logger.LogInfo($"Super Grip Mode: {(SuperGripMode ? "ENABLED" : "DISABLED")}");
        }
        
        // Make the cart super slippery (ice physics)
        public static void ToggleSlipperyCart()
        {
            SlipperyCartMode = !SlipperyCartMode;
            SuperGripMode = false; // Turn off conflicting mode
            Plugin.Logger.LogInfo($"Slippery Cart Mode: {(SlipperyCartMode ? "ENABLED" : "DISABLED")}");
        }
        
        private static System.Collections.IEnumerator DisableBoostAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            BoostActive = false;
            Plugin.Logger.LogInfo("Cart boost deactivated");
        }
    }
} 