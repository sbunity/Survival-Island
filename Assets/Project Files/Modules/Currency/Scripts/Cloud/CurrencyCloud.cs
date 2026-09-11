using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Watermelon
{
    public class CurrencyCloud : MonoBehaviour
    {
        private const string CLOUD_LAYER_NAME = "[CURRENCY CLOUD LAYER]";

        [SerializeField] Data[] floatingCloudCases;

        private static Dictionary<int, Data> floatingCloudLink = new Dictionary<int, Data>();
        private static List<Animation> activeClouds = new List<Animation>();

        private static RectTransform cloudLayer;
        public static RectTransform CloudLayer => cloudLayer;

        private void Start()
        {
            cloudLayer = CreateCloudLayer();

            for (int i = 0; i < floatingCloudCases.Length; i++)
            {
                RegisterCase(floatingCloudCases[i]);
            }

            Currency[] currencies = CurrencyController.Currencies;
            if (!currencies.IsNullOrEmpty())
            {
                foreach (var currency in currencies)
                {
                    Currency.FloatingCloudCase floatingCloudCase = currency.CurrencyCloud;
                    if (floatingCloudCase.AddToCloud)
                    {
                        CurrencyCloudSettings settings;

                        if (floatingCloudCase.SpecialPrefab != null)
                        {
                            settings = new CurrencyCloudSettings(currency.CurrencyType.ToString(), floatingCloudCase.SpecialPrefab);
                        }
                        else
                        {
                            settings = new CurrencyCloudSettings(currency.CurrencyType.ToString(), currency.Icon, new Vector2(100, 100));
                        }

                        settings.SetAudio(floatingCloudCase.AppearAudioClip, floatingCloudCase.CollectAudioClip);

                        RegisterCase(settings);
                    }
                }
            }
        }

        private void OnDestroy()
        {
            Unload();

            foreach (Data floatingCloudData in floatingCloudLink.Values)
            {
                floatingCloudData.Destroy();
            }

            floatingCloudLink.Clear();

            if (cloudLayer != null)
            {
                Destroy(cloudLayer.gameObject);

                cloudLayer = null;
            }
        }

        private static RectTransform CreateCloudLayer()
        {
            var mainCanvas = UIController.MainCanvas;
            if (mainCanvas == null)
            {
                Debug.LogError("[Currency Cloud]: Main canvas isn't initialized, cloud elements have no layer to fly in.");

                return null;
            }

            var layerObject = new GameObject(CLOUD_LAYER_NAME);

            var layerTransform = layerObject.AddComponent<RectTransform>();
            layerTransform.SetParent(mainCanvas.transform, false);
            layerTransform.anchorMin = Vector2.zero;
            layerTransform.anchorMax = Vector2.one;
            layerTransform.offsetMin = Vector2.zero;
            layerTransform.offsetMax = Vector2.zero;
            layerTransform.localScale = Vector3.one;
            layerTransform.SetAsLastSibling();

            var layerCanvasGroup = layerObject.AddComponent<CanvasGroup>();
            layerCanvasGroup.interactable = false;
            layerCanvasGroup.blocksRaycasts = false;

            return layerTransform;
        }

        public static void Unload()
        {
            if (activeClouds.Count == 0)
                return;

            var cloudsToAbort = activeClouds.ToArray();

            activeClouds.Clear();

            for (var i = 0; i < cloudsToAbort.Length; i++)
            {
                cloudsToAbort[i].Abort();
            }
        }

        public static void RegisterCase(CurrencyCloudSettings settings)
        {
            int cloudHash = settings.Name.GetHashCode();

            if (floatingCloudLink.ContainsKey(cloudHash))
            {
                Debug.LogError($"Cloud {settings.Name} already registered!");
                return;
            }

            var data = new Data(settings);
            data.Init(cloudLayer);

            floatingCloudLink.Add(cloudHash, data);
        }

        public static void RegisterCase(Data floatingCloudCase)
        {
            int cloudHash = floatingCloudCase.Name.GetHashCode();

            if (floatingCloudLink.ContainsKey(cloudHash))
            {
                Debug.LogError($"Cloud {floatingCloudCase.Name} already registered!");
                return;
            }

            floatingCloudCase.Init(cloudLayer);

            floatingCloudLink.Add(cloudHash, floatingCloudCase);
        }

        public static void SpawnCurrency(string key, RectTransform rectTransform, RectTransform targetTransform, int elementsAmount, string text, SimpleCallback onCurrencyHittedTarget = null)
        {
            SpawnCurrency(key.GetHashCode(), rectTransform, targetTransform, elementsAmount, text, onCurrencyHittedTarget);
        }

        public static void SpawnCurrency(int hash, RectTransform rectTransform, RectTransform targetTransform, int elementsAmount, string text, SimpleCallback onCurrencyHittedTarget = null)
        {
            if (!floatingCloudLink.ContainsKey(hash))
            {
                Debug.LogError($"Cloud with hash {hash} isn't registered!");
                return;
            }

            var animation = new Animation(floatingCloudLink[hash], rectTransform, targetTransform, elementsAmount, onCurrencyHittedTarget);

            activeClouds.Add(animation);

            animation.PlayAnimation();
        }

        public static void OnAnimationFinished(Animation animation)
        {
            activeClouds.Remove(animation);
        }

        [System.Serializable]
        public class Data
        {
            [SerializeField] string name;
            public string Name => name;

            [SerializeField] GameObject prefab;
            public GameObject Prefab => prefab;

            [SerializeField] AudioClip appearAudioClip;
            public AudioClip AppearAudioClip => appearAudioClip;

            [SerializeField] AudioClipHandler appearClipHandler;
            public AudioClipHandler AppearClipHandler => appearClipHandler;

            [SerializeField] AudioClip collectAudioClip;
            public AudioClip CollectAudioClip => collectAudioClip;

            [SerializeField] AudioClipHandler collectClipHandler;
            public AudioClipHandler CollectClipHandler => collectClipHandler;

            [Space]
            [SerializeField] float cloudRadius;
            public float CloudRadius => cloudRadius;

            private Pool pool;
            public Pool Pool => pool;

            public Data(CurrencyCloudSettings settings)
            {
                name = settings.Name;
                prefab = settings.Prefab;
                cloudRadius = settings.CloudRadius;
                appearAudioClip = settings.AppearAudioClip;
                collectAudioClip = settings.CollectAudioClip;
                appearClipHandler = new AudioClipHandler(AudioType.Sound, 1.0f);
                collectClipHandler = new AudioClipHandler(AudioType.Sound, 1.0f);
            }

            public void Init(Transform objectsContainer)
            {
                pool = new Pool(prefab, "CurrencyCloud_" + name, objectsContainer);
            }

            public void Destroy()
            {
                PoolManager.DestroyPool(pool);
                pool = null;
            }
        }

        public class Animation
        {
            private const float FADE_IN_DURATION = 0.2f;
            private const float SCATTER_DURATION_MIN = 0.6f;
            private const float SCATTER_DURATION_MAX = 0.8f;
            private const float GATHER_DELAY = 0.1f;
            private const float GATHER_DURATION = 0.5f;
            private const float GATHER_SCALE = 0.3f;
            private const float TARGET_PUNCH_SCALE = 1.2f;
            private const float TARGET_PUNCH_DURATION = 0.15f;
            private const float TARGET_RESET_DURATION = 0.1f;

            private readonly Data floatingCloudData;
            private readonly RectTransform spawnTransform;
            private readonly RectTransform targetTransform;
            private readonly Transform flightParent;
            private readonly int elementsAmount;
            private readonly SimpleCallback onCurrencyHittedTarget;

            private readonly List<Element> flyingElements = new List<Element>();

            private TweenCaseCollection tweenCaseCollection;
            private TweenCase targetPunchCase;

            private bool currencyHittedTarget;
            private bool isReleased;

            public bool IsReleased => isReleased;

            public Animation(Data floatingCloudData, RectTransform rectTransform, RectTransform targetTransform, int elementsAmount, SimpleCallback onCurrencyHittedTarget)
            {
                this.floatingCloudData = floatingCloudData;
                this.spawnTransform = rectTransform;
                this.targetTransform = targetTransform;
                this.elementsAmount = elementsAmount;
                this.onCurrencyHittedTarget = onCurrencyHittedTarget;

                flightParent = cloudLayer != null ? cloudLayer : targetTransform.parent;
            }

            public void PlayAnimation()
            {
                tweenCaseCollection = Tween.BeginTweenCaseCollection();

                if (floatingCloudData.AppearAudioClip != null)
                    floatingCloudData.AppearClipHandler.Play(floatingCloudData.AppearAudioClip);

                for (var i = 0; i < elementsAmount; i++)
                {
                    var element = TakeElement();
                    if (element == null)
                        continue;

                    flyingElements.Add(element);

                    PlayScatter(element);
                }

                Tween.EndTweenCaseCollection();

                if (flyingElements.Count == 0)
                {
                    OnTweensFinished();

                    return;
                }

                tweenCaseCollection.OnComplete(OnTweensFinished);
            }

            public void Abort()
            {
                tweenCaseCollection?.Kill();

                Release();
            }

            public void Release()
            {
                if (isReleased)
                    return;

                isReleased = true;

                for (var i = 0; i < flyingElements.Count; i++)
                {
                    ReturnToPool(flyingElements[i]);
                }

                flyingElements.Clear();

                ResetTarget();
            }

            private void OnTweensFinished()
            {
                Release();

                CurrencyCloud.OnAnimationFinished(this);
            }

            private Element TakeElement()
            {
                var elementObject = floatingCloudData.Pool.GetPooledObject();
                if (elementObject == null)
                    return null;

                var elementTransform = (RectTransform)elementObject.transform;
                elementTransform.SetParent(flightParent);
                elementTransform.position = spawnTransform.position;
                elementTransform.localRotation = Quaternion.identity;
                elementTransform.localScale = Vector3.one;

                var element = new Element(elementObject, elementTransform, elementObject.GetComponent<Image>());

                if (element.Image != null)
                    element.Image.color = Color.white.SetAlpha(0);

                return element;
            }

            private void PlayScatter(Element element)
            {
                if (element.Image != null)
                    element.Image.DOFade(1, FADE_IN_DURATION, unscaledTime: true);

                var scatterPosition = element.RectTransform.anchoredPosition + Random.insideUnitCircle * floatingCloudData.CloudRadius;
                var scatterDuration = Random.Range(SCATTER_DURATION_MIN, SCATTER_DURATION_MAX);

                element.RectTransform.DOAnchoredPosition(scatterPosition, scatterDuration, unscaledTime: true).SetEasing(Ease.Type.CubicOut).OnComplete(delegate
                {
                    ScheduleGather(element);
                });
            }

            private void ScheduleGather(Element element)
            {
                tweenCaseCollection.AddTween(Tween.DelayedCall(GATHER_DELAY, delegate
                {
                    PlayGather(element);
                }, unscaledTime: true));
            }

            private void PlayGather(Element element)
            {
                tweenCaseCollection.AddTween(element.RectTransform.DOScale(GATHER_SCALE, GATHER_DURATION, unscaledTime: true).SetEasing(Ease.Type.ExpoIn));

                var gatherPosition = targetTransform != null ? targetTransform.position : element.RectTransform.position;

                tweenCaseCollection.AddTween(element.RectTransform.DOMove(gatherPosition, GATHER_DURATION, unscaledTime: true).SetEasing(Ease.Type.SineIn).OnComplete(delegate
                {
                    OnElementReachedTarget(element);
                }));
            }

            private void OnElementReachedTarget(Element element)
            {
                if (!currencyHittedTarget)
                {
                    currencyHittedTarget = true;

                    onCurrencyHittedTarget?.Invoke();
                }

                PunchTarget();

                flyingElements.Remove(element);

                ReturnToPool(element);
            }

            private void PunchTarget()
            {
                if (targetTransform == null)
                    return;

                if (floatingCloudData.CollectAudioClip != null)
                    floatingCloudData.CollectClipHandler.Play(floatingCloudData.CollectAudioClip);

                targetPunchCase.KillActive();

                targetPunchCase = targetTransform.DOScale(TARGET_PUNCH_SCALE, TARGET_PUNCH_DURATION, unscaledTime: true).OnComplete(delegate
                {
                    targetPunchCase = targetTransform.DOScale(1.0f, TARGET_RESET_DURATION, unscaledTime: true);

                    tweenCaseCollection.AddTween(targetPunchCase);
                });

                tweenCaseCollection.AddTween(targetPunchCase);
            }

            private void ResetTarget()
            {
                if (targetPunchCase == null)
                    return;

                targetPunchCase.KillActive();
                targetPunchCase = null;

                if (targetTransform != null)
                    targetTransform.localScale = Vector3.one;
            }

            private void ReturnToPool(Element element)
            {
                floatingCloudData.Pool?.ReturnToPool(element.PooledObject);
            }

            private class Element
            {
                public GameObject PooledObject { get; }
                public RectTransform RectTransform { get; }
                public Image Image { get; }

                public Element(GameObject elementObject, RectTransform rectTransform, Image image)
                {
                    PooledObject = elementObject;
                    RectTransform = rectTransform;
                    Image = image;
                }
            }
        }
    }
}
