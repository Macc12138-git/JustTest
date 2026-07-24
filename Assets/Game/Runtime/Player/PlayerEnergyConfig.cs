using UnityEngine;

namespace JustTest.Game.Player
{
    [CreateAssetMenu(fileName = "PlayerEnergyConfig", menuName = "JustTest/Player/Energy Config")]
    public sealed class PlayerEnergyConfig : ScriptableObject
    {
        [SerializeField, Min(0.01f)] private float maximumEnergy = 100f;
        [SerializeField, Min(0f)] private float startingEnergy = 100f;
        [SerializeField, Min(0f)] private float automaticRecoveryPerSecond = 8f;
        [SerializeField, Min(0f)] private float recoveryPerHit = 12f;

        internal float MaximumEnergy => maximumEnergy;
        internal float StartingEnergy => startingEnergy;
        internal float AutomaticRecoveryPerSecond => automaticRecoveryPerSecond;
        internal float RecoveryPerHit => recoveryPerHit;

        internal bool IsValid =>
            IsFinitePositive(maximumEnergy) &&
            IsFiniteNonNegative(startingEnergy) &&
            startingEnergy <= maximumEnergy &&
            IsFiniteNonNegative(automaticRecoveryPerSecond) &&
            IsFiniteNonNegative(recoveryPerHit);

        private void OnValidate()
        {
            maximumEnergy = Mathf.Max(0.01f, SanitizeFinite(maximumEnergy));
            startingEnergy = Mathf.Clamp(SanitizeFinite(startingEnergy), 0f, maximumEnergy);
            automaticRecoveryPerSecond = Mathf.Max(
                0f,
                SanitizeFinite(automaticRecoveryPerSecond));
            recoveryPerHit = Mathf.Max(0f, SanitizeFinite(recoveryPerHit));
        }

        private static bool IsFinitePositive(float value)
        {
            return value > 0f && IsFinite(value);
        }

        private static bool IsFiniteNonNegative(float value)
        {
            return value >= 0f && IsFinite(value);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static float SanitizeFinite(float value)
        {
            return IsFinite(value) ? value : 0f;
        }
    }
}
