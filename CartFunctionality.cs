using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Logging;

namespace BecomeCart
{
    public partial class Plugin
    {
        private GameObject _temporaryFloorCollider;
        private Dictionary<string, bool> _playerVisibilityState = new Dictionary<string, bool>();
        private Dictionary<string, bool> _savedCartCollisionSettings = new Dictionary<string, bool>();
        private bool _debugShowPlayerModel = false;
        private bool _playerDamageImmune = true;
        private Component _playerDamageHandler = null;
        private PlayerCartSwap _lastPlayerSwap;
        private GameObject _lastFoundCart;

        /// <summary>
        /// Represents data needed to restore the player after cart swap
        /// </summary>
        private class PlayerCartSwap
        {
            public GameObject PlayerObject;
            public GameObject CartObject;
            public Vector3 OriginalPlayerPosition;
            public Quaternion OriginalPlayerRotation;
            public Transform OriginalPlayerParent;
            public Component PlayerController;
            public Camera OriginalCamera;
            public Transform OriginalCameraParent;
            public Vector3 OriginalCameraLocalPosition;
            public Quaternion OriginalCameraLocalRotation;
            public bool CartWasKinematic;
            public Component CartPhysGrabObject;
            public Component CartPhysGrabCart;
            public List<Component> PlayerComponents;
            public List<MonoBehaviour> DisabledMonoBehaviours;
            public bool PlayerHadRigidbody;
            public bool PlayerRbWasKinematic;
            public Component PlayerDamageHandler;
            public int OriginalPlayerLayer;
        }

        /// <summary>
        /// Coroutine to handle cart control and movement
        /// </summary>
        private IEnumerator CartCoroutineControl()
        {
            if (_lastPlayerSwap == null || _lastPlayerSwap.CartObject == null)
                yield break;
            
            Logger.LogInfo("Starting cart control update coroutine");
            
            GameObject cartObject = _lastPlayerSwap.CartObject;
            Rigidbody cartRb = cartObject.GetComponent<Rigidbody>();
            
            if (cartRb == null)
            {
                Logger.LogError("Cart has no Rigidbody component, cannot control");
                yield break;
            }
            
            // Configure rigidbody for better control
            cartRb.mass = 2.0f;
            cartRb.drag = 1.0f;
            cartRb.angularDrag = 5.0f;
            cartRb.interpolation = RigidbodyInterpolation.Interpolate;
            cartRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            
            // Movement parameters
            float forwardSpeed = 11.25f;  // 15.0f reduced by 25%
            float reverseSpeed = 5.25f;   // 7.0f reduced by 25%
            float turnSpeed = 100.0f;
            float strafeSpeed = 9.0f;     // New parameter for strafing
            float stabilizationForce = 5.0f;
            
            // Main control loop
            while (_lastPlayerSwap != null && cartObject != null)
            {
                // Get input from WASD or arrow keys
                float horizontalInput = Input.GetAxis("Horizontal"); // A/D for strafing left/right
                float verticalInput = Input.GetAxis("Vertical");     // W/S for forward/backward
                float mouseX = Input.GetAxis("Mouse X") * 2.0f;      // Mouse for turning
                
                if (Mathf.Abs(verticalInput) > 0.1f)
                {
                    Logger.LogDebug($"Cart input: V={verticalInput:F2}");
                    
                    // Get the cart's forward direction
                    Vector3 forwardDirection = cartObject.transform.forward;
                    
                    // Calculate speed based on direction (slower in reverse)
                    float currentSpeed = verticalInput >= 0 ? forwardSpeed : reverseSpeed;
                    
                    // Apply forward/backward movement force
                    Vector3 moveForce = forwardDirection * verticalInput * currentSpeed;
                    cartRb.AddForce(moveForce, ForceMode.Acceleration);
                }
                
                // Handle mouse turning
                if (Mathf.Abs(mouseX) > 0.1f)
                {
                    cartObject.transform.Rotate(0, mouseX, 0);
                }
                
                // Handle strafing with A/D keys
                if (Mathf.Abs(horizontalInput) > 0.1f)
                {
                    Logger.LogDebug($"Cart strafe: H={horizontalInput:F2}");
                    
                    // Get the cart's right direction for strafing
                    Vector3 rightDirection = cartObject.transform.right;
                    
                    // Apply strafing force
                    Vector3 strafeForce = rightDirection * horizontalInput * strafeSpeed;
                    cartRb.AddForce(strafeForce, ForceMode.Acceleration);
                }
                
                // Stabilization: Apply upward force to prevent tipping
                Vector3 cartUp = cartObject.transform.up;
                Vector3 worldUp = Vector3.up;
                
                // If the cart is tipping, apply corrective torque
                if (Vector3.Dot(cartUp, worldUp) < 0.95f)
                {
                    Vector3 rightAxis = Vector3.Cross(cartUp, worldUp);
                    float correctionAngle = Vector3.Angle(cartUp, worldUp);
                    Vector3 correctionTorque = rightAxis.normalized * correctionAngle * stabilizationForce;
                    
                    cartRb.AddTorque(correctionTorque, ForceMode.Acceleration);
                    Logger.LogDebug($"Stabilizing cart, correction: {correctionTorque.magnitude:F2}");
                }
                
                // Add slight drag to slow down when not accelerating
                if (Mathf.Abs(verticalInput) < 0.1f && cartRb.velocity.magnitude > 0.5f)
                {
                    Vector3 dragForce = -cartRb.velocity.normalized * cartRb.mass * 0.5f;
                    cartRb.AddForce(dragForce, ForceMode.Acceleration);
                }
                
                // Wait for next frame
                yield return null;
            }
            
            Logger.LogInfo("Cart control update coroutine ended");
        }
    }
} 