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
                    
                    // Detach from player
                    Transform originalCameraParent = mainCamera.transform.parent;
                    Vector3 originalCameraLocalPos = mainCamera.transform.localPosition;
                    Quaternion originalCameraLocalRot = mainCamera.transform.localRotation;
                    
                    // Then parent the camera to the cart for a third-person view
                    mainCamera.transform.parent = cartObject.transform;
                    mainCamera.transform.localPosition = new Vector3(0, 3f, -4f);
                    mainCamera.transform.localRotation = Quaternion.Euler(20f, 0, 0);
                    Logger.LogInfo("Positioned camera behind cart");
                }
                
                // Implementation of the rest of the attachment logic
                // ...
                
                // Start continuous update to handle cart movement
                StartCoroutine(UpdateCartControl());
                
                // Also start a coroutine to prevent falling through floors
                StartCoroutine(PreventFallingThroughFloor());
                
                Logger.LogInfo("Now driving the cart! Use WASD to control it like a car.");
                Logger.LogInfo($"Player is {(_playerDamageImmune ? "immune to damage" : "vulnerable to damage")}");
                Logger.LogInfo($"Player model is {(_debugShowPlayerModel ? "visible (debug mode)" : "hidden")}");
                Logger.LogInfo("Press F5 to toggle player model visibility");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error during player-cart attachment: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
} 