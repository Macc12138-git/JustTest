using System;
using UnityEngine;

namespace JustTest.Game.Combat
{
    public sealed class CombatStatusController : MonoBehaviour
    {
        [SerializeField] private DamageReceiver damageReceiver;
        [SerializeField] private HealthComponent health;
        [SerializeField] private CombatStatusEventChannel eventChannel;

        private readonly CombatStatusState state = new CombatStatusState();
        private bool ready;

        public event Action<CombatStatusEvent> StatusApplied;

        public event Action<CombatStatusEvent> StatusEnded;

        public int ActiveStatusCount => state.ActiveCount;

        private void Awake()
        {
            ready = damageReceiver != null && health != null && eventChannel != null;
            if (ready)
            {
                return;
            }

            Debug.LogError($"{nameof(CombatStatusController)} is missing an Inspector reference.", this);
            enabled = false;
        }

        private void OnEnable()
        {
            if (!ready)
            {
                return;
            }

            damageReceiver.HitProcessed += OnHitProcessed;
            damageReceiver.CombatStateReset += ClearAll;
            health.Died += ClearAll;
        }

        private void Update()
        {
            while (state.TryExpireNext(Time.time, out CombatStatusEvent statusEvent))
            {
                PublishStatusEnded(statusEvent);
            }
        }

        private void OnDisable()
        {
            if (damageReceiver != null)
            {
                damageReceiver.HitProcessed -= OnHitProcessed;
                damageReceiver.CombatStateReset -= ClearAll;
            }

            if (health != null)
            {
                health.Died -= ClearAll;
            }

            ClearAll();
        }

        public bool IsActive(CombatStatusType statusType)
        {
            return state.IsActive(statusType);
        }

        public float GetRemainingDuration(CombatStatusType statusType)
        {
            return state.GetRemainingDuration(statusType, Time.time);
        }

        internal int GetApplicationId(CombatStatusType statusType)
        {
            return state.GetApplicationId(statusType);
        }

        internal bool RemoveStatus(CombatStatusType statusType, int expectedApplicationId = 0)
        {
            if (!state.Remove(statusType, expectedApplicationId, out CombatStatusEvent statusEvent))
            {
                return false;
            }

            PublishStatusEnded(statusEvent);
            return true;
        }

        internal void ClearAll()
        {
            while (state.TryClearNext(out CombatStatusEvent statusEvent))
            {
                PublishStatusEnded(statusEvent);
            }
        }

        private void OnHitProcessed(HitResolution resolution)
        {
            if (!resolution.Result.WasApplied ||
                resolution.Result.KilledTarget ||
                !state.Apply(resolution.Request.Status, Time.time, out CombatStatusEvent statusEvent))
            {
                return;
            }

            StatusApplied?.Invoke(statusEvent);
            eventChannel.RaiseStatusApplied(this, statusEvent);
        }

        private void PublishStatusEnded(in CombatStatusEvent statusEvent)
        {
            StatusEnded?.Invoke(statusEvent);
            eventChannel.RaiseStatusEnded(this, statusEvent);
        }
    }
}
