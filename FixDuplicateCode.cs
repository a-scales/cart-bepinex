using UnityEngine;
using System;
using System.Collections.Generic;

namespace BecomeCart
{
    /// <summary>
    /// Utility class to check component existence by name or type
    /// </summary>
    public static class ComponentUtility
    {
        /// <summary>
        /// Checks if a GameObject has a component with the given name
        /// </summary>
        public static bool HasComponentByName(GameObject obj, string componentName)
        {
            if (obj == null) return false;
            
            Component[] components = obj.GetComponents<Component>();
            foreach (Component comp in components)
            {
                if (comp != null && comp.GetType().Name.Contains(componentName))
                {
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// Gets a component of the specified name from a GameObject
        /// </summary>
        public static Component GetComponentByName(GameObject obj, string componentName)
        {
            if (obj == null) return null;
            
            Component[] components = obj.GetComponents<Component>();
            foreach (Component comp in components)
            {
                if (comp != null && comp.GetType().Name.Contains(componentName))
                {
                    return comp;
                }
            }
            return null;
        }
        
        /// <summary>
        /// Gets all components of the specified name from a GameObject
        /// </summary>
        public static List<Component> GetComponentsByName(GameObject obj, string componentName)
        {
            List<Component> result = new List<Component>();
            if (obj == null) return result;
            
            Component[] components = obj.GetComponents<Component>();
            foreach (Component comp in components)
            {
                if (comp != null && comp.GetType().Name.Contains(componentName))
                {
                    result.Add(comp);
                }
            }
            return result;
        }
    }
} 