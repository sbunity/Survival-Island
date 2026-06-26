using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    public class DefinePostprocessor : AssetPostprocessor
    {
        private const string PREFS_KEY = "DefinesCheck";

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void AssemblyReload()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || string.IsNullOrEmpty(CoreEditor.FOLDER_CORE))
            {
                EditorApplication.delayCall += AssemblyReload;
                return;
            }

            EditorApplication.delayCall += () =>
            {
                DefineManager.RebuildCache();
                DefineManager.CheckAutoDefines();
            };
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            HandleDeletedModuleDefines(deletedAssets);

            ValidateRequirement(importedAssets, deletedAssets);

            if (EditorApplication.isCompiling || EditorApplication.isUpdating || string.IsNullOrEmpty(CoreEditor.FOLDER_CORE))
            {
                EditorApplication.delayCall += () => OnPostprocessAllAssets(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths, didDomainReload);
                return;
            }

            if (EditorPrefs.GetBool(PREFS_KEY, false))
            {
                DefineManager.CheckAutoDefines(deletedAssets);
                EditorPrefs.SetBool(PREFS_KEY, false);
            }
        }

        // When a ModuleDefine asset is deleted, proactively remove its asmdef from any
        // module that declared it as an optionalDependency — before Unity recompiles.
        private static void HandleDeletedModuleDefines(string[] deletedAssets)
        {
            if (deletedAssets.IsNullOrEmpty()) return;

            ModuleDefineCache cache = ModuleDefineCache.Load();
            bool cacheChanged = false;

            foreach (string path in deletedAssets)
            {
                if (!path.EndsWith(".asset")) continue;

                ModuleDefineCache.Entry deletedEntry = cache.FindByPath(path);
                if (deletedEntry == null) continue;

                // Find all modules that reference the deleted module as an optional dependency
                AssetDatabase.StartAssetEditing();
                try
                {
                    foreach (ModuleDefineCache.Entry entry in cache.Entries)
                    {
                        if (entry == deletedEntry) continue;
                        if (!entry.optionalDependencies.Contains(deletedEntry.define)) continue;

                        AsmdefPatcher.Patch(entry.moduleAsmdefGuid, deletedEntry.moduleAsmdefGuid, false);
                    }
                }
                finally
                {
                    AssetDatabase.StopAssetEditing();
                }

                DefineManager.DisableDefine(deletedEntry.define);
                cache.Remove(deletedEntry);
                cacheChanged = true;

                Debug.Log($"[Define Manager]: Module '{deletedEntry.define}' removed. Unlinked from dependents.");
            }

            if (cacheChanged)
                cache.Save();
        }

        private static void ValidateRequirement(string[] importedAssets, string[] deletedAssets)
        {
            if (!importedAssets.IsNullOrEmpty())
            {
                foreach (string str in importedAssets)
                {
                    if (str.EndsWith(".cs") || str.EndsWith(".dll"))
                    {
                        EditorPrefs.SetBool(PREFS_KEY, true);
                        return;
                    }
                }
            }

            if (!deletedAssets.IsNullOrEmpty())
            {
                foreach (string str in deletedAssets)
                {
                    if (str.EndsWith(".cs") || str.EndsWith(".dll"))
                    {
                        EditorPrefs.SetBool(PREFS_KEY, true);
                        return;
                    }
                }
            }
        }
    }
}
