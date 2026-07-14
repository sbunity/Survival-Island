using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class HealthbarBehaviour : MonoBehaviour
    {
        [SerializeField] Transform healthBarTransform;
        public Transform HealthBarTransform => healthBarTransform;

        [SerializeField] Vector3 healthbarOffset;
        public Vector3 HealthbarOffset => healthbarOffset;

        [Space]
        [SerializeField] CanvasGroup healthBarCanvasGroup;
        [SerializeField] Image healthFillImage;
        [SerializeField] Image maskFillImage;
        [SerializeField] TextMeshProUGUI healthText;

        [Space]
        [SerializeField] Color standartHealthbarColor;

        private IHealth targetHealth;

        private bool isInitialised;

        private bool isDisabled;
        public bool IsDisabled => isDisabled;

        private TweenCase fadeTweenCase;

        public void Initialise(Transform parentTransform, IHealth targetHealth, bool showAlways)
        {
            this.targetHealth = targetHealth;

            fadeTweenCase.KillActive();

            isDisabled = !showAlways;

            healthBarTransform.localPosition = HealthbarOffset;
            healthBarTransform.gameObject.SetActive(true);
            healthBarCanvasGroup.gameObject.SetActive(true);

            healthFillImage.color = standartHealthbarColor;
            healthBarCanvasGroup.alpha = showAlways ? 1.0f : 0.0f;

            RedrawHealth();

            isInitialised = true;
        }

        public void FollowUpdate()
        {
            if (!isInitialised || Camera.main == null)
                return;

            healthBarTransform.rotation = Camera.main.transform.rotation;
        }

        public void OnHealthChanged()
        {
            RedrawHealth();
        }

        public void DisableBar()
        {
            if (isDisabled)
                return;

            isDisabled = true;

            fadeTweenCase.KillActive();

            fadeTweenCase = healthBarCanvasGroup.DOFade(0.0f, 0.3f).OnComplete(delegate
            {
                healthBarTransform.gameObject.SetActive(false);
            });
        }

        public void EnableBar()
        {
            if (!isDisabled)
                return;

            isDisabled = false;

            healthBarTransform.gameObject.SetActive(true);
            healthBarCanvasGroup.gameObject.SetActive(true);

            fadeTweenCase.KillActive();
            fadeTweenCase = healthBarCanvasGroup.DOFade(1.0f, 0.3f);
        }

        public void RedrawHealth()
        {
            if (targetHealth == null)
                return;

            var fillAmount = targetHealth.MaxHealth > 0 ? targetHealth.CurrentHealth / targetHealth.MaxHealth : 0;

            healthFillImage.fillAmount = fillAmount;
            maskFillImage.fillAmount = fillAmount;
        }

        public void ForceDisable()
        {
            isDisabled = true;

            fadeTweenCase.KillActive();

            healthBarCanvasGroup.alpha = 0;
            healthBarTransform.gameObject.SetActive(false);
            healthBarCanvasGroup.gameObject.SetActive(false);
        }

        public void Destroy()
        {
            isDisabled = true;

            Destroy(healthBarTransform.gameObject);
        }
    }

    public interface IHealth
    {
        float CurrentHealth { get; }
        float MaxHealth { get; }
    }
}
