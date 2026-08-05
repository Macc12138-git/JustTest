using JustTest.Game.Combat;
using JustTest.Game.Presentation;
using UnityEngine;

namespace JustTest.Game.Enemies
{
    [DefaultExecutionOrder(-30)]
    public sealed class MeleeEnemyRuntimeBindings : CombatEnemyRuntime2D
    {
        [SerializeField] private MeleeEnemyController2D controller;
        [SerializeField] private HealthComponent health;
        [SerializeField] private DamageReceiver damageReceiver;
        [SerializeField] private CombatReactionReceiver reactionReceiver;
        [SerializeField] private CombatStatusController statusController;
        [SerializeField] private MeleeEnemyMotor2D motor;
        [SerializeField] private EnemyAttackRunner attackRunner;
        [SerializeField] private Hurtbox2D hurtbox;
        [SerializeField] private Rigidbody2D body;

        [Header("Combat Feedback")]
        [SerializeField] private CombatHitFlash2D hitFlash;
        [SerializeField] private Transform impactAnchor;
        [SerializeField] private MonoBehaviour[] feedbackSources;
        [SerializeField] private CombatAttackRecoil2D attackRecoil;

        private bool sceneContextBound;
        private bool leased;
        private int leaseId;

        internal MeleeEnemyController2D Controller => controller;
        internal override DamageReceiver DamageReceiver => damageReceiver;
        internal override CombatHitFlash2D HitFlash => hitFlash;
        internal override Transform ImpactAnchor => impactAnchor;
        internal override MonoBehaviour[] FeedbackSources => feedbackSources;
        internal override CombatAttackRecoil2D AttackRecoil => attackRecoil;
        internal override int LeaseId => leaseId;
        internal override bool IsLeased => leased;
        internal override bool IsAlive => leased && health != null && !health.IsDead;

        private void Awake()
        {
            if (ValidateInternalReferences())
            {
                controller.Defeated += OnControllerDefeated;
                return;
            }

            Debug.LogError($"{nameof(MeleeEnemyRuntimeBindings)} is missing an Inspector reference.", this);
            enabled = false;
        }

        private void OnDestroy()
        {
            if (controller != null)
            {
                controller.Defeated -= OnControllerDefeated;
            }
        }

        internal override bool BindSceneContext(in CombatEnemySceneContext context)
        {
            if (!context.IsValid ||
                controller == null ||
                !controller.BindSceneContext(context, this))
            {
                return false;
            }

            sceneContextBound = true;
            return true;
        }

        internal override bool PrepareForSpawn(Vector3 position)
        {
            if (!sceneContextBound || leased || !ValidateInternalReferences())
            {
                return false;
            }

            leased = true;
            leaseId = leaseId == int.MaxValue ? 1 : leaseId + 1;
            ApplySpawnPose(position);
            hurtbox.enabled = false;
            controller.PrepareForEncounter();
            gameObject.SetActive(true);
            damageReceiver.ResetCombatState();
            ApplySpawnPose(position);
            hitFlash.ResetFlash();
            attackRecoil.ResetRecoil();
            return true;
        }

        internal override bool ActivateEncounter()
        {
            if (!leased || health.IsDead)
            {
                return false;
            }

            hurtbox.enabled = true;
            controller.ActivateEncounter();
            return true;
        }

        internal override void InterruptEncounter()
        {
            if (leased)
            {
                controller.InterruptEncounter();
            }
        }

        internal override void PrepareForPool()
        {
            if (!leased)
            {
                gameObject.SetActive(false);
                return;
            }

            controller.InterruptEncounter();
            hurtbox.enabled = false;
            damageReceiver.ResetCombatState();
            reactionReceiver.ResetReaction();
            statusController.ClearAll();
            hitFlash.ResetFlash();
            attackRecoil.ResetRecoil();
            attackRunner.CancelAttack();
            motor.ResetMotion();
            motor.SetControlEnabled(false);
            body.velocity = Vector2.zero;
            body.angularVelocity = 0f;
            controller.PrepareForEncounter();
            leased = false;
            gameObject.SetActive(false);
        }

        private bool ValidateInternalReferences()
        {
            if (controller == null ||
                health == null ||
                damageReceiver == null ||
                reactionReceiver == null ||
                statusController == null ||
                motor == null ||
                attackRunner == null ||
                hurtbox == null ||
                body == null ||
                hitFlash == null ||
                impactAnchor == null ||
                attackRecoil == null ||
                feedbackSources == null ||
                feedbackSources.Length == 0)
            {
                return false;
            }

            for (int index = 0; index < feedbackSources.Length; index++)
            {
                if (feedbackSources[index] == null)
                {
                    return false;
                }
            }

            return true;
        }

        private void ApplySpawnPose(Vector3 position)
        {
            transform.SetPositionAndRotation(position, Quaternion.identity);
            body.position = position;
            body.rotation = 0f;
            body.velocity = Vector2.zero;
            body.angularVelocity = 0f;
            Physics2D.SyncTransforms();
        }

        private void OnControllerDefeated(MeleeEnemyController2D defeatedController)
        {
            if (leased && defeatedController == controller)
            {
                RaiseDefeated();
            }
        }
    }
}
