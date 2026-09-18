using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Watermelon
{
    public class TaskHandler
    {
        public const float DEFAULT_SUPPRESSION_DURATION = 20f;

        private List<BaseTask> tasks = new();
        public List<BaseTask> Tasks => tasks;

        private List<TaskSuppression> suppressions = new();

        public void Initialise()
        {

        }

        public void Unload()
        {
            foreach(var task in tasks)
            {
                task.Unload();
            }

            tasks.Clear();
            suppressions.Clear();
        }

        public void RegisterTask(BaseTask task)
        {
            tasks.Add(task);
        }

        public void RemoveTask(BaseTask task)
        {
            tasks.Remove(task);

            suppressions.RemoveAll(x => x.Task == task);
        }

        public BaseTask GetAvailableTask(HelperBehavior helperBehavior)
        {
            PruneSuppressions();

            HelperTaskType taskType = helperBehavior.AvailableTaskTypes;

            IEnumerable<BaseTask> filteredTasks = tasks.Where(x => x.IsActive && !x.IsTaken() && x.IsTypeAvailable(taskType) && x.IsInRange(helperBehavior) && !IsSuppressed(helperBehavior, x));

            // Randomize list
            filteredTasks = filteredTasks.OrderBy(x => Random.value);

            // Order by priority
            filteredTasks = filteredTasks.OrderByDescending(x => x.GetPriority(helperBehavior));

            // Validate
            filteredTasks = filteredTasks.Where(x => x.Validate(helperBehavior));

            // Get first task with path or null
            return filteredTasks.FirstOrDefault(x => x.IsPathExists(helperBehavior));
        }

        #region Suppression

        public void SuppressTask(HelperBehavior helperBehavior, BaseTask task, float duration = DEFAULT_SUPPRESSION_DURATION)
        {
            if (helperBehavior == null || task == null || duration <= 0f)
                return;

            var expireTime = Time.time + duration;

            for (var i = 0; i < suppressions.Count; i++)
            {
                if (suppressions[i].Matches(helperBehavior, task))
                {
                    suppressions[i] = new TaskSuppression(helperBehavior, task, expireTime);

                    return;
                }
            }

            suppressions.Add(new TaskSuppression(helperBehavior, task, expireTime));
        }

        public bool IsSuppressed(HelperBehavior helperBehavior, BaseTask task)
        {
            for (var i = 0; i < suppressions.Count; i++)
            {
                if (suppressions[i].Matches(helperBehavior, task))
                    return suppressions[i].ExpireTime > Time.time;
            }

            return false;
        }

        private void PruneSuppressions()
        {
            for (var i = suppressions.Count - 1; i >= 0; i--)
            {
                var suppression = suppressions[i];

                if (suppression.ExpireTime <= Time.time || suppression.Helper == null)
                    suppressions.RemoveAt(i);
            }
        }

        private readonly struct TaskSuppression
        {
            public readonly HelperBehavior Helper;
            public readonly BaseTask Task;
            public readonly float ExpireTime;

            public TaskSuppression(HelperBehavior helper, BaseTask task, float expireTime)
            {
                Helper = helper;
                Task = task;
                ExpireTime = expireTime;
            }

            public bool Matches(HelperBehavior helper, BaseTask task)
            {
                return ReferenceEquals(Task, task) && Helper == helper;
            }
        }

        #endregion

        public void DebugPrint()
        {
            tasks.Display(x => x.ToString());
        }
    }
}
