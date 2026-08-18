using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class ShipVisual : MonoBehaviour
    {
        [SerializeField] Transform playerHolderTransform;

        [SerializeField] Transform[] passengerHolderTransforms;

        [Space]
        [SerializeField] Animator shipAnimator;
        [SerializeField] string departureAnimationName = "Move";
        [SerializeField] ParticleSystem splashParticleSystem;

        [Space]
        [SerializeField, Min(0f)] float worldChangeEventDelay = 0.5f;
        public float WorldChangeEventDelay => worldChangeEventDelay;

        public int PassengerCapacity => passengerHolderTransforms != null ? passengerHolderTransforms.Length : 0;

        private void Awake()
        {
            if (shipAnimator == null)
                shipAnimator = GetComponent<Animator>();
        }

        public void BoardPlayer()
        {
            if (playerHolderTransform == null)
            {
                Debug.LogWarning("[Ship]: no player seat assigned, the player travels without riding the ship.", gameObject);

                return;
            }

            PlayerBehavior.GetBehavior().OnBoardRaft(playerHolderTransform);
        }

        public void SeatPassengers(IReadOnlyList<IRaftPassenger> passengers)
        {
            if (passengers == null)
                return;

            for (var i = 0; i < passengers.Count; i++)
            {
                var passenger = passengers[i];
                if (passenger == null)
                    continue;

                if (!passengerHolderTransforms.IsInRange(i) || passengerHolderTransforms[i] == null)
                {
                    Debug.LogWarning($"[Ship]: no seat for passenger #{i}, it travels without riding the ship.", gameObject);

                    continue;
                }

                passenger.OnBoardRaft(passengerHolderTransforms[i]);
            }
        }

        public void PlayDeparture()
        {
            if (shipAnimator != null && !string.IsNullOrEmpty(departureAnimationName))
                shipAnimator.Play(departureAnimationName);

            if (splashParticleSystem != null)
                splashParticleSystem.Play();
        }
    }
}
