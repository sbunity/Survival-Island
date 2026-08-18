using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    [CustomEditor(typeof(ShipStageVisualBinder))]
    public class ShipStageVisualBinderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var binder = (ShipStageVisualBinder)target;

            ShipUpgradesDatabase database = binder.UpgradesDatabase;
            if (database == null || database.Stages.IsNullOrEmpty())
                return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Preview In Scene", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Applies a stage's enable/disable lists right in the scene, so the models can be authored without entering play mode. This edits the scene - undo to revert.", MessageType.Info);

            ShipUpgradeStage[] stages = database.Stages;

            for (int i = 0; i < stages.Length; i++)
            {
                ShipUpgradeStage stage = stages[i];
                if (stage == null)
                    continue;

                string title = !string.IsNullOrEmpty(stage.Title) ? stage.Title : "Stage";

                if (GUILayout.Button($"Preview #{i + 1} - {title}"))
                    binder.EditorPreviewStage(stage.ID);
            }
        }
    }
}
