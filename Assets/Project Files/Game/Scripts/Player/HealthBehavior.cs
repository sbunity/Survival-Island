using System.Runtime.CompilerServices;
using UnityEngine;

namespace Watermelon
{
    public class HealthBehavior : MonoBehaviour, IHealth
    {
        [SerializeField] HealthbarBehaviour healthbar;

        [Space]
        [SerializeField] bool enableRegeneration;
        [SerializeField, Min(0)] float regenerationDelay = 5f;
        [SerializeField, Min(0)] float regenerationPerSecond = 1f;

        public float CurrentHealth { get; private set; }
        public float MaxHealth { get; private set; }

        public bool IsDepleted => CurrentHealth <= 0;
        public bool IsFull => CurrentHealth >= MaxHealth;
        public bool IsRegenerationEnabled => enableRegeneration;

        public bool ShowOnChange { get; set; }

        public bool HideOnFull { get; set; }

        public event SimpleCallback HealthChanged;
        public event SimpleCallback Damaged;
        public event SimpleCallback Depleted;
        public event SimpleCallback Restored;

        private bool isInitialised;
        private bool isHealthbarVisibilityAllowed;
        private float regenerationAllowedTime;

        public void Initialise(float maxHealth)
        {
            Initialise(maxHealth, maxHealth);
        }

        public void Initialise(float maxHealth, float currentHealth)
        {
            MaxHealth = Mathf.Max(0, maxHealth);
            CurrentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth);

            isHealthbarVisibilityAllowed = false;
            regenerationAllowedTime = Time.time + regenerationDelay;

            HealthChanged -= RefreshHealthbar;
            HealthChanged += RefreshHealthbar;

            if (healthbar != null)
                healthbar.Initialise(transform, this, false);

            isInitialised = true;

            NotifyHealthChanged();
        }

        private void Update()
        {
            if (healthbar != null)
                healthbar.FollowUpdate();

            if (!isInitialised || !enableRegeneration || regenerationPerSecond <= 0)
                return;

            if (IsDepleted || IsFull || Time.time < regenerationAllowedTime)
                return;

            Add(regenerationPerSecond * Time.deltaTime);
        }

        public void Add(float value)
        {
            if (value <= 0 || MaxHealth <= 0)
                return;

            var previousHealth = CurrentHealth;
            CurrentHealth = Mathf.Min(CurrentHealth + value, MaxHealth);

            if (Mathf.Approximately(previousHealth, CurrentHealth))
                return;

            NotifyHealthChanged();
            Restored?.Invoke();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddPercent(float value)
        {
            Add(MaxHealth / 100f * value);
        }

        public void Subtract(float value)
        {
            if (value <= 0 || IsDepleted)
                return;

            var previousHealth = CurrentHealth;
            CurrentHealth = Mathf.Max(0, CurrentHealth - value);
            regenerationAllowedTime = Time.time + regenerationDelay;

            if (ShowOnChange)
                isHealthbarVisibilityAllowed = true;

            NotifyHealthChanged();
            Damaged?.Invoke();

            if (previousHealth > 0 && IsDepleted)
                Depleted?.Invoke();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SubtractPercent(float value)
        {
            Subtract(MaxHealth / 100f * value);
        }

        public void SetCurrentHealth(float value)
        {
            CurrentHealth = Mathf.Clamp(value, 0, MaxHealth);
            regenerationAllowedTime = Time.time + regenerationDelay;

            NotifyHealthChanged();
        }

        public void Restore()
        {
            if (IsFull)
            {
                NotifyHealthChanged();
                return;
            }

            CurrentHealth = MaxHealth;

            NotifyHealthChanged();
            Restored?.Invoke();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Show()
        {
            isHealthbarVisibilityAllowed = true;
            RefreshHealthbar();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Hide()
        {
            isHealthbarVisibilityAllowed = false;
            RefreshHealthbar();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForceHide()
        {
            isHealthbarVisibilityAllowed = false;

            if (healthbar != null)
                healthbar.ForceDisable();
        }

        private void NotifyHealthChanged()
        {
            HealthChanged?.Invoke();
        }

        private void RefreshHealthbar()
        {
            if (healthbar == null)
                return;

            healthbar.OnHealthChanged();

            var shouldBeVisible = isHealthbarVisibilityAllowed && CurrentHealth > 0 && CurrentHealth < MaxHealth;
            if (shouldBeVisible)
                healthbar.EnableBar();
            else
                healthbar.DisableBar();
        }
    }
}
