using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
using System.Collections;

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
        
        // F9: Find the Item Cart Medium and apply ALL modifications at once
        if (Input.GetKeyDown(KeyCode.F9))
        {
            Logger.LogInfo("=== FINDING ITEM CART MEDIUM AND APPLYING ALL MODS ===");
            
            // Look specifically for "Item Cart Medium" objects
            var cartObjects = GameObject.FindObjectsOfType<GameObject>()
                .Where(obj => obj.name.Contains("Item Cart Medium"))
                .ToArray();
                
            if (cartObjects.Length > 0)
            {
                GameObject cart = cartObjects[0];
                Logger.LogInfo($"Found cart: {Debugging.GetGameObjectPath(cart)}");
                
                // Find the PhysGrabCart component
                var physGrabCart = cart.GetComponent<MonoBehaviour>();
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
                    
                    // Register it for modifications
                    CartPatch.RegisterCartComponent(physGrabCart, cart);
                    
                    // Apply all modifications at once
                    Logger.LogInfo("APPLYING ALL MODIFICATIONS AT ONCE!");
                    
                    // Boost speed
                    CartPatch.BoostMultiplier = 5.0f;
                    CartPatch.BoostAllCarts();
                    
                    // Enable fun modes
                    CartPatch.ZeroGravityMode = true;
                    CartPatch.ToggleZeroGravity();
                    
                    Logger.LogInfo("All modifications applied! Cart is ready to go!");
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
                    
                    // Register it for modifications
                    CartPatch.RegisterCartComponent(physGrabCart, physGrabCart.gameObject);
                    
                    // Apply all modifications at once
                    Logger.LogInfo("APPLYING ALL MODIFICATIONS AT ONCE!");
                    
                    // Boost speed
                    CartPatch.BoostMultiplier = 5.0f;
                    CartPatch.BoostAllCarts();
                    
                    // Enable fun modes
                    CartPatch.ZeroGravityMode = true;
                    CartPatch.ToggleZeroGravity();
                    
                    Logger.LogInfo("All modifications applied! Cart is ready to go!");
                }
                else
                {
                    Logger.LogInfo("No carts found in the scene at all. Try a different level!");
                }
            }
        }
        
        // F10: Stop tracking cart objects
        if (Input.GetKeyDown(KeyCode.F10))
        {
            CartTracker.Instance.StopTrackingAll();
            Logger.LogInfo("Stopped tracking all cart objects");
        }
        
        // F11: Manually register a cart component under the cursor
        if (Input.GetKeyDown(KeyCode.F11))
        {
            Logger.LogInfo("=== ATTEMPTING TO REGISTER CART COMPONENT UNDER CURSOR ===");
            
            // Try to get the object under the cursor using a raycast
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit))
            {
                GameObject hitObject = hit.collider.gameObject;
                Logger.LogInfo($"Found object under cursor: {Debugging.GetGameObjectPath(hitObject)}");
                
                // Look for interesting components on this object
                Component[] components = hitObject.GetComponents<Component>();
                foreach (Component component in components)
                {
                    if (component == null) continue;
                    
                    string typeName = component.GetType().Name;
                    if (typeName.Contains("Controller") || 
                        typeName.Contains("Movement") || 
                        typeName.Contains("Motor") ||
                        typeName.Contains("Drive") ||
                        typeName.Contains("Vehicle"))
                    {
                        Logger.LogInfo($"Registering cart component: {typeName}");
                        CartPatch.RegisterCartComponent(component, hitObject);
                    }
                }
                
                // Also check parent objects (cart controller might be on a parent)
                Transform parent = hitObject.transform.parent;
                if (parent != null)
                {
                    Logger.LogInfo($"Checking parent object: {parent.name}");
                    components = parent.GetComponents<Component>();
                    foreach (Component component in components)
                    {
                        if (component == null) continue;
                        
                        string typeName = component.GetType().Name;
                        if (typeName.Contains("Controller") || 
                            typeName.Contains("Movement") || 
                            typeName.Contains("Motor") ||
                            typeName.Contains("Drive") ||
                            typeName.Contains("Vehicle"))
                        {
                            Logger.LogInfo($"Registering cart component from parent: {typeName}");
                            CartPatch.RegisterCartComponent(component, parent.gameObject);
                        }
                    }
                }
            }
            else
            {
                Logger.LogInfo("No object found under cursor");
            }
        }
        
        // F12: SUPER BOOST any registered carts
        if (Input.GetKeyDown(KeyCode.F12))
        {
            Logger.LogInfo("=== SUPER BOOST ACTIVATED ===");
            
            // Double the normal boost multiplier for super boost
            float originalMultiplier = CartPatch.BoostMultiplier;
            CartPatch.BoostMultiplier = 10.0f;
            
            if (CartPatch.CartControllers.Count > 0)
            {
                Logger.LogInfo($"SUPER BOOSTING {CartPatch.CartControllers.Count} CARTS!");
                CartPatch.BoostAllCarts();
            }
            else
            {
                Logger.LogInfo("No carts registered yet. Use F9 to find carts or F11 to register a cart under your cursor.");
            }
            
            // Reset the multiplier after a delay
            StartCoroutine(ResetBoostMultiplier(originalMultiplier, 5f));
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
}
