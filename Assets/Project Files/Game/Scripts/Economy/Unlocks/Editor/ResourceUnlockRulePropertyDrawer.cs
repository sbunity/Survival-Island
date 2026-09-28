using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    [CustomPropertyDrawer(typeof(ResourceUnlockRule))]
    public class ResourceUnlockRulePropertyDrawer : PropertyDrawer
    {
        private const float SPACING = 4f;
        private const float MODE_MAX_WIDTH = 120f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty currencyProperty = property.FindPropertyRelative("currency");
            SerializedProperty modeProperty = property.FindPropertyRelative("mode");
            SerializedProperty missionProperty = property.FindPropertyRelative("missionId");

            EditorGUI.BeginProperty(position, label, property);

            bool requiresMission = modeProperty.enumValueIndex == (int)ResourceUnlockRule.UnlockMode.AfterMission;

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float modeWidth = Mathf.Min(MODE_MAX_WIDTH, position.width * 0.3f);
            float restWidth = position.width - modeWidth - SPACING;
            float currencyWidth = requiresMission ? restWidth * 0.38f : restWidth;

            Rect currencyRect = new Rect(position.x, position.y, currencyWidth, lineHeight);
            Rect modeRect = new Rect(currencyRect.xMax + SPACING, position.y, modeWidth, lineHeight);

            EditorGUI.PropertyField(currencyRect, currencyProperty, GUIContent.none);
            EditorGUI.PropertyField(modeRect, modeProperty, GUIContent.none);

            if (requiresMission)
            {
                Rect missionRect = new Rect(modeRect.xMax + SPACING, position.y, position.xMax - modeRect.xMax - SPACING, lineHeight);

                EditorGUI.PropertyField(missionRect, missionProperty, GUIContent.none);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}
