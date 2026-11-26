using UnityEngine;

namespace Watermelon
{
    public class MissionComicTrigger : ComicTrigger
    {
        [SerializeField] Mission mission;

        protected override bool CanArm() => mission != null && mission.MissionStage != Mission.Stage.Finished && mission.MissionStage != Mission.Stage.Collected;

        protected override void Subscribe() => mission.OnStageChanged += OnMissionStageChanged;

        protected override void Unsubscribe() => mission.OnStageChanged -= OnMissionStageChanged;

        private void OnMissionStageChanged(Mission.Stage previousStage, Mission.Stage currentStage)
        {
            if (previousStage == Mission.Stage.Active && currentStage == Mission.Stage.Finished)
                Fire();
        }
    }
}
