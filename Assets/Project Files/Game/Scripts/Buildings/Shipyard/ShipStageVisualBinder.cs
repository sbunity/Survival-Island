using UnityEngine;

namespace Watermelon
{
    public class ShipStageVisualBinder : MonoBehaviour, IWorldElement
    {
        public int InitialisationOrder => 3;

        public BaseWorldBehavior LinkedWorldBehavior { get; set; }

        [SerializeField] ShipUpgradesDatabase upgradesDatabase;
        public ShipUpgradesDatabase UpgradesDatabase => upgradesDatabase;

        [Space]
        [SerializeField] StageVisuals[] bindings;

        private bool isApplied;

        public void OnWorldLoaded()
        {
            ApplyCompletedStages(true);
        }

        public void OnWorldUnloaded()
        {
            isApplied = false;
        }

        public void ApplyCompletedStages(bool immediately)
        {
            if (isApplied || upgradesDatabase == null || upgradesDatabase.Stages.IsNullOrEmpty())
                return;

            isApplied = true;

            var stages = upgradesDatabase.Stages;
            var hasChanges = false;

            for (var i = 0; i < stages.Length; i++)
            {
                var stage = stages[i];

                if (stage == null || !ShipUpgradeState.IsCompleted(stage.ID))
                    continue;

                if (ApplyBinding(GetBinding(stage.ID), immediately))
                    hasChanges = true;
            }

            if (hasChanges && !immediately)
                RequestNavMeshRebuild();
        }

        public void ApplyStage(ShipUpgradeStage stage, bool immediately)
        {
            if (stage == null)
                return;

            if (ApplyBinding(GetBinding(stage.ID), immediately) && !immediately)
                RequestNavMeshRebuild();
        }

        private bool ApplyBinding(StageVisuals binding, bool immediately)
        {
            if (binding == null)
                return false;

            SetObjectsActive(binding.ObjectsToDisable, false);
            SetObjectsActive(binding.ObjectsToEnable, true);

            if (!immediately && binding.RevealAnimation != null)
                binding.RevealAnimation.RunUnlockedAnimation();

            return binding.RebuildNavMesh;
        }

        private static void SetObjectsActive(GameObject[] objects, bool value)
        {
            if (objects.IsNullOrEmpty())
                return;

            for (var i = 0; i < objects.Length; i++)
            {
                if (objects[i] != null)
                    objects[i].SetActive(value);
            }
        }

        private StageVisuals GetBinding(string stageId)
        {
            if (string.IsNullOrEmpty(stageId) || bindings.IsNullOrEmpty())
                return null;

            for (var i = 0; i < bindings.Length; i++)
            {
                if (bindings[i] != null && bindings[i].StageID == stageId)
                    return bindings[i];
            }

            return null;
        }

        private void RequestNavMeshRebuild()
        {
            if (NavMeshController.IsNavMeshCalculated)
                NavMeshController.CalculateNavMesh();
        }

        [System.Serializable]
        public class StageVisuals
        {
            [SerializeField, ShipStagePicker] string stageId;
            public string StageID => stageId;

            [SerializeField] GameObject[] objectsToEnable;
            public GameObject[] ObjectsToEnable => objectsToEnable;

            [SerializeField] GameObject[] objectsToDisable;
            public GameObject[] ObjectsToDisable => objectsToDisable;

            [Space]
            [SerializeField] AnimationForUnlockable revealAnimation;
            public AnimationForUnlockable RevealAnimation => revealAnimation;

            [SerializeField] bool rebuildNavMesh = true;
            public bool RebuildNavMesh => rebuildNavMesh;
        }

        #region Editor

#if UNITY_EDITOR
        public void EditorPreviewStage(string stageId)
        {
            var binding = GetBinding(stageId);
            if (binding == null)
            {
                Debug.LogWarning($"[Shipyard]: no visual binding for stage '{stageId}'.", gameObject);

                return;
            }

            UnityEditor.Undo.RecordObjects(CollectPreviewUndoTargets(binding), "Preview ship stage");

            SetObjectsActive(binding.ObjectsToDisable, false);
            SetObjectsActive(binding.ObjectsToEnable, true);
        }

        private static Object[] CollectPreviewUndoTargets(StageVisuals binding)
        {
            var targets = new System.Collections.Generic.List<Object>();

            if (!binding.ObjectsToEnable.IsNullOrEmpty())
                targets.AddRange(binding.ObjectsToEnable);

            if (!binding.ObjectsToDisable.IsNullOrEmpty())
                targets.AddRange(binding.ObjectsToDisable);

            targets.RemoveAll(target => target == null);

            return targets.ToArray();
        }
#endif

        #endregion
    }
}
