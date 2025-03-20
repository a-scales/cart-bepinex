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
}
