using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    /// <summary>Shared editor utilities for RewardSetEditor and RewardsHolderEditor.</summary>
    internal static class RewardsEditorUtils
    {
        private static List<Type> cachedRewardTypes;

        /// <summary>Returns all concrete non-abstract Reward subclasses across all loaded assemblies, sorted by name.</summary>
        internal static List<Type> GetAllRewardTypes()
        {
            if (cachedRewardTypes != null)
                return cachedRewardTypes;

            cachedRewardTypes = new List<Type>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly == null) continue;

                Type[] types;
                try { types = assembly.GetTypes(); }
                catch (ReflectionTypeLoadException e) { types = e.Types.Where(t => t != null).ToArray(); }

                foreach (Type t in types)
                {
                    if (t != null && t.IsClass && !t.IsAbstract && typeof(Reward).IsAssignableFrom(t))
                        cachedRewardTypes.Add(t);
                }
            }

            cachedRewardTypes = cachedRewardTypes.OrderBy(t => t.Name).ToList();
            return cachedRewardTypes;
        }

        /// <summary>Invalidates the cached type list so it is rebuilt on next access.</summary>
        internal static void InvalidateCache() => cachedRewardTypes = null;

        /// <summary>Resolves the CLR Type from a SerializedProperty's managedReferenceFullTypename ("AssemblyName TypeFullName").</summary>
        internal static Type GetManagedReferenceSystemType(SerializedProperty prop)
        {
            if (prop == null) return null;

            string full = prop.managedReferenceFullTypename;
            if (string.IsNullOrEmpty(full)) return null;

            int space = full.IndexOf(' ');
            if (space < 0 || space + 1 >= full.Length) return null;

            string asmName = full.Substring(0, space);
            string typeName = full.Substring(space + 1);

            var asm = AppDomain.CurrentDomain
                               .GetAssemblies()
                               .FirstOrDefault(a => a.GetName().Name == asmName);
            if (asm != null)
            {
                var t = asm.GetType(typeName);
                if (t != null) return t;
            }

            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = a.GetType(typeName);
                if (t != null) return t;
            }

            return Type.GetType($"{typeName}, {asmName}");
        }

        /// <summary>Draws children of a managed-reference SerializedProperty without the root foldout.</summary>
        internal static void DrawManagedReferenceContents(SerializedProperty element)
        {
            if (element == null) return;

            var copy = element.Copy();
            var end = copy.GetEndProperty();

            bool enterChildren = true;

            while (copy.NextVisible(enterChildren) && !SerializedProperty.EqualContents(copy, end))
            {
                if (copy.name == "m_Script")
                {
                    enterChildren = false;
                    continue;
                }

                EditorGUILayout.PropertyField(copy, includeChildren: true);
                enterChildren = false;
            }
        }

        /// <summary>Returns a human-readable type name from a managedReferenceFullTypename string.</summary>
        internal static string GetNiceTypeName(string managedReferenceFullTypename)
        {
            if (string.IsNullOrEmpty(managedReferenceFullTypename)) return "(null)";

            int space = managedReferenceFullTypename.IndexOf(' ');
            if (space >= 0 && space + 1 < managedReferenceFullTypename.Length)
            {
                string full = managedReferenceFullTypename.Substring(space + 1);
                int lastDot = full.LastIndexOf('.');
                return lastDot >= 0 ? full.Substring(lastDot + 1).AddSpaces() : full;
            }

            return managedReferenceFullTypename;
        }
    }
}
