using UnityEngine;
using UnityEngine.Serialization;

namespace JustTest.Game.Run
{
    [CreateAssetMenu(fileName = "CombatRunConfig", menuName = "JustTest/Run/Combat Run Config")]
    public sealed class CombatRunConfig : ScriptableObject
    {
        [SerializeField] private bool allowRestartWhileActive = true;
        [FormerlySerializedAs("restartInputDelayAfterDefeat")]
        [SerializeField, Min(0f)] private float restartInputDelayAfterResult = 0.25f;
        [SerializeField, Min(0f)] private float sceneReloadDelay = 0.05f;

        internal bool AllowRestartWhileActive => allowRestartWhileActive;
        internal float RestartInputDelayAfterResult => restartInputDelayAfterResult;
        internal float SceneReloadDelay => sceneReloadDelay;

        internal bool IsValid =>
            IsFiniteNonNegative(restartInputDelayAfterResult) &&
            IsFiniteNonNegative(sceneReloadDelay);

        private void OnValidate()
        {
            restartInputDelayAfterResult = Mathf.Max(
                0f,
                SanitizeFinite(restartInputDelayAfterResult));
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
