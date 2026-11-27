using UnityEngine;

namespace Watermelon
{
    public class MissionComicTrigger : ComicTrigger
    {
        [SerializeField] Mission mission;

        protected override bool CanArm() => mission != null && !IsCompleted(mission.MissionStage);

        protected override void Subscribe() => mission.OnStageChanged += OnMissionStageChanged;

        protected override void Unsubscribe() => mission.OnStageChanged -= OnMissionStageChanged;

        private void OnMissionStageChanged(Mission.Stage previousStage, Mission.Stage currentStage)
        {
            if (currentStage == Mission.Stage.Finished && !IsCompleted(previousStage))
                Fire();
        }

        private static bool IsCompleted(Mission.Stage stage) => stage == Mission.Stage.Finished || stage == Mission.Stage.Collected;
    }
}
