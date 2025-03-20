using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BecomeCart;

/// <summary>
/// Utility class with debugging helpers for your mod
/// </summary>
public static class Debugging
{
    /// <summary>
    /// Dump all public methods of a class instance to the log
    /// </summary>
    public static void DumpMethods(object instance)
    {
        if (instance == null)
        {
            Plugin.Logger.LogError("Cannot dump methods: Instance is null");
            return;
        }

        Type type = instance.GetType();
        Plugin.Logger.LogInfo($"=== Methods for {type.Name} ===");
        
        MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        foreach (MethodInfo method in methods.OrderBy(m => m.Name))
        {
            string parameters = string.Join(", ", method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
            Plugin.Logger.LogInfo($"  {method.ReturnType.Name} {method.Name}({parameters})");
        }
    }
    
    /// <summary>
    /// Dump all fields of a class instance to the log
    /// </summary>
    public static void DumpFields(object instance)
    {
        if (instance == null)
        {
            Plugin.Logger.LogError("Cannot dump fields: Instance is null");
            return;
        }

        Type type = instance.GetType();
        Plugin.Logger.LogInfo($"=== Fields for {type.Name} ===");
        
        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        foreach (FieldInfo field in fields.OrderBy(f => f.Name))
        {
            try
            {
                object value = field.GetValue(instance);
                string valueStr = value == null ? "null" : value.ToString();
                if (valueStr.Length > 100) valueStr = valueStr.Substring(0, 100) + "...";
                Plugin.Logger.LogInfo($"  {field.FieldType.Name} {field.Name} = {valueStr}");
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogInfo($"  {field.FieldType.Name} {field.Name} = <ERROR: {ex.Message}>");
            }
        }
    }
    
    /// <summary>
    /// Find all instances of a specific component type in the current scene
    /// </summary>
    public static List<T> FindAllInstances<T>() where T : Component
    {
        List<T> results = new List<T>();
        T[] components = GameObject.FindObjectsOfType<T>();
        
        Plugin.Logger.LogInfo($"Found {components.Length} instances of {typeof(T).Name}");
        for (int i = 0; i < components.Length; i++)
        {
            results.Add(components[i]);
            string path = GetGameObjectPath(components[i].gameObject);
            Plugin.Logger.LogInfo($"  [{i}] {path}");
        }
        
        return results;
    }
    
    /// <summary>
    /// Get the full hierarchy path of a GameObject
    /// </summary>
    public static string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        
        return path;
    }
    
    /// <summary>
    /// Dump all the loaded assemblies and types relevant to the game
    /// Very useful for finding classes to patch
    /// </summary>
    public static void DumpAssemblies()
    {
        Plugin.Logger.LogInfo("=== Dumping loaded assemblies ===");
        
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (Assembly assembly in assemblies)
        {
            // Skip system assemblies
            if (assembly.FullName.StartsWith("System.") || 
                assembly.FullName.StartsWith("Microsoft.") || 
                assembly.FullName.StartsWith("mscorlib") ||
                assembly.FullName.StartsWith("netstandard"))
            {
                continue;
            }
            
            Plugin.Logger.LogInfo($"Assembly: {assembly.GetName().Name}");
            
            // Uncomment to log all types in each assembly (warning: very verbose!)
            /*
            Type[] types = assembly.GetTypes();
            foreach (Type type in types.OrderBy(t => t.Name))
            {
                Plugin.Logger.LogInfo($"  Type: {type.FullName}");
            }
            */
        }
    }
} 