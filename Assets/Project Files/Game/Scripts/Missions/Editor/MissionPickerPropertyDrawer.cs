using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    [CustomPropertyDrawer(typeof(MissionPickerAttribute))]
    public class MissionPickerPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.HelpBox(position, "Incorect property type!", MessageType.Error);

                return;
            }

            string[] ids = MissionPickerSource.Ids;
            GUIContent[] options = MissionPickerSource.Options;

            if (ids.Length <= 1)
            {
                EditorGUI.HelpBox(position, "Missions can't be found, rebuild the catalog: Tools/Missions/Rebuild Missions Catalog", MessageType.Warning);

                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            int selectedIndex = System.Array.IndexOf(ids, property.stringValue);

            if (selectedIndex < 0)
            {
                GUIContent[] extendedOptions = new GUIContent[options.Length + 1];
                System.Array.Copy(options, extendedOptions, options.Length);
                extendedOptions[options.Length] = new GUIContent(string.Format("Missing ({0})", property.stringValue));

                options = extendedOptions;
                selectedIndex = options.Length - 1;
            }

            bool hasLabel = label != null && (!string.IsNullOrEmpty(label.text) || label.image != null);

            int newIndex = hasLabel
                ? EditorGUI.Popup(position, label, selectedIndex, options)
                : EditorGUI.Popup(position, selectedIndex, options);

            if (newIndex != selectedIndex && newIndex >= 0 && newIndex < ids.Length)
                property.stringValue = ids[newIndex];

            EditorGUI.EndProperty();
        }
    }
}
