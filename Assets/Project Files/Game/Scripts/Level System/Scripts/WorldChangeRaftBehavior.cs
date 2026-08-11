using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [RequireComponent(typeof(Animator))]
    public sealed class WorldChangeRaftBehavior : WorldChangeSpecialBehavior
    {
        [SerializeField] Transform playerHolderTransform;

        [SerializeField] Transform[] passengerHolderTransforms;

        [SerializeField] ParticleSystem splashParticleSystem;

        [Space]
        [SerializeField] float worldChangeEventDelay = 0.5f;

        private Animator raftAnimator;

        private IReadOnlyList<IRaftPassenger> passengers;

        private void Awake()
        {
            raftAnimator = GetComponent<Animator>();
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
                transform.DOScale(1.0f, 0.3f);
            }
        }

        public override void SetPassengers(IReadOnlyList<IRaftPassenger> passengers)
        {
            this.passengers = passengers;
        }

        public override void OnWorldChanged(SimpleCallback worldChangeCallback)
        {
            PlayerBehavior playerBehavior = PlayerBehavior.GetBehavior();

            playerBehavior.OnBoardRaft(playerHolderTransform);

            SeatPassengers();

            raftAnimator.Play("Move");

            splashParticleSystem.Play();

            Tween.DelayedCall(worldChangeEventDelay, worldChangeCallback);
        }

        private void SeatPassengers()
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
                    Debug.LogWarning($"[Raft]: no seat for passenger #{i}, it travels without riding the raft.", gameObject);

                    continue;
                }

                passenger.OnBoardRaft(passengerHolderTransforms[i]);
            }

            passengers = null;
        }
    }
}
