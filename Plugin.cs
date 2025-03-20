using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;

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
        
        // F9: Find and track cart objects
        if (Input.GetKeyDown(KeyCode.F9))
        {
            Logger.LogInfo("=== LOOKING FOR CART OBJECTS ===");
            
            // Look for objects with "cart" in the name
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            var cartObjects = allObjects
                .Where(obj => obj.name.ToLower().Contains("cart") || 
                             obj.name.ToLower().Contains("vehicle") ||
                             obj.name.ToLower().Contains("transport"))
                .ToArray();
            
            Logger.LogInfo($"Found {cartObjects.Length} potential cart objects");
            
            // Clear previous tracking
            CartTracker.Instance.StopTrackingAll();
            
            foreach (var cartObj in cartObjects)
            {
                string path = Debugging.GetGameObjectPath(cartObj);
                Logger.LogInfo($"Potential cart: {path}");
                
                // Log position and rotation for later verification
                Logger.LogInfo($"  Position: {cartObj.transform.position}");
                Logger.LogInfo($"  Rotation: {cartObj.transform.rotation.eulerAngles}");
                
                // Dump components to see what makes this cart work
                DumpGameObject(cartObj);
                
                // Start tracking this object
                CartTracker.Instance.TrackObject(cartObj);
            }
            
            // Also look for MonoBehaviours that might control carts
            var cartControllers = GameObject.FindObjectsOfType<MonoBehaviour>()
                .Where(mb => mb.GetType().Name.ToLower().Contains("cart") ||
                            mb.GetType().Name.ToLower().Contains("vehicle") ||
                            mb.GetType().Name.ToLower().Contains("driv"))
                .ToArray();
            
            Logger.LogInfo($"Found {cartControllers.Length} potential cart controller scripts");
            
            foreach (var controller in cartControllers)
            {
                Logger.LogInfo($"Potential controller: {controller.GetType().Name} on {Debugging.GetGameObjectPath(controller.gameObject)}");
                // Log fields to understand the controller's properties
                Debugging.DumpFields(controller);
                
                // Also track the GameObject this component is attached to
                CartTracker.Instance.TrackObject(controller.gameObject);
            }
            
            Logger.LogInfo("Started tracking potential cart objects. Movement will be logged every 5 seconds.");
            Logger.LogInfo("Press F10 to stop tracking.");
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
    }
}
