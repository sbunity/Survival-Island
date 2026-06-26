using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Linq;

#if UNITY_6000_0_OR_NEWER
using UnityEditor.Build;
#endif

namespace Watermelon
{
    public static class DefineManager
    {
        public static bool HasDefine(string define)
        {
#if UNITY_6000_0_OR_NEWER
            string definesLine = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget)));
#else
            string definesLine = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
#endif
            return Array.FindIndex(definesLine.Split(';'), x => x == define) != -1;
        }

        public static void EnableDefine(string define)
        {
#if UNITY_6000_0_OR_NEWER
            string defineLine = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget)));
#else
            string defineLine = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
#endif
            if (Array.FindIndex(defineLine.Split(';'), x => x == define) != -1)
                return;

            defineLine = defineLine.Insert(0, define + ";");

#if UNITY_6000_0_OR_NEWER
            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget)), defineLine);
#else
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget), defineLine);
#endif
        }

        public static void DisableDefine(string define)
        {
#if UNITY_6000_0_OR_NEWER
            string defineLine = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget)));
#else
            string defineLine = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
#endif
            string[] splitedDefines = defineLine.Split(';');
            int tempDefineIndex = Array.FindIndex(splitedDefines, x => x == define);

            if (tempDefineIndex == -1)
                return;

            string tempDefineLine = "";
            for (int i = 0; i < splitedDefines.Length; i++)
            {
                if (i != tempDefineIndex && !string.IsNullOrEmpty(splitedDefines[i]))
                {
                    if (!string.IsNullOrEmpty(tempDefineLine))
                        tempDefineLine += ";";

                    tempDefineLine += splitedDefines[i];
                }
            }

            if (defineLine != tempDefineLine)
            {
#if UNITY_6000_0_OR_NEWER
                PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget)), tempDefineLine);
#else
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget), tempDefineLine);
#endif
            }
        }

        public static void CheckAutoDefines(string[] deletedAssets = null)
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || string.IsNullOrEmpty(CoreEditor.FOLDER_CORE))
            {
                EditorApplication.delayCall += () => { CheckAutoDefines(deletedAssets); };
                return;
            }

            bool CheckDeletedAssets(string filePath)
            {
                if (!string.IsNullOrEmpty(filePath) && !deletedAssets.IsNullOrEmpty())
                {
                    foreach (string deletedAsset in deletedAssets)
                    {
                        if (deletedAsset.EndsWith(filePath, StringComparison.OrdinalIgnoreCase))
                            return true;
                    }
                }
                return false;
            }

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            List<DefineState> markedDefines = new List<DefineState>();
            List<RegisteredDefine> registeredDefines = GetDynamicDefines();

            foreach (RegisteredDefine registeredDefine in registeredDefines)
            {
                if (CheckDeletedAssets(registeredDefine.FilePath))
                {
                    markedDefines.Add(new DefineState(registeredDefine.Define, false));
                    continue;
                }

                bool defineFound = false;
                foreach (Assembly assembly in assemblies)
                {
                    if (assembly.GetType(registeredDefine.AssemblyType, false) != null)
                    {
                        defineFound = true;
                        markedDefines.Add(new DefineState(registeredDefine.Define, true));
                        break;
                    }
                }

                if (!defineFound)
                    markedDefines.Add(new DefineState(registeredDefine.Define, false));
            }

            ChangeAutoDefinesState(markedDefines);
        }

        public static void ChangeAutoDefinesState(List<DefineState> defineStates)
        {
            if (EditorApplication.isCompiling || defineStates.IsNullOrEmpty())
                return;

            bool definesUpdated = false;

            StringBuilder sb = new StringBuilder();
            sb.Append("[Define Manager]: Dependencies change is detected. Updating Scripting Define Symbols..");
            sb.AppendLine();

            DefineString definesString = new DefineString();
            foreach (DefineState defineState in defineStates)
            {
                if (defineState.State)
                {
                    if (!definesString.HasDefine(defineState.Define))
                    {
                        definesUpdated = true;
                        definesString.AddDefine(defineState.Define);
                        sb.AppendLine();
                        sb.Append(defineState.Define);
                        sb.Append(" - added");
                    }
                }
                else
                {
                    if (definesString.HasDefine(defineState.Define))
                    {
                        definesUpdated = true;
                        definesString.RemoveDefine(defineState.Define);
                        sb.AppendLine();
                        sb.Append(defineState.Define);
                        sb.Append(" - removed");
                    }
                }
            }
            sb.AppendLine();

            if (definesUpdated)
                Debug.Log(sb.ToString());

            definesString.ApplyDefines();

            ReconcileOptionalDependencies(definesString);
        }

        // For each active module, ensures its optional dependency asmdefs are linked/unlinked
        // based on whether those dependency modules are also active.
        private static void ReconcileOptionalDependencies(DefineString definesString)
        {
            ModuleDefineCache cache = ModuleDefineCache.Load();
            if (cache.Entries.IsNullOrEmpty()) return;

            AssetDatabase.StartAssetEditing();

            try
            {
                foreach (ModuleDefineCache.Entry entry in cache.Entries)
                {
                    if (string.IsNullOrEmpty(entry.moduleAsmdefGuid) || entry.optionalDependencies.IsNullOrEmpty())
                        continue;

                    // Module is "active" for linking purposes if its asmdef file exists on disk,
                    // not whether its high-level define is set. The define may be off (e.g.
                    // MODULE_IAP without Unity IAP SDK) while the asmdef is still present and
                    // needs its optional references managed.
                    string asmdefPath = AssetDatabase.GUIDToAssetPath(entry.moduleAsmdefGuid);
                    bool moduleAsmdefExists = !string.IsNullOrEmpty(asmdefPath) && File.Exists(asmdefPath);

                    foreach (string depDefine in entry.optionalDependencies)
                    {
                        ModuleDefineCache.Entry depEntry = cache.FindByDefine(depDefine);
                        if (depEntry == null || string.IsNullOrEmpty(depEntry.moduleAsmdefGuid))
                            continue;

                        bool shouldLink = moduleAsmdefExists && definesString.HasDefine(depDefine);
                        AsmdefPatcher.Patch(entry.moduleAsmdefGuid, depEntry.moduleAsmdefGuid, shouldLink);
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }

        public static void RebuildCache()
        {
            ModuleDefineSettings[] allSettings = GetModuleDefineSettings();
            ModuleDefineCache cache = new ModuleDefineCache();
            cache.Rebuild(allSettings);
            cache.Save();
        }

        public static ModuleDefineSettings[] GetModuleDefineSettings()
        {
            string[] guids = AssetDatabase.FindAssets("t:ModuleDefineSettings");
            var result = new List<ModuleDefineSettings>();

            foreach (string guid in guids)
            {
                ModuleDefineSettings settings = AssetDatabase.LoadAssetAtPath<ModuleDefineSettings>(
                    AssetDatabase.GUIDToAssetPath(guid));

                if (settings != null)
                    result.Add(settings);
            }

            return result.ToArray();
        }

        public static List<RegisteredDefine> GetDynamicDefines()
        {
            List<Type> gameTypes = new List<Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                if (assembly == null) continue;
                try
                {
                    Type[] tempTypes = assembly.GetTypes()
                        .Where(m => m.IsDefined(typeof(DefineAttribute), true))
                        .ToArray();

                    if (!tempTypes.IsNullOrEmpty())
                        gameTypes.AddRange(tempTypes);
                }
                catch (ReflectionTypeLoadException e)
                {
                    Debug.LogException(e);
                }
            }

            List<RegisteredDefine> registeredDefines = new List<RegisteredDefine>();
            registeredDefines.AddRange(DefineSettings.STATIC_REGISTERED_DEFINES);

            foreach (Type type in gameTypes)
            {
                DefineAttribute[] defineAttributes = (DefineAttribute[])Attribute.GetCustomAttributes(type, typeof(DefineAttribute));
                for (int i = 0; i < defineAttributes.Length; i++)
                {
                    if (!string.IsNullOrEmpty(defineAttributes[i].AssemblyType))
                    {
                        if (registeredDefines.FindIndex(x => x.Define == defineAttributes[i].Define) == -1)
                            registeredDefines.Add(new RegisteredDefine(defineAttributes[i]));
                    }
                }
            }

            foreach (ModuleDefineSettings settings in GetModuleDefineSettings())
            {
                if (string.IsNullOrEmpty(settings.DetectionType)) continue;
                if (registeredDefines.FindIndex(x => x.Define == settings.Define) == -1)
                    registeredDefines.Add(new RegisteredDefine(settings.Define, settings.DetectionType, settings.FilePath));
            }

            return registeredDefines;
        }
    }
}
