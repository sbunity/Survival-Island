using System;
using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// Persists session timing data: last exit timestamp and accumulated gameplay time.
    /// Automatically loaded by <see cref="SaveController"/> during initialization.
    /// Use <see cref="SaveController.LastExitTime"/> and <see cref="SaveController.GameTime"/> to access values.
    /// </summary>
    [System.Serializable]
    [SaveKey("sys_time")]
    public class TimeSave : ISaveObject
    {
        [SerializeField] private long lastExitUnix;
        [SerializeField] private float gameTime;

        [System.NonSerialized] private float sessionCheckpointRealtime;

        /// <summary>The time the game was last saved (approximates last exit time). Returns <see cref="DateTime.MinValue"/> if never saved.</summary>
        public DateTime LastExitTime => lastExitUnix > 0
            ? DateTimeOffset.FromUnixTimeSeconds(lastExitUnix).LocalDateTime
            : DateTime.MinValue;

        /// <summary>Total accumulated gameplay time in seconds, including the current session.</summary>
        public float GameTime => gameTime + (Time.realtimeSinceStartup - sessionCheckpointRealtime);

        /// <summary>
        /// Marks the start of the current session for accurate <see cref="GameTime"/> tracking.
        /// Must be called once after the save is loaded, before any <see cref="GameTime"/> reads.
        /// </summary>
        public void BeginSession()
        {
            sessionCheckpointRealtime = Time.realtimeSinceStartup;
        }

        public void OnBeforeSave()
        {
            float elapsed = Time.realtimeSinceStartup - sessionCheckpointRealtime;
            gameTime += elapsed;
            sessionCheckpointRealtime = Time.realtimeSinceStartup;

            lastExitUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }
}
