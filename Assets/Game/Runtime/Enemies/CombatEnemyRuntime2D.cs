using System;
using JustTest.Game.Combat;
using JustTest.Game.Presentation;
using UnityEngine;

namespace JustTest.Game.Enemies
{
    public abstract class CombatEnemyRuntime2D : MonoBehaviour
    {
        internal event Action<CombatEnemyRuntime2D> Defeated;

        internal int ParticipantId => GetInstanceID();
        internal abstract DamageReceiver DamageReceiver { get; }
        internal abstract Collider2D TargetingCollider { get; }
        internal abstract CombatHitFlash2D HitFlash { get; }
        internal abstract Transform ImpactAnchor { get; }
        internal abstract MonoBehaviour[] FeedbackSources { get; }
        internal abstract CombatAttackRecoil2D AttackRecoil { get; }
        internal abstract int LeaseId { get; }
        internal abstract bool IsLeased { get; }
        internal abstract bool IsAlive { get; }

        internal abstract bool BindSceneContext(in CombatEnemySceneContext context);
        internal abstract bool PrepareForSpawn(Vector3 position);
        internal abstract bool ActivateEncounter();
        internal abstract void InterruptEncounter();
        internal abstract void PrepareForPool();

        protected void RaiseDefeated()
        {
            Defeated?.Invoke(this);
        }
    }
}
