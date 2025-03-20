using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace BecomeCart;

/// <summary>
/// Tracks cart objects to help identify the correct game object
/// and understand its behavior
/// </summary>
public class CartTracker : MonoBehaviour
{
    private class TrackedObject
    {
        public GameObject GameObject;
        public string Path;
        public Vector3 LastPosition;
        public Vector3 LastRotation;
        public float LastMovementTime;
        public float TotalDistanceMoved;
        public bool IsMoving;
        
        public TrackedObject(GameObject obj)
        {
            GameObject = obj;
            Path = Debugging.GetGameObjectPath(obj);
            LastPosition = obj.transform.position;
            LastRotation = obj.transform.rotation.eulerAngles;
            LastMovementTime = Time.time;
            TotalDistanceMoved = 0f;
            IsMoving = false;
        }
    }
    
    private static CartTracker _instance;
    private Dictionary<int, TrackedObject> _trackedObjects = new Dictionary<int, TrackedObject>();
    private float _reportInterval = 5.0f; // Report every 5 seconds
    private float _lastReportTime = 0f;
    private float _movementThreshold = 0.05f; // Minimum movement to register as "moving"
    
    public static CartTracker Instance 
    {
        get 
        {
            if (_instance == null)
            {
                // Create a new GameObject to host our tracker
                GameObject trackerObject = new GameObject("CartTracker");
                DontDestroyOnLoad(trackerObject);
                _instance = trackerObject.AddComponent<CartTracker>();
            }
            return _instance;
        }
    }
    
    public void TrackObject(GameObject obj)
    {
        if (obj == null) return;
        
        int id = obj.GetInstanceID();
        if (!_trackedObjects.ContainsKey(id))
        {
            _trackedObjects.Add(id, new TrackedObject(obj));
            Plugin.Logger.LogInfo($"Started tracking potential cart: {Debugging.GetGameObjectPath(obj)}");
        }
    }
    
    public void StopTrackingObject(GameObject obj)
    {
        if (obj == null) return;
        
        int id = obj.GetInstanceID();
        if (_trackedObjects.ContainsKey(id))
        {
            _trackedObjects.Remove(id);
        }
    }
    
    public void StopTrackingAll()
    {
        _trackedObjects.Clear();
        Plugin.Logger.LogInfo("Stopped tracking all potential cart objects");
    }
    
    private void Update()
    {
        // Update tracking for all objects
        List<int> destroyedObjects = new List<int>();
        
        foreach (var kvp in _trackedObjects)
        {
            int id = kvp.Key;
            TrackedObject tracked = kvp.Value;
            
            if (tracked.GameObject == null)
            {
                destroyedObjects.Add(id);
                continue;
            }
            
            // Check for movement
            Vector3 currentPos = tracked.GameObject.transform.position;
            Vector3 currentRot = tracked.GameObject.transform.rotation.eulerAngles;
            
            float distance = Vector3.Distance(currentPos, tracked.LastPosition);
            if (distance > _movementThreshold)
            {
                tracked.IsMoving = true;
                tracked.LastMovementTime = Time.time;
                tracked.TotalDistanceMoved += distance;
            }
            else
            {
                // Only set to not moving if it's been still for a short time
                if (Time.time - tracked.LastMovementTime > 0.5f)
                {
                    tracked.IsMoving = false;
                }
            }
            
            // Update last position
            tracked.LastPosition = currentPos;
            tracked.LastRotation = currentRot;
        }
        
        // Remove destroyed objects
        foreach (int id in destroyedObjects)
        {
            _trackedObjects.Remove(id);
        }
        
        // Report on tracked objects periodically
        if (Time.time - _lastReportTime > _reportInterval && _trackedObjects.Count > 0)
        {
            ReportMovingObjects();
            _lastReportTime = Time.time;
        }
    }
    
    private void ReportMovingObjects()
    {
        bool foundMovingObjects = false;
        
        foreach (var tracked in _trackedObjects.Values)
        {
            if (tracked.IsMoving || tracked.TotalDistanceMoved > 1.0f)
            {
                if (!foundMovingObjects)
                {
                    Plugin.Logger.LogInfo("=== MOVING CART CANDIDATES ===");
                    foundMovingObjects = true;
                }
                
                Plugin.Logger.LogInfo($"ACTIVE CART CANDIDATE: {tracked.Path}");
                Plugin.Logger.LogInfo($"  Current Position: {tracked.LastPosition}");
                Plugin.Logger.LogInfo($"  Total Distance: {tracked.TotalDistanceMoved:F2}m");
                Plugin.Logger.LogInfo($"  Currently Moving: {tracked.IsMoving}");
                
                // This is very likely our cart if it's moving!
                if (tracked.IsMoving)
                {
                    Plugin.Logger.LogInfo("  *** THIS IS LIKELY THE CART YOU'RE LOOKING FOR ***");
                    
                    // Extra logging for active cart objects
                    LogCartComponents(tracked.GameObject);
                }
            }
        }
        
        if (!foundMovingObjects)
        {
            Plugin.Logger.LogInfo("No moving cart candidates found");
        }
    }
    
    private void LogCartComponents(GameObject obj)
    {
        Component[] components = obj.GetComponents<Component>();
        Plugin.Logger.LogInfo($"Active cart has {components.Length} components:");
        
        foreach (Component component in components)
        {
            if (component == null) continue;
            
            string typeName = component.GetType().Name;
            Plugin.Logger.LogInfo($"  Component: {typeName}");
            
            // Extra logging for interesting components
            if (typeName.Contains("Controller") || 
                typeName.Contains("Movement") || 
                typeName.Contains("Motor") ||
                typeName.Contains("Drive") ||
                typeName.Contains("Vehicle"))
            {
                Plugin.Logger.LogInfo($"  *** IMPORTANT COMPONENT: {typeName} ***");
                // Log its fields
                Debugging.DumpFields(component);
                
                // Register this component for potential patching
                CartPatch.RegisterCartComponent(component, obj);
            }
        }
    }
} 