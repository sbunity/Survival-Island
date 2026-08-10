using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public static class WorldProductionSnapshotBuilder
    {
        public static void Capture(BaseWorldBehavior world, WorldProductionSnapshot snapshot, string worldId, IdleProductionSettings settings)
        {
            if (world == null || snapshot == null || string.IsNullOrEmpty(worldId))
                return;

            var taskHandler = world.TaskHandler;
            if (taskHandler == null || taskHandler.Tasks == null)
                return;

            snapshot.Apply(
                CaptureProducers(world, snapshot, worldId, settings),
                CaptureSources(taskHandler),
                CaptureSinks(taskHandler),
                CaptureConverters(taskHandler));
        }

        private static WorldProductionSnapshot.ProducerEntry[] CaptureProducers(BaseWorldBehavior world, WorldProductionSnapshot snapshot, string worldId, IdleProductionSettings settings)
        {
            var helpers = world.GetComponentsInChildren<HelperBehavior>(true);

            var entries = new List<WorldProductionSnapshot.ProducerEntry>(helpers.Length);

            foreach (HelperBehavior helper in helpers)
            {
                if (helper == null || !helper.IsOpened)
                    continue;

                var entry = new WorldProductionSnapshot.ProducerEntry
                {
                    HelperId = helper.ID,
                    MeasuredWorldId = worldId,
                    TaskMask = (int)helper.AvailableTaskTypes,
                    AuthoredUnitsPerMinute = helper.IdleUnitsPerMinute
                };

                CarryOverMeasurement(snapshot, entry, worldId);
                FoldInSession(entry, helper, settings);

                entries.Add(entry);
            }

            return entries.ToArray();
        }

        private static void CarryOverMeasurement(WorldProductionSnapshot snapshot, WorldProductionSnapshot.ProducerEntry entry, string worldId)
        {
            if (snapshot.Producers.IsNullOrEmpty())
                return;

            foreach (var previous in snapshot.Producers)
            {
                if (previous == null || previous.HelperId != entry.HelperId || previous.MeasuredWorldId != worldId)
                    continue;

                entry.MeasuredUnitsPerMinute = previous.MeasuredUnitsPerMinute;
                entry.SampleMinutes = previous.SampleMinutes;

                break;
            }
        }

        private static void FoldInSession(WorldProductionSnapshot.ProducerEntry entry, HelperBehavior helper, IdleProductionSettings settings)
        {
            FoldMeasurement(entry, helper.IdleActiveMinutes, helper.IdleDeliveredUnits, settings);
        }

        public static void FoldMeasurement(WorldProductionSnapshot.ProducerEntry entry, float sessionMinutes, int sessionUnits, IdleProductionSettings settings)
        {
            if (entry == null || settings == null)
                return;

            if (sessionMinutes <= 0f || sessionUnits <= 0)
                return;

            var decayedSample = entry.SampleMinutes * Mathf.Pow(0.5f, sessionMinutes / settings.MeasurementHalfLifeMinutes);
            var totalWeight = decayedSample + sessionMinutes;

            entry.MeasuredUnitsPerMinute = (entry.MeasuredUnitsPerMinute * decayedSample + sessionUnits) / totalWeight;
            entry.SampleMinutes = totalWeight;
        }

        private static WorldProductionSnapshot.SourceEntry[] CaptureSources(TaskHandler taskHandler)
        {
            var entries = new List<WorldProductionSnapshot.SourceEntry>();

            foreach (var task in taskHandler.Tasks)
            {
                if (task == null || !task.IsActive)
                    continue;

                if (task is GatheringTask gatheringTask)
                {
                    var source = gatheringTask.ResourceSource;
                    if (source == null || !source.IsHelperTaskActive || source.Drop.IsNullOrEmpty())
                        continue;

                    foreach (var drop in source.Drop)
                        AddSource(entries, drop.currency, (int)gatheringTask.Type);
                }
                else if (task is FishingTask fishingTask)
                {
                    var fishingPlace = fishingTask.FishingPlaceBehavior;
                    if (fishingPlace == null || !fishingPlace.IsHelperTaskActive)
                        continue;

                    AddSource(entries, fishingPlace.DropType, (int)fishingTask.Type);
                }
            }

            return entries.ToArray();
        }

        private static void AddSource(List<WorldProductionSnapshot.SourceEntry> entries, CurrencyType currency, int taskTypeFlag)
        {
            foreach (var entry in entries)
            {
                if (entry.Currency != currency || entry.TaskTypeFlag != taskTypeFlag)
                    continue;

                entry.SourceCount++;

                return;
            }

            entries.Add(new WorldProductionSnapshot.SourceEntry
            {
                Currency = currency,
                TaskTypeFlag = taskTypeFlag,
                SourceCount = 1
            });
        }

        private static WorldProductionSnapshot.SinkEntry[] CaptureSinks(TaskHandler taskHandler)
        {
            var entries = new List<WorldProductionSnapshot.SinkEntry>();

            foreach (var task in taskHandler.Tasks)
            {
                if (task is StoreResourcesTask storeTask)
                {
                    var building = storeTask.StorageBuildingBehavior;
                    if (building == null || !building.IsOperational || !building.IsHelperTaskActive)
                        continue;

                    entries.Add(new WorldProductionSnapshot.SinkEntry
                    {
                        SaveKey = $"{building.ID}_Storage",
                        Accepted = building.StoredResources.ToArray(),
                        FlatCapacity = building.Storage.Capacity
                    });
                }
                else if (task is ConverterStoringTask converterTask)
                {
                    var converter = converterTask.ResourceConverter;
                    if (converter == null || !converter.IsOperational || !converter.IsHelperTaskActive)
                        continue;

                    entries.Add(new WorldProductionSnapshot.SinkEntry
                    {
                        SaveKey = $"{converter.ID}_IngridientsStorage",
                        PerCurrencyCapacity = BuildInputCapacity(converter)
                    });
                }
            }

            return entries.ToArray();
        }

        private static WorldProductionSnapshot.ConverterEntry[] CaptureConverters(TaskHandler taskHandler)
        {
            var entries = new List<WorldProductionSnapshot.ConverterEntry>();

            foreach (var task in taskHandler.Tasks)
            {
                if (task is not ConverterStoringTask converterTask)
                    continue;

                var converter = converterTask.ResourceConverter;
                if (converter == null || !converter.IsOperational || converter.Recipe == null)
                    continue;

                entries.Add(new WorldProductionSnapshot.ConverterEntry
                {
                    InSaveKey = $"{converter.ID}_IngridientsStorage",
                    OutSaveKey = $"{converter.ID}_ResultStorage",
                    Recipe = BuildRecipe(converter.Recipe),
                    Result = converter.Recipe.ResultResourceType,
                    OutCapacity = converter.OutputStorageCapacity,
                    Duration = converter.ConversionDuration
                });
            }

            return entries.ToArray();
        }

        private static Resource[] BuildRecipe(Recipe recipe)
        {
            var components = new Resource[recipe.ComponentsAmount];

            for (int i = 0; i < components.Length; i++)
                components[i] = recipe.GetComponent(i);

            return components;
        }

        private static Resource[] BuildInputCapacity(ResourceConverterBuildingBehavior converter)
        {
            var recipe = converter.Recipe;
            if (recipe == null)
                return null;

            var capacity = new Resource[recipe.ComponentsAmount];

            for (var i = 0; i < capacity.Length; i++)
            {
                var component = recipe.GetComponent(i);

                capacity[i] = new Resource(component.ResourceType, component.Amount * converter.InputStorageCapacity);
            }

            return capacity;
        }
    }
}
