using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace BecomeCart
{
    public partial class Plugin
    {
        private bool _isControllingCart = false;
        private float _cartMoveSpeed = 5f;
        private float _cartRotateSpeed = 100f;
        private Vector3 _lastCartMoveDirection = Vector3.zero;
        private Rigidbody _cartRigidbody;
        private Component _physGrabCart;
        private float _mouseSensitivity = 2.0f; // Mouse turning sensitivity

        /// <summary>
        /// Updates cart control with WASD input when player is swapped with cart
        /// </summary>
        private void UpdateCartControl()
        {
            if (!_isControllingCart || _lastPlayerSwap == null || _lastPlayerSwap.CartObject == null) return;

            try
            {
                // Calculate movement direction from input
                float horizontal = Input.GetAxis("Horizontal"); // For strafing left/right
                float vertical = Input.GetAxis("Vertical"); // For forward/backward movement
                
                // Get mouse input for turning
                float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity;
                
                // Get camera for reference
                Camera mainCamera = Camera.main;
                if (mainCamera == null) return;
                
                // Get camera's forward and right directions for movement relative to camera
                Vector3 cameraForward = mainCamera.transform.forward;
                Vector3 cameraRight = mainCamera.transform.right;
                
                // Project camera directions onto XZ plane (ignore Y component for flat movement)
                cameraForward.y = 0;
                cameraRight.y = 0;
                cameraForward.Normalize();
                cameraRight.Normalize();
                
                if (_cartRigidbody != null)
                {
                    // Apply mouse turning (rotate the cart based on mouse X)
                    if (Mathf.Abs(mouseX) > 0.1f)
                    {
                        Quaternion rotation = Quaternion.Euler(0, mouseX, 0);
                        _cartRigidbody.MoveRotation(_cartRigidbody.rotation * rotation);
                        
                        // Update forward direction based on new rotation
                        cameraForward = _cartRigidbody.transform.forward;
                        cameraForward.y = 0;
                        cameraForward.Normalize();
                        
                        // Update right direction based on new rotation
                        cameraRight = _cartRigidbody.transform.right;
                        cameraRight.y = 0;
                        cameraRight.Normalize();
                    }
                    
                    // Handle movement input
                    if (Mathf.Abs(vertical) > 0.1f)
                    {
                        // Forward/backward movement (along cart's forward direction)
                        Vector3 moveDirection = cameraForward * vertical;
                        _cartRigidbody.AddForce(moveDirection * (_cartMoveSpeed * 0.75f), ForceMode.Acceleration); // Reduced by 25%
                        _lastCartMoveDirection = moveDirection;
                    }
                    
                    // Handle strafing left/right (A/D keys)
                    if (Mathf.Abs(horizontal) > 0.1f)
                    {
                        // Strafe left/right (along cart's right direction)
                        Vector3 strafeDirection = cameraRight * horizontal;
                        _cartRigidbody.AddForce(strafeDirection * (_cartMoveSpeed * 0.75f * 0.8f), ForceMode.Acceleration); // Further reduced for strafing
                    }
                }
                
                // Check for cart-specific functionality
                if (_physGrabCart != null)
                {
                    // Try to unlock the cart if it has a state system
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        // Try to access and change cart state
                        Type cartType = _physGrabCart.GetType();
                        PropertyInfo stateProperty = cartType.GetProperty("currentState");
                        
                        if (stateProperty != null)
                        {
                            // Assuming "Unlocked" is a valid state
                            object unlocked = Enum.Parse(stateProperty.PropertyType, "Unlocked");
                            stateProperty.SetValue(_physGrabCart, unlocked);
                            Logger.LogInfo("Changed cart state to Unlocked");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error during cart control update: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Initializes cart control by setting up necessary references
        /// </summary>
        private void InitializeCartControl(GameObject cartObject)
        {
            if (cartObject == null) return;
            
            try
            {
                Logger.LogInfo("Initializing cart control system");
                
                // Get cart rigidbody
                _cartRigidbody = cartObject.GetComponent<Rigidbody>();
                if (_cartRigidbody == null)
                {
                    Logger.LogError("Cart doesn't have a Rigidbody component!");
                    return;
                }
                
                // Set initial values
                _isControllingCart = true;
                _lastCartMoveDirection = cartObject.transform.forward;
                
                // Get PhysGrabCart component if it exists
                Component[] components = cartObject.GetComponents<Component>();
                foreach (Component comp in components)
                {
                    if (comp != null && comp.GetType().Name.Contains("PhysGrabCart"))
                    {
                        _physGrabCart = comp;
                        Logger.LogInfo($"Found PhysGrabCart component: {comp.GetType().Name}");
                        break;
                    }
                }
                
                // Set up physics properties for better control
                if (_cartRigidbody != null)
                {
                    // Store original values in the swap data
                    if (_lastPlayerSwap != null)
                    {
                        _lastPlayerSwap.CartWasKinematic = _cartRigidbody.isKinematic;
                    }
                    
                    // Configure rigidbody for controlled movement
                    _cartRigidbody.isKinematic = false;
                    _cartRigidbody.angularDrag = 10; // Higher angular drag for stability
                    
                    // Make cart more responsive to input but 25% slower
                    _cartMoveSpeed = 7.5f; // Original was 10f, reduced by 25%
                    _cartRotateSpeed = 120f;
                    
                    Logger.LogInfo("Cart control initialized successfully");
                    Logger.LogInfo("Use mouse to turn the cart, WASD to move (A/D now strafe left/right)");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error initializing cart control: {ex.Message}\n{ex.StackTrace}");
                _isControllingCart = false;
            }
        }

        /// <summary>
        /// Stops cart control and resets properties
        /// </summary>
        private void StopCartControl()
        {
            if (_cartRigidbody != null && _lastPlayerSwap != null)
            {
                // Restore original kinematic state
                _cartRigidbody.isKinematic = _lastPlayerSwap.CartWasKinematic;
            }
            
            // Reset control variables
            _isControllingCart = false;
            _cartRigidbody = null;
            _physGrabCart = null;
            _lastCartMoveDirection = Vector3.zero;
            
            Logger.LogInfo("Cart control stopped");
        }
    }
} 