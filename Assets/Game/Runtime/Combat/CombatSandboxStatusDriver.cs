using UnityEngine;

namespace JustTest.Game.Combat
{
    public sealed class CombatSandboxStatusDriver : MonoBehaviour
    {
        [SerializeField] private Transform sourceAnchor;
        [SerializeField] private DamageReceiver target;
        [SerializeField] private AttackDefinition unbalancedAttack;
        [SerializeField] private AttackDefinition airborneAttack;
        [SerializeField] private AttackDefinition stunnedAttack;
        [SerializeField] private CombatDebugConfig config;

        private AttackInstanceFactory attackFactory;
        private bool ready;

        private void Awake()
        {
            ready =
                sourceAnchor != null &&
                target != null &&
                unbalancedAttack != null &&
                airborneAttack != null &&
                stunnedAttack != null &&
                config != null;
            if (!ready)
            {
                Debug.LogError($"{nameof(CombatSandboxStatusDriver)} is missing an Inspector reference.", this);
                enabled = false;
                return;
            }

            attackFactory = new AttackInstanceFactory();
        }

        private void Update()
        {
            if (!ready)
            {
                return;
            }

            if (UnityEngine.Input.GetKeyDown(config.ApplyUnbalancedKey))
            {
                ApplyAttack(unbalancedAttack);
            }
            else if (UnityEngine.Input.GetKeyDown(config.ApplyAirborneKey))
            {
                ApplyAttack(airborneAttack);
            }
            else if (UnityEngine.Input.GetKeyDown(config.ApplyStunnedKey))
            {
                ApplyAttack(stunnedAttack);
            }
        }

        private void ApplyAttack(AttackDefinition definition)
        {
            if (config.BypassPostHitInvulnerabilityForStatusTests)
            {
                target.Invulnerability.RemoveSource(InvulnerabilitySource.PostHit);
            }

            int direction = target.transform.position.x >= sourceAnchor.position.x ? 1 : -1;
            AttackInstance attack = attackFactory.Create(
                GetInstanceID(),
                CombatFaction.Player,
                definition.Damage,
                direction,
                definition.HitReaction,
                definition.StatusApplication,
                definition.AllowFriendlyFire);
            attack.TryHit(target);
        }
    }
}
