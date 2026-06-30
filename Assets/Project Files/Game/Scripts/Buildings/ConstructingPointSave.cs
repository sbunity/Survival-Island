using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class ConstructingPointSave : SimpleIntSave
    {
        [SerializeField] bool isBought;
        public bool IsBought { get => isBought; set => isBought = value; }

        [SerializeField] bool hasHealthData;
        public bool HasHealthData { get => hasHealthData; set => hasHealthData = value; }

        [SerializeField] float currentHealth;
        public float CurrentHealth { get => currentHealth; set => currentHealth = value; }

        [SerializeField] bool isDestroyed;
        public bool IsDestroyed { get => isDestroyed; set => isDestroyed = value; }
    }
}
