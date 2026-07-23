using System;
using UnityEngine;

namespace JustTest.Game.Combat
{
    public sealed class DamageReceiver : MonoBehaviour
    {
        [SerializeField] private CombatantDefinition definition;
        [SerializeField] private HealthComponent health;
        [SerializeField] private InvulnerabilityController invulnerability;

        private bool ready;
        private DamageResolver damageResolver;

        public event Action<HitResult> HitResolved;

        public CombatFaction Faction => definition != null ? definition.Faction : CombatFaction.None;

        public HealthComponent Health => health;

        public InvulnerabilityController Invulnerability => invulnerability;

        private void Awake()
        {
            damageResolver = new DamageResolver(new CombatFactionRules());

            ready =
                definition != null &&
                definition.Faction != CombatFaction.None &&
                health != null &&
                invulnerability != null;
            if (!ready)
            {
                Debug.LogError($"{nameof(DamageReceiver)} is missing a combatant definition or component.", this);
                enabled = false;
                return;
            }

            health.Initialize(definition.MaximumHealth);
        }

        public HitResult ReceiveHit(in HitRequest request)
        {
            if (!ready)
            {
                HitResult invalidResult = new HitResult(
                    HitOutcome.InvalidRequest,
                    request.AttackInstanceId,
                    GetInstanceID(),
                    request.Damage,
                    0f,
                    health != null ? health.CurrentHealth : 0f,
                    false);
                HitResolved?.Invoke(invalidResult);
                return invalidResult;
            }

            HitResult result = damageResolver.Resolve(
                request,
                GetInstanceID(),
                definition.Faction,
                health.State,
                invulnerability.IsInvulnerable);

            if (result.WasApplied)
            {
                health.NotifyDamageApplied(result.AppliedDamage);
                if (!result.KilledTarget)
                {
                    invulnerability.Grant(
                        InvulnerabilitySource.PostHit,
                        definition.PostHitInvulnerabilityDuration);
                }
            }

            HitResolved?.Invoke(result);
            return result;
        }

        public void ResetCombatState()
        {
            if (!ready)
            {
                return;
            }

            invulnerability.ClearAll();
            health.RestoreToFull();
        }
    }
}
