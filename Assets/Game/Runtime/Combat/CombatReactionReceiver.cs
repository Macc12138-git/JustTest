using System;
using UnityEngine;

namespace JustTest.Game.Combat
{
    public sealed class CombatReactionReceiver : MonoBehaviour
    {
        [SerializeField] private DamageReceiver damageReceiver;

        private readonly HitReactionState state = new HitReactionState();
        private bool ready;

        public event Action<HitReactionData> ReactionStarted;

        public event Action ReactionEnded;

        public bool IsReacting => state.IsActive;

        public float RemainingDuration => state.RemainingDuration;

        public HitReactionData CurrentReaction => state.Current;

        private void Awake()
        {
            ready = damageReceiver != null;
            if (ready)
            {
                return;
            }

            Debug.LogError($"{nameof(CombatReactionReceiver)} is missing an Inspector reference.", this);
            enabled = false;
        }

        private void OnEnable()
        {
            if (!ready)
            {
                return;
            }

            damageReceiver.HitProcessed += OnHitProcessed;
            damageReceiver.CombatStateReset += ResetReaction;
        }

        private void Update()
        {
            if (state.Tick(Time.deltaTime))
            {
                ReactionEnded?.Invoke();
            }
        }

        private void OnDisable()
        {
            if (damageReceiver != null)
            {
                damageReceiver.HitProcessed -= OnHitProcessed;
                damageReceiver.CombatStateReset -= ResetReaction;
            }

            ResetReaction();
        }

        internal void ResetReaction()
        {
            if (state.Clear())
            {
                ReactionEnded?.Invoke();
            }
        }

        private void OnHitProcessed(HitResolution resolution)
        {
            if (!resolution.Result.WasApplied ||
                resolution.Result.KilledTarget ||
                !state.Start(resolution.Request.Reaction))
            {
                return;
            }

            ReactionStarted?.Invoke(state.Current);
            if (!state.IsActive)
            {
                state.Clear();
                ReactionEnded?.Invoke();
            }
        }
    }
}
