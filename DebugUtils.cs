using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BepInEx.Logging;

namespace BecomeCart
{
    public partial class Plugin
    {
        /// <summary>
        /// Dumps game object information to logs or file
        /// </summary>
        public static void DumpGameObject(GameObject obj, int depth = 0)
        {
            if (obj == null)
            {
                Logger.LogInfo("GameObject is null");
                return;
            }

            // Implementation of GameObject dumping logic
        }

        /// <summary>
        /// Dumps detailed information about a game object and its components to the logs
        /// </summary>
        private void DumpGameObjectAndComponents(GameObject obj, int depth)
        {
            // Implementation of component dumping logic
        }

        /// <summary>
        /// Process the request to dump cart details, finding and documenting cart objects
        /// </summary>
        private void ProcessDumpCartDetails(bool findAndRegister)
        {
            Logger.LogInfo($"Processing cart details dump (find mode: {findAndRegister})");
            
            try
            {
                if (findAndRegister)
                {
                    // Find all carts in the scene
                    GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
                    List<GameObject> carts = new List<GameObject>();
                    
                    foreach (GameObject obj in allObjects)
                    {
                        // Look for objects with cart-related components or naming
                        if (obj.name.Contains("Cart") || 
                            ComponentUtility.HasComponentByName(obj, "PhysGrabCart") || 
                            ComponentUtility.HasComponentByName(obj, "CartController"))
                        {
                            carts.Add(obj);
                            Logger.LogInfo($"Found cart: {Debugging.GetGameObjectPath(obj)}");
                        }
                    }
                    
                    if (carts.Count > 0)
                    {
                        _lastFoundCart = carts[0]; // Take the first one for simplicity
                        Logger.LogInfo($"Registered cart for detailed inspection: {Debugging.GetGameObjectPath(_lastFoundCart)}");
                        
                        // Dump the details of this cart to a file
                        DumpCartDetailsToFile(_lastFoundCart);
                    }
                    else
                    {
                        Logger.LogInfo("No carts found in the scene");
                    }
                }
                else
                {
                    // Dump details of the previously found cart
                    if (_lastFoundCart != null)
                    {
                        Logger.LogInfo($"Dumping details of previously registered cart: {Debugging.GetGameObjectPath(_lastFoundCart)}");
                        DumpCartDetailsToFile(_lastFoundCart);
                    }
                    else
                    {
                        Logger.LogInfo("No cart previously registered, please use F9 first to find a cart");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error during cart details processing: {ex.Message}\n{ex.StackTrace}");
            }
        }
        
        /// <summary>
        /// Dumps details about a cart to a markdown file
        /// </summary>
        private void DumpCartDetailsToFile(GameObject cart)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("## Cart Summary");
                sb.AppendLine("- **Basic Identification**: Medium-sized cart with physics-based interactions");
                sb.AppendLine("- **Control Methods**:");
                sb.AppendLine("  - Basic grabbing via PhysGrabObject component");
                sb.AppendLine("  - Enhanced control through dedicated Cart Handle and Cart Grab Point");
                sb.AppendLine("  - State system (currently \"Locked\") controls behavior modes");
                sb.AppendLine("- **Item Management**:");
                sb.AppendLine("  - PhysGrabInCart component tracks items inside the cart");
                sb.AppendLine("  - Collider system keeps items contained");
                sb.AppendLine("  - Display shows current haul value");
                sb.AppendLine("- **Physics Properties**:");
                sb.AppendLine("  - Mass: 2, Drag: 2, Angular Drag: 4");
                sb.AppendLine("  - Multiple specialized physics materials for different situations");
                sb.AppendLine("  - Stabilization force (100) helps maintain balance");
                sb.AppendLine("  - Three capsule colliders configured as wheels for movement");
                sb.AppendLine("- **Visual Feedback**:");
                sb.AppendLine("  - Screen with TextMeshPro display shows information");
                sb.AppendLine("  - Handle system provides visual indicators for interaction");
                sb.AppendLine("  - Particle effects for impacts and movement");
                sb.AppendLine();
                
                // Add detailed component information
                sb.AppendLine($"[Info   :BecomeCart] Cart: {Debugging.GetGameObjectPath(cart)}");
                sb.AppendLine($"[Info   :BecomeCart] GameObject: {cart.name}, Active: {cart.activeSelf}, Layer: {LayerMask.LayerToName(cart.layer)}");
                
                // Dump components
                Component[] components = cart.GetComponents<Component>();
                foreach (Component component in components)
                {
                    if (component == null) continue;
                    sb.AppendLine($"[Info   :BecomeCart]   Component: {component.GetType().Name}");
                    
                    // You would add detailed field information for each component here
                }
                
                // Dump children
                DumpChildrenToStringBuilder(sb, cart, 0);
                
                // Write to file
                File.WriteAllText("cart_details.md", sb.ToString());
                Logger.LogInfo("Successfully wrote cart details to cart_details.md");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error writing cart details to file: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Recursively dumps child GameObjects to a StringBuilder
        /// </summary>
        private void DumpChildrenToStringBuilder(StringBuilder sb, GameObject obj, int depth)
        {
            if (depth > 0)
            {
                sb.AppendLine($"[Info   :BecomeCart]   Children ({obj.transform.childCount}):");
            }
            
            foreach (Transform child in obj.transform)
            {
                string indent = new string(' ', (depth + 1) * 2);
                sb.AppendLine($"[Info   :BecomeCart] {indent}GameObject: {child.name}, Active: {child.gameObject.activeSelf}, Layer: {LayerMask.LayerToName(child.gameObject.layer)}");
                
                // Log components
                Component[] components = child.GetComponents<Component>();
                foreach (Component component in components)
                {
                    if (component == null) continue;
                    sb.AppendLine($"[Info   :BecomeCart] {indent}  Component: {component.GetType().Name}");
                }
                
                // Recursively log children (limited depth)
                if (depth < 3 && child.childCount > 0)
                {
                    DumpChildrenToStringBuilder(sb, child.gameObject, depth + 1);
                }
            }
        }

        /// <summary>
        /// Dumps player health information to logs or files
        /// </summary>
        private void DumpPlayerHealthInfo()
        {
            Logger.LogInfo("Dumping player details to file...");
            
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("## Player Summary");
                
                // Get all tracked players
                List<GameObject> players = PlayerTracker.Instance.GetAllPlayers();
                
                if (players.Count == 0)
                {
                    Logger.LogInfo("No players tracked yet. Press F11 first to find and track players.");
                    return;
                }
                
                sb.AppendLine($"- Total Players: {players.Count}");
                
                // Dump details for each player
                foreach (GameObject player in players)
                {
                    if (player == null) continue;
                    
                    sb.AppendLine($"\n### Player: {player.name}");
                    sb.AppendLine($"- Path: {Debugging.GetGameObjectPath(player)}");
                    sb.AppendLine($"- Position: {player.transform.position}");
                    sb.AppendLine($"- Rotation: {player.transform.rotation.eulerAngles}");
                    sb.AppendLine($"- Active: {player.activeSelf}");
                    
                    // Check if this is the local player
                    bool isLocalPlayer = player == PlayerTracker.Instance.LocalPlayer;
                    sb.AppendLine($"- Is Local Player: {isLocalPlayer}");
                    
                    // Dump components
                    sb.AppendLine("\n#### Components:");
                    Component[] components = player.GetComponents<Component>();
                    foreach (Component component in components)
                    {
                        if (component == null) continue;
                        sb.AppendLine($"- {component.GetType().Name}");
                        
                        // Check for health-related components
                        string typeName = component.GetType().Name;
                        if (typeName.Contains("Health") || typeName.Contains("Damage") || 
                            typeName.Contains("Player") || typeName.Contains("Controller"))
                        {
                            sb.AppendLine($"  - **Health-Related Component**: {typeName}");
                            // You would dump the fields here
                        }
                    }
                    
                    // Dump child objects (controllers, weapons, etc.)
                    sb.AppendLine("\n#### Child Objects:");
                    DumpPlayerChildrenToStringBuilder(sb, player, 0);
                }
                
                // Write to file
                File.WriteAllText("player_details.md", sb.ToString());
                Logger.LogInfo("Successfully wrote player details to player_details.md");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error dumping player details: {ex.Message}\n{ex.StackTrace}");
            }
        }
        
        /// <summary>
        /// Recursively dumps child GameObjects to a StringBuilder for player details
        /// </summary>
        private void DumpPlayerChildrenToStringBuilder(StringBuilder sb, GameObject obj, int depth)
        {
            string indent = new string(' ', (depth + 1) * 2);
            
            foreach (Transform child in obj.transform)
            {
                sb.AppendLine($"{indent}- {child.name} (Active: {child.gameObject.activeSelf})");
                
                // Log components (summarized)
                Component[] components = child.GetComponents<Component>();
                if (components.Length > 0)
                {
                    sb.Append($"{indent}  - Components: ");
                    List<string> componentNames = new List<string>();
                    foreach (Component component in components)
                    {
                        if (component != null)
                            componentNames.Add(component.GetType().Name);
                    }
                    sb.AppendLine(string.Join(", ", componentNames));
                }
                
                // Recursively log children (limited depth)
                if (depth < 2 && child.childCount > 0)
                {
                    DumpPlayerChildrenToStringBuilder(sb, child.gameObject, depth + 1);
                }
                else if (child.childCount > 0)
                {
                    sb.AppendLine($"{indent}  - ({child.childCount} more children not shown)");
                }
            }
        }
    }
} 