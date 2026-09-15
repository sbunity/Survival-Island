using UnityEngine;

namespace Watermelon
{
    public class WorldChangeZoneBehavior : MonoBehaviour, IGroundOpenable
    {
        [SerializeField] WorldChangeSpecialBehavior changeSpecialBehavior;

        [SerializeField] WorldTravelManifest travelManifest;

        private Vector3 defaultScale;

        public event SimpleCallback OnWorldChangeZoneEntered;

        private void Awake()
        {
            defaultScale = transform.localScale;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(PhysicsHelper.TAG_PLAYER))
            {
                if (!WorldController.HasTravelDestinations())
                    return;

                var gameUI = UIController.GetPage<UIGame>();

                gameUI.WorldTransitionPopUp.Show(OnDestinationSelected);
            }
        }

        private void OnDestinationSelected(WorldData destinationWorld)
        {
            if (destinationWorld == null)
            {
                Debug.LogError("Destination world is missing!", gameObject);

                return;
            }

            enabled = false;

            if (changeSpecialBehavior != null)
            {
                if (travelManifest != null)
                    changeSpecialBehavior.SetPassengers(travelManifest.CollectPassengers());

                changeSpecialBehavior.OnWorldChanged(() =>
                {
                    LoadWorld(destinationWorld);
                });
            }
            else
            {
                LoadWorld(destinationWorld);
            }
        }

        private void LoadWorld(WorldData destinationWorld)
        {
            OnWorldChangeZoneEntered?.Invoke();

            if (travelManifest != null)
                travelManifest.CommitTravel(destinationWorld.ID);

            GameController.LoadWorld(destinationWorld.ID);
        }

        public void OnGroundOpen(bool immediately)
        {
            gameObject.SetActive(true);

            if (immediately)
            {
                transform.localScale = defaultScale;
            }
            else
            {
                transform.localScale = Vector3.zero;
                transform.DOScale(defaultScale, 0.3f).SetEasing(Ease.Type.SineOut);
            }

            if (changeSpecialBehavior != null)
            {
                changeSpecialBehavior.OnGroundTileOpened(immediately);
            }
        }

        public void OnGroundHidden(bool immediately)
        {
            if (immediately)
            {
                gameObject.SetActive(false);
            }
            else
            {
                transform.DOScale(0, 0.3f).SetEasing(Ease.Type.SineOut).OnComplete(() => gameObject.SetActive(false));
            }
        }
    }
}
