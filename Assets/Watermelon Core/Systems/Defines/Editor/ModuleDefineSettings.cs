using UnityEditorInternal;
using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "ModuleDefine", menuName = "Data/Core/Module Define Settings")]
    public class ModuleDefineSettings : ScriptableObject
    {
        [SerializeField] private string define;
        public string Define => define;

        [SerializeField] private string detectionType;
        public string DetectionType => detectionType;

        [SerializeField] private AssemblyDefinitionAsset moduleAsmdef;
        public AssemblyDefinitionAsset ModuleAsmdef => moduleAsmdef;

        // Optional file/DLL path to monitor for deletion (e.g. "GoogleMobileAds.Unity.dll").
        // When the file appears in deletedAssets the define is proactively disabled before
        // the next domain reload — same behaviour as the legacy RegisteredDefine.FilePath.
        [SerializeField] private string filePath;
        public string FilePath => filePath;

        // Defines whose asmdefs should be referenced by this module when active
        [SerializeField] private string[] optionalDependencies;
        public string[] OptionalDependencies => optionalDependencies;
    }
}
