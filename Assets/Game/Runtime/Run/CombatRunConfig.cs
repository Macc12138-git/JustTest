using UnityEngine;

namespace JustTest.Game.Run
{
    [CreateAssetMenu(fileName = "CombatRunConfig", menuName = "JustTest/Run/Combat Run Config")]
    public sealed class CombatRunConfig : ScriptableObject
    {
        [SerializeField] private bool allowRestartWhileActive = true;
        [SerializeField, Min(0f)] private float restartInputDelayAfterDefeat = 0.25f;
        [SerializeField, Min(0f)] private float sceneReloadDelay = 0.05f;

        internal bool AllowRestartWhileActive => allowRestartWhileActive;
        internal float RestartInputDelayAfterDefeat => restartInputDelayAfterDefeat;
        internal float SceneReloadDelay => sceneReloadDelay;

        internal bool IsValid =>
            IsFiniteNonNegative(restartInputDelayAfterDefeat) &&
            IsFiniteNonNegative(sceneReloadDelay);

        private void OnValidate()
        {
            restartInputDelayAfterDefeat = Mathf.Max(
                0f,
                SanitizeFinite(restartInputDelayAfterDefeat));
            sceneReloadDelay = Mathf.Max(0f, SanitizeFinite(sceneReloadDelay));
        }

        private static bool IsFiniteNonNegative(float value)
        {
            return value >= 0f && !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static float SanitizeFinite(float value)
        {
            return float.IsNaN(value) || float.IsInfinity(value) ? 0f : value;
        }
    }
}
