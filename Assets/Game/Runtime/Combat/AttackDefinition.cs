using UnityEngine;

namespace JustTest.Game.Combat
{
    [CreateAssetMenu(fileName = "AttackDefinition", menuName = "JustTest/Combat/Attack Definition")]
    public sealed class AttackDefinition : ScriptableObject
    {
        [SerializeField, Min(0.01f)] private float damage = 20f;
        [SerializeField, Min(0.01f)] private float windupDuration = 0.08f;
        [SerializeField, Min(0.01f)] private float activeDuration = 0.12f;
        [SerializeField, Min(0.01f)] private float recoveryDuration = 0.2f;
        [SerializeField, Min(0f)] private float inputBufferDuration = 0.1f;
        [SerializeField] private bool allowFriendlyFire;

        internal float Damage => damage;

        internal float WindupDuration => windupDuration;

        internal float ActiveDuration => activeDuration;

        internal float RecoveryDuration => recoveryDuration;

        internal float InputBufferDuration => inputBufferDuration;

        internal bool AllowFriendlyFire => allowFriendlyFire;

        private void OnValidate()
        {
            damage = Mathf.Max(0.01f, damage);
            windupDuration = Mathf.Max(0.01f, windupDuration);
            activeDuration = Mathf.Max(0.01f, activeDuration);
            recoveryDuration = Mathf.Max(0.01f, recoveryDuration);
            inputBufferDuration = Mathf.Max(0f, inputBufferDuration);
        }
    }
}
