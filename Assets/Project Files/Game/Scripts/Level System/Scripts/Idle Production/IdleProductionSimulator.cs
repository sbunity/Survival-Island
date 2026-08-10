using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public static class IdleProductionSimulator
    {
        private const int DISTRIBUTION_PASSES = 3;

        private static readonly List<ResourceListSave> touchedSaves = new();
        private static readonly HashSet<CurrencyType> producibleCurrencies = new();

        private static readonly List<WorldProductionSnapshot.SinkEntry> candidateSinks = new();
        private static readonly List<ResourceListSave> candidateSaves = new();
        private static readonly List<float> candidateWeights = new();
        private static readonly List<float> candidateObserved = new();

        private static readonly List<CurrencyType> slotCurrencies = new();
        private static readonly List<int> slotSpace = new();
        private static readonly List<float> slotWeights = new();
        private static readonly List<float> slotObserved = new();

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

            CollectProducibleCurrencies(snapshot, settings);

            var budget = TotalRatePerMinute(snapshot, settings) * elapsedMinutes * settings.IdleEfficiency;

            budget = FillSinks(worldId, snapshot, settings, report, budget, true);

            if (!HasSpace(worldId, snapshot, true))
                FillSinks(worldId, snapshot, settings, report, budget, false);

            ConvertResources(worldId, snapshot, elapsedMinutes, report);

            CommitTouchedSaves();
            producibleCurrencies.Clear();

            snapshot.Checkpoint();

            if (!report.IsEmpty)
                SaveController.MarkAsSaveIsRequired();

            return report;
        }

        private static void CollectProducibleCurrencies(WorldProductionSnapshot snapshot, IdleProductionSettings settings)
        {
            producibleCurrencies.Clear();

            foreach (var source in snapshot.Sources)
            {
                foreach (var producer in snapshot.Producers)
                {
                    if (producer == null || GetRate(producer, settings) <= 0f)
                        continue;

                    if (!CanHarvest(producer.TaskMask, source.TaskTypeFlag))
                        continue;

                    producibleCurrencies.Add(source.Currency);

                    break;
                }
            }
        }

        private static float TotalRatePerMinute(WorldProductionSnapshot snapshot, IdleProductionSettings settings)
        {
            var total = 0f;

            foreach (var producer in snapshot.Producers)
            {
                if (producer == null || !CanHarvestAnything(producer, snapshot))
                    continue;

                total += Mathf.Max(0f, GetRate(producer, settings));
            }

            return total;
        }

        private static bool CanHarvestAnything(WorldProductionSnapshot.ProducerEntry producer, WorldProductionSnapshot snapshot)
        {
            foreach (var source in snapshot.Sources)
            {
                if (CanHarvest(producer.TaskMask, source.TaskTypeFlag))
                    return true;
            }

            return false;
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

        private static float FillSinks(string worldId, WorldProductionSnapshot snapshot, IdleProductionSettings settings, IdleProductionReport report, float budget, bool storagePhase)
        {
            if (snapshot.Sinks.IsNullOrEmpty())
                return budget;

            for (var pass = 0; pass < DISTRIBUTION_PASSES && budget >= 1f; pass++)
            {
                GatherCandidates(worldId, snapshot, storagePhase);

                if (candidateSinks.Count == 0)
                    break;

                ComputeSinkWeights(settings);

                var spent = 0f;
                for (var i = 0; i < candidateSinks.Count; i++)
                    spent += FillSink(candidateSinks[i], candidateSaves[i], budget * candidateWeights[i], settings, report);

                if (spent < 1f)
                    break;

                budget -= spent;
            }

            return Mathf.Max(0f, budget);
        }

        private static bool HasSpace(string worldId, WorldProductionSnapshot snapshot, bool storagePhase)
        {
            GatherCandidates(worldId, snapshot, storagePhase);

            return candidateSinks.Count > 0;
        }

        private static void GatherCandidates(string worldId, WorldProductionSnapshot snapshot, bool storagePhase)
        {
            candidateSinks.Clear();
            candidateSaves.Clear();

            foreach (var sink in snapshot.Sinks)
            {
                if (sink == null || sink.IsStorage != storagePhase)
                    continue;

                var save = GetResourceSave(worldId, sink.SaveKey);
                if (save == null || FreeSpace(sink, save) <= 0)
                    continue;

                BuildSlots(sink, save);
                if (slotCurrencies.Count == 0)
                    continue;

                candidateSinks.Add(sink);
                candidateSaves.Add(save);
            }
        }

        private static void ComputeSinkWeights(IdleProductionSettings settings)
        {
            candidateWeights.Clear();
            candidateObserved.Clear();

            var totalObserved = 0f;
            for (var i = 0; i < candidateSinks.Count; i++)
            {
                var observed = SumObserved(candidateSinks[i].ObservedMix);

                candidateObserved.Add(observed);
                totalObserved += observed;
            }

            var evenShare = 1f / candidateSinks.Count;
            var trust = Mathf.Clamp01(totalObserved / (candidateSinks.Count * settings.MixConfidenceUnits));

            for (var i = 0; i < candidateSinks.Count; i++)
            {
                candidateWeights.Add(totalObserved > 0f
                    ? Mathf.Lerp(evenShare, candidateObserved[i] / totalObserved, trust)
                    : evenShare);
            }
        }

        private static float FillSink(WorldProductionSnapshot.SinkEntry sink, ResourceListSave save, float share, IdleProductionSettings settings, IdleProductionReport report)
        {
            if (share < 1f)
                return 0f;

            BuildSlots(sink, save);
            if (slotCurrencies.Count == 0)
                return 0f;

            ComputeSlotWeights(sink, settings);

            var remainingSpace = FreeSpace(sink, save);

            share = Mathf.Min(share, remainingSpace);

            var spent = 0;

            for (var i = 0; i < slotCurrencies.Count && remainingSpace > 0; i++)
            {
                var amount = Mathf.Min(Mathf.Min(Mathf.FloorToInt(share * slotWeights[i]), slotSpace[i]), remainingSpace);
                if (amount <= 0)
                    continue;

                Deposit(save, slotCurrencies[i], amount, report);

                slotSpace[i] -= amount;
                remainingSpace -= amount;
                spent += amount;
            }

            var leftover = Mathf.FloorToInt(share) - spent;
            for (var i = 0; i < slotCurrencies.Count && leftover > 0 && remainingSpace > 0; i++)
            {
                var amount = Mathf.Min(Mathf.Min(leftover, slotSpace[i]), remainingSpace);
                if (amount <= 0)
                    continue;

                Deposit(save, slotCurrencies[i], amount, report);

                slotSpace[i] -= amount;
                remainingSpace -= amount;
                leftover -= amount;
                spent += amount;
            }

            return spent;
        }

        private static void BuildSlots(WorldProductionSnapshot.SinkEntry sink, ResourceListSave save)
        {
            slotCurrencies.Clear();
            slotSpace.Clear();

            if (sink.IsStorage)
            {
                if (sink.Accepted.IsNullOrEmpty() || sink.FlatCapacity <= 0)
                    return;

                var space = sink.FlatCapacity - TotalAmount(save.Resources);
                if (space <= 0)
                    return;

                foreach (var currency in sink.Accepted)
                {
                    if (!producibleCurrencies.Contains(currency) || slotCurrencies.Contains(currency))
                        continue;

                    slotCurrencies.Add(currency);
                    slotSpace.Add(space);
                }

                return;
            }

            foreach (var capacity in sink.PerCurrencyCapacity)
            {
                if (!producibleCurrencies.Contains(capacity.currency) || slotCurrencies.Contains(capacity.currency))
                    continue;

                var space = capacity.amount - AmountOf(save.Resources, capacity.currency);
                if (space <= 0)
                    continue;

                slotCurrencies.Add(capacity.currency);
                slotSpace.Add(space);
            }
        }

        private static void ComputeSlotWeights(WorldProductionSnapshot.SinkEntry sink, IdleProductionSettings settings)
        {
            slotWeights.Clear();
            slotObserved.Clear();

            var totalObserved = 0f;
            for (var i = 0; i < slotCurrencies.Count; i++)
            {
                var observed = ObservedAmount(sink.ObservedMix, slotCurrencies[i]);

                slotObserved.Add(observed);
                totalObserved += observed;
            }

            var evenShare = 1f / slotCurrencies.Count;
            var trust = Mathf.Clamp01(totalObserved / settings.MixConfidenceUnits);

            for (var i = 0; i < slotCurrencies.Count; i++)
            {
                slotWeights.Add(totalObserved > 0f
                    ? Mathf.Lerp(evenShare, slotObserved[i] / totalObserved, trust)
                    : evenShare);
            }
        }

        private static void Deposit(ResourceListSave save, CurrencyType currency, int amount, IdleProductionReport report)
        {
            save.Resources += new Resource(currency, amount);

            report.AddGathered(currency, amount);
        }

        private static int FreeSpace(WorldProductionSnapshot.SinkEntry sink, ResourceListSave save)
        {
            if (sink.IsStorage)
                return sink.FlatCapacity - TotalAmount(save.Resources);

            var space = 0;
            foreach (var capacity in sink.PerCurrencyCapacity)
                space += Mathf.Max(0, capacity.amount - AmountOf(save.Resources, capacity.currency));

            return space;
        }

        private static float SumObserved(Resource[] mix)
        {
            if (mix == null)
                return 0f;

            var total = 0f;
            for (var i = 0; i < mix.Length; i++)
                total += mix[i].amount;

            return total;
        }

        private static float ObservedAmount(Resource[] mix, CurrencyType currency)
        {
            if (mix == null)
                return 0f;

            for (var i = 0; i < mix.Length; i++)
            {
                if (mix[i].currency == currency)
                    return mix[i].amount;
            }

            return 0f;
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
