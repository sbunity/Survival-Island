using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public sealed class WorldChangeShipBehavior : WorldChangeSpecialBehavior
    {
        [SerializeField, Min(0f)] float appearDuration = 0.3f;

        private ShipVisual[] shipVisuals;

        private IReadOnlyList<IRaftPassenger> passengers;

        private void Awake()
        {
            shipVisuals = GetComponentsInChildren<ShipVisual>(true);
        }

        public override void OnGroundTileOpened(bool immediately)
        {
            gameObject.SetActive(true);

            if (immediately)
            {
                transform.localScale = Vector3.one;
            }
            else
            {
                transform.localScale = Vector3.zero;
                transform.DOScale(1.0f, appearDuration);
            }
        }

        public override void SetPassengers(IReadOnlyList<IRaftPassenger> passengers)
        {
            this.passengers = passengers;
        }

        public override void OnWorldChanged(SimpleCallback worldChangeCallback)
        {
            var shipVisual = GetActiveVisual();

            if (shipVisual == null)
            {
                Debug.LogError("[Ship]: no active ShipVisual found, travelling without the departure animation.", gameObject);

                passengers = null;

                worldChangeCallback?.Invoke();

                return;
            }

            shipVisual.BoardPlayer();
            shipVisual.SeatPassengers(passengers);

            passengers = null;

            shipVisual.PlayDeparture();

            Tween.DelayedCall(shipVisual.WorldChangeEventDelay, worldChangeCallback);
        }

        private ShipVisual GetActiveVisual()
        {
            if (shipVisuals.IsNullOrEmpty())
                return null;

            for (var i = 0; i < shipVisuals.Length; i++)
            {
                var shipVisual = shipVisuals[i];

                if (shipVisual != null && shipVisual.gameObject.activeInHierarchy)
                    return shipVisual;
            }

            return null;
        }
    }
}
