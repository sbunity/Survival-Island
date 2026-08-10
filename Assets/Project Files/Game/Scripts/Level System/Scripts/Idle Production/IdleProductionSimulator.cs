using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public static class IdleProductionSimulator
    {
        private static readonly Dictionary<CurrencyType, float> producedBuffer = new Dictionary<CurrencyType, float>();
        private static readonly List<ResourceListSave> touchedSaves = new List<ResourceListSave>();

        public static IdleProductionReport Simulate(string worldId, WorldProductionSnapshot snapshot, IdleProductionSettings settings)
        {
            var report = new IdleProductionReport { WorldId = worldId };

            if (snapshot == null || settings == null || !settings.IsEnabled || !snapshot.CanProduce)
                return report;

            var elapsedMinutes = Mathf.Clamp((IdleClock.Now - snapshot.LastSimulatedGameTime) / 60f, 0f, settings.MaxIdleMinutes);
            if (elapsedMinutes <= 0f)
            {
                snapshot.Checkpoint();

                return report;
            }

            report.ElapsedMinutes = elapsedMinutes;

            AccumulateProduction(snapshot, elapsedMinutes * settings.IdleEfficiency, settings);

            DistributeToSinks(worldId, snapshot, report);
            ConvertResources(worldId, snapshot, elapsedMinutes, report);

            producedBuffer.Clear();

            CommitTouchedSaves();

            snapshot.Checkpoint();

            if (!report.IsEmpty)
                SaveController.MarkAsSaveIsRequired();

            return report;
        }

        private static void AccumulateProduction(WorldProductionSnapshot snapshot, float effectiveMinutes, IdleProductionSettings settings)
        {
            producedBuffer.Clear();

            var sources = snapshot.Sources;

            foreach (var producer in snapshot.Producers)
            {
                if (producer == null)
                    continue;

                var rate = GetRate(producer, settings);
                if (rate <= 0f)
                    continue;

                var totalWeight = 0;
                foreach (var source in sources)
                {
                    if (CanHarvest(producer.TaskMask, source.TaskTypeFlag))
                        totalWeight += source.SourceCount;
                }

                if (totalWeight <= 0)
                    continue;

                var units = rate * effectiveMinutes;

                foreach (var source in sources)
                {
                    if (!CanHarvest(producer.TaskMask, source.TaskTypeFlag))
                        continue;

                    producedBuffer.TryGetValue(source.Currency, out float current);
                    producedBuffer[source.Currency] = current + units * source.SourceCount / totalWeight;
                }
            }
        }

        private static float GetRate(WorldProductionSnapshot.ProducerEntry producer, IdleProductionSettings settings)
        {
            if (producer.SampleMinutes <= 0f || producer.MeasuredUnitsPerMinute <= 0f)
                return producer.AuthoredUnitsPerMinute;

            var threshold = settings.MeasuredBlendThresholdMinutes;
            var trust = threshold > 0f ? Mathf.Clamp01(producer.SampleMinutes / threshold) : 1f;

            return Mathf.Lerp(producer.AuthoredUnitsPerMinute, producer.MeasuredUnitsPerMinute, trust);
        }

        private static bool CanHarvest(int taskMask, int sourceFlag)
        {
            return sourceFlag != 0 && (taskMask & sourceFlag) == sourceFlag;
        }

        private static void DistributeToSinks(string worldId, WorldProductionSnapshot snapshot, IdleProductionReport report)
        {
            foreach (var sink in snapshot.Sinks)
            {
                if (sink == null)
                    continue;

                var save = GetResourceSave(worldId, sink.SaveKey);
                if (save == null)
                    continue;

                if (!sink.PerCurrencyCapacity.IsNullOrEmpty())
                    FillPerCurrencySink(sink, save, report);
                else
                    FillFlatSink(sink, save, report);
            }
        }

        private static void FillFlatSink(WorldProductionSnapshot.SinkEntry sink, ResourceListSave save, IdleProductionReport report)
        {
            if (sink.Accepted.IsNullOrEmpty() || sink.FlatCapacity <= 0)
                return;

            var space = sink.FlatCapacity - TotalAmount(save.Resources);
            if (space <= 0)
                return;

            foreach (var currency in sink.Accepted)
            {
                if (space <= 0)
                    break;

                var amount = Take(currency, space);
                if (amount <= 0)
                    continue;

                save.Resources += new Resource(currency, amount);
                space -= amount;

                report.AddGathered(currency, amount);
            }
        }

        private static void FillPerCurrencySink(WorldProductionSnapshot.SinkEntry sink, ResourceListSave save, IdleProductionReport report)
        {
            foreach (var capacity in sink.PerCurrencyCapacity)
            {
                var space = capacity.amount - AmountOf(save.Resources, capacity.currency);
                if (space <= 0)
                    continue;

                var amount = Take(capacity.currency, space);
                if (amount <= 0)
                    continue;

                save.Resources += new Resource(capacity.currency, amount);

                report.AddGathered(capacity.currency, amount);
            }
        }

        private static int Take(CurrencyType currency, int limit)
        {
            if (limit <= 0 || !producedBuffer.TryGetValue(currency, out float available))
                return 0;

            var amount = Mathf.Min(Mathf.FloorToInt(available), limit);
            if (amount <= 0)
                return 0;

            producedBuffer[currency] = available - amount;

            return amount;
        }

        private static void ConvertResources(string worldId, WorldProductionSnapshot snapshot, float elapsedMinutes, IdleProductionReport report)
        {
            if (snapshot.Converters.IsNullOrEmpty())
                return;

            foreach (var converter in snapshot.Converters)
            {
                if (converter == null || converter.Recipe.IsNullOrEmpty() || converter.OutCapacity <= 0)
                    continue;

                var inSave = GetResourceSave(worldId, converter.InSaveKey);
                var outSave = GetResourceSave(worldId, converter.OutSaveKey);

                if (inSave == null || outSave == null)
                    continue;

                var cycles = converter.Duration > 0f
                    ? Mathf.FloorToInt(elapsedMinutes * 60f / converter.Duration)
                    : int.MaxValue;

                var outAmount = TotalAmount(outSave.Resources);
                var produced = 0;

                while (cycles > 0 && outAmount < converter.OutCapacity && HasIngredients(inSave.Resources, converter.Recipe))
                {
                    for (var i = 0; i < converter.Recipe.Length; i++)
                        inSave.Resources -= converter.Recipe[i];

                    outSave.Resources += new Resource(converter.Result, 1);

                    outAmount++;
                    produced++;
                    cycles--;
                }

                if (produced > 0)
                    report.AddConverted(converter.Result, produced);
            }
        }

        private static bool HasIngredients(ResourcesList storage, Resource[] recipe)
        {
            for (var i = 0; i < recipe.Length; i++)
            {
                if (AmountOf(storage, recipe[i].currency) < recipe[i].amount)
                    return false;
            }

            return true;
        }

        private static ResourceListSave GetResourceSave(string worldId, string saveKey)
        {
            if (string.IsNullOrEmpty(saveKey))
                return null;

            var save = SaveController.GetSaveObject<ResourceListSave>(worldId, saveKey);
            if (save == null)
                return null;

            if (save.Resources == null)
                save.Init();

            if (!touchedSaves.Contains(save))
                touchedSaves.Add(save);

            return save;
        }

        private static void CommitTouchedSaves()
        {
            for (var i = 0; i < touchedSaves.Count; i++)
                touchedSaves[i].OnBeforeSave();

            touchedSaves.Clear();
        }

        private static int TotalAmount(ResourcesList list)
        {
            var total = 0;

            for (var i = 0; i < list.Count; i++)
                total += list[i].amount;

            return total;
        }

        private static int AmountOf(ResourcesList list, CurrencyType currency)
        {
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i].currency == currency)
                    return list[i].amount;
            }

            return 0;
        }
    }
}
