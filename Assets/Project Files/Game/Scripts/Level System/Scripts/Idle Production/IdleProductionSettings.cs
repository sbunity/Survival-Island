using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class IdleProductionSettings
    {
        [SerializeField, OnOff] bool isEnabled = true;
        public bool IsEnabled => isEnabled;

        [SerializeField, Min(0f)] float maxIdleHours = 8f;
        public float MaxIdleMinutes => maxIdleHours * 60f;

        [SerializeField, Min(0f)] float idleEfficiency = 1f;
        public float IdleEfficiency => idleEfficiency;

        [SerializeField, Min(0f)] float measuredBlendThresholdMinutes = 20f;
        public float MeasuredBlendThresholdMinutes => measuredBlendThresholdMinutes;

        [SerializeField, Min(1f)] float measurementHalfLifeMinutes = 60f;
        public float MeasurementHalfLifeMinutes => measurementHalfLifeMinutes;

        [SerializeField, Min(1f)] float mixConfidenceUnits = 100f;
        public float MixConfidenceUnits => mixConfidenceUnits;
    }
}
