using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using System.Text;

namespace BecomeCart;

/// <summary>
/// Utility class with debugging helpers for your mod
/// </summary>
public static class Debugging
{
    /// <summary>
    /// Gets the full hierarchical path of a GameObject
    /// </summary>
    /// <param name="obj">The GameObject to get the path for</param>
    /// <returns>A string representing the full path in the hierarchy</returns>
    public static string GetGameObjectPath(GameObject obj)
    {
        if (obj == null)
            return "null";
                
        StringBuilder sb = new StringBuilder();
        GetGameObjectPathRecursive(obj.transform, sb);
        return sb.ToString();
    }
    
    /// <summary>
    /// Recursive helper for building GameObject paths
    /// </summary>
    private static void GetGameObjectPathRecursive(Transform transform, StringBuilder sb)
    {
        if (transform.parent == null)
        {
            sb.Append(transform.name);
            return;
        }
        
        GetGameObjectPathRecursive(transform.parent, sb);
        sb.Append("/").Append(transform.name);
    }
} 