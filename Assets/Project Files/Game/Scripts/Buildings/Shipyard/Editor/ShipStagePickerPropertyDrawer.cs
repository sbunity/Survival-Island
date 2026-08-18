using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    [CustomPropertyDrawer(typeof(ShipStagePickerAttribute))]
    public class ShipStagePickerPropertyDrawer : UnityEditor.PropertyDrawer
    {
        private const string NONE_LABEL = "- none -";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.HelpBox(position, "Incorect property type!", MessageType.Error);

                EditorGUI.EndProperty();

                return;
            }

            ShipUpgradesDatabase database = EditorUtils.GetAsset<ShipUpgradesDatabase>();

            if (database == null || database.Stages.IsNullOrEmpty())
            {
                EditorGUI.HelpBox(position, "Ship Upgrades Database has no stages!", MessageType.Warning);

                EditorGUI.EndProperty();

                return;
            }

            ShipUpgradeStage[] stages = database.Stages;

            var options = new GUIContent[stages.Length + 1];
            options[0] = new GUIContent(NONE_LABEL);

            int selectedIndex = 0;

            for (int i = 0; i < stages.Length; i++)
            {
                ShipUpgradeStage stage = stages[i];

                string title = stage != null && !string.IsNullOrEmpty(stage.Title) ? stage.Title : "Stage";

                options[i + 1] = new GUIContent($"#{i + 1} - {title}");

                if (stage != null && stage.ID == property.stringValue)
                    selectedIndex = i + 1;
            }

            int newIndex = EditorGUI.Popup(position, label, selectedIndex, options);

            if (newIndex != selectedIndex)
                property.stringValue = newIndex == 0 ? string.Empty : stages[newIndex - 1].ID;

            EditorGUI.EndProperty();
        }
    }
}
