using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    [SaveKey(SAVE_KEY)]
    public class WorldProductionSnapshot : ISaveObject
    {
        public const string SAVE_KEY = "idle_production";

        [SerializeField] float lastSimulatedGameTime;
        [SerializeField] ProducerEntry[] producers;
        [SerializeField] SourceEntry[] sources;
        [SerializeField] SinkEntry[] sinks;
        [SerializeField] ConverterEntry[] converters;

        [System.NonSerialized] private bool isLive;
        [System.NonSerialized] private System.Action flushCallback;

        public float LastSimulatedGameTime => lastSimulatedGameTime;

        public ProducerEntry[] Producers => producers;
        public SourceEntry[] Sources => sources;
        public SinkEntry[] Sinks => sinks;
        public ConverterEntry[] Converters => converters;

        public bool CanProduce => !producers.IsNullOrEmpty() && !sources.IsNullOrEmpty() && !sinks.IsNullOrEmpty();

        public bool IsLive => isLive;

        public void Apply(ProducerEntry[] producers, SourceEntry[] sources, SinkEntry[] sinks, ConverterEntry[] converters)
        {
            this.producers = producers;
            this.sources = sources;
            this.sinks = sinks;
            this.converters = converters;
        }

        public void Checkpoint()
        {
            lastSimulatedGameTime = IdleClock.Now;
        }

        public void SetLive(bool value, System.Action onFlush = null)
        {
            isLive = value;
            flushCallback = value ? onFlush : null;

            Checkpoint();
        }

        public void Invalidate()
        {
            producers = null;
            sources = null;
            sinks = null;
            converters = null;

            Checkpoint();
        }

        public void OnBeforeSave()
        {
            if (!isLive)
                return;

            flushCallback?.Invoke();

            Checkpoint();
        }

        [System.Serializable]
        public class ProducerEntry
        {
            public string HelperId;

            public string MeasuredWorldId;

            public int TaskMask;
            public float AuthoredUnitsPerMinute;
            public float MeasuredUnitsPerMinute;
            public float SampleMinutes;
        }

        [System.Serializable]
        public class SourceEntry
        {
            public CurrencyType Currency;
            public int TaskTypeFlag;
            public int SourceCount;
        }

        [System.Serializable]
        public class SinkEntry
        {
            public string SaveKey;
            public CurrencyType[] Accepted;
            public int FlatCapacity;
            public Resource[] PerCurrencyCapacity;

            public Resource[] ObservedMix;

            public bool IsStorage => PerCurrencyCapacity == null || PerCurrencyCapacity.Length == 0;
        }

        [System.Serializable]
        public class ConverterEntry
        {
            public string InSaveKey;
            public string OutSaveKey;
            public Resource[] Recipe;
            public CurrencyType Result;
            public int OutCapacity;
            public float Duration;
        }
    }
}
