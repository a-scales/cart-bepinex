using UnityEngine;
using System.Collections;

namespace BecomeCart
{
    public partial class Plugin
    {
        /// <summary>
        /// Prevents the cart from falling through floors by monitoring its Y position
        /// </summary>
        private IEnumerator PreventFallingThroughFloor()
        {
            if (_lastPlayerSwap == null || _lastPlayerSwap.CartObject == null)
                yield break;
                    
            GameObject cartObject = _lastPlayerSwap.CartObject;
            Transform cartTransform = cartObject.transform;
            Vector3 lastPosition = cartTransform.position;
            float lowestAllowedY = -100f; // Set this to the minimum Y value for your game world
            
            Logger.LogInfo("Starting floor protection coroutine");
            
            while (_lastPlayerSwap != null)
            {
                // Check if cart has fallen through floor
                if (cartTransform.position.y < lowestAllowedY)
                {
                    Logger.LogWarning("Cart detected falling through floor! Restoring to last safe position");
                    cartTransform.position = lastPosition;
                    
                    // Also reset velocity
                    Rigidbody cartRb = cartObject.GetComponent<Rigidbody>();
                    if (cartRb != null)
                    {
                        cartRb.velocity = Vector3.zero;
                        cartRb.angularVelocity = Vector3.zero;
                    }
                }
                else if (cartTransform.position.y > lastPosition.y - 10)
                {
                    // Only update last position if we haven't fallen too far
                    // This prevents updating the "safe" position while in freefall
                    lastPosition = cartTransform.position;
                }
                
                yield return new WaitForSeconds(0.2f);
            }
            
            Logger.LogInfo("Floor protection coroutine ended");
        }
    }
} 