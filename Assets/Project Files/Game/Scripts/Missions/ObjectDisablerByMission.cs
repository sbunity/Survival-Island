using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [DefaultExecutionOrder(10)]
    public class ObjectDisablerByMission : MonoBehaviour
    {
        [MissionPicker]
        [SerializeField] string requiredMissionId;

        private Mission missionRef;

        private bool subscribedToTheMission;

        private void OnEnable()
        {
            Tween.NextFrame(() =>
            {
                missionRef = MissionsController.GetMissionById(requiredMissionId);

                if (missionRef == null)
                {
                    DestroyComponent();
                }
                else
                {
                    if (missionRef.MissionStage == Mission.Stage.Collected)
                    {
                        missionRef = null;

                        if (gameObject != null)
                            gameObject.SetActive(false);

                        DestroyComponent();
                    }
                    else
                    {
                        if (!subscribedToTheMission)
                            missionRef.OnStageChanged += OnStageChanged;

                        subscribedToTheMission = true;
                    }
                }
            });
        }

        private void OnStageChanged(Mission.Stage previousStage, Mission.Stage currentStage)
        {
            if (currentStage == Mission.Stage.Collected)
            {
                if (missionRef != null)
                    missionRef.OnStageChanged -= OnStageChanged;

                if (gameObject != null)
                    gameObject.SetActive(false);

                DestroyComponent();
            }
        }

        private void DestroyComponent()
        {
            if (missionRef != null && subscribedToTheMission)
            {
                missionRef.OnStageChanged -= OnStageChanged;
            }
        }
    }
}
