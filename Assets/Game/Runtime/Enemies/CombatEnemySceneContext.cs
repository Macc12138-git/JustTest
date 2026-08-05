using JustTest.Game.Combat;
using JustTest.Game.Player;
using JustTest.Game.Run;
using UnityEngine;

namespace JustTest.Game.Enemies
{
    internal readonly struct CombatEnemySceneContext
    {
        internal CombatEnemySceneContext(
            Transform target,
            HealthComponent targetHealth,
            PlayerAttackRunner targetAttackRunner,
            PlayerRollController targetRollController,
            CombatPlatformController2D combatPlatform,
            CombatProjectilePool2D projectilePool)
        {
            Target = target;
            TargetHealth = targetHealth;
            TargetAttackRunner = targetAttackRunner;
            TargetRollController = targetRollController;
            CombatPlatform = combatPlatform;
            ProjectilePool = projectilePool;
        }

        internal Transform Target { get; }
        internal HealthComponent TargetHealth { get; }
        internal PlayerAttackRunner TargetAttackRunner { get; }
        internal PlayerRollController TargetRollController { get; }
        internal CombatPlatformController2D CombatPlatform { get; }
        internal CombatProjectilePool2D ProjectilePool { get; }

        internal bool IsValid =>
            Target != null &&
            TargetHealth != null &&
            TargetAttackRunner != null &&
            TargetRollController != null &&
            CombatPlatform != null;
    }
}
