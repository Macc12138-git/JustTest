using JustTest.Game.Combat;
using UnityEngine;

namespace JustTest.Game.Presentation
{
    [CreateAssetMenu(fileName = "CombatMotionProfile", menuName = "JustTest/Presentation/Combat Motion Profile")]
    public sealed class CombatMotionProfile : ScriptableObject
    {
        [Header("Weapon Style")]
        [SerializeField] private Color mainWeaponColor = Color.white;
        [SerializeField] private Vector2 mainWeaponSize = new Vector2(0.8f, 0.14f);
        [SerializeField] private Vector2 mainWeaponVisualOffset = new Vector2(0.4f, 0f);
        [SerializeField] private bool showOffhandWeapon;
        [SerializeField] private Color offhandWeaponColor = Color.white;
        [SerializeField] private Vector2 offhandWeaponSize = new Vector2(0.5f, 0.12f);
        [SerializeField] private Vector2 offhandWeaponVisualOffset = new Vector2(0.25f, 0f);

        [Header("Attack Poses")]
        [SerializeField] private MotionPose2D windupPose = new MotionPose2D();
        [SerializeField] private MotionPose2D activePose = new MotionPose2D();
        [SerializeField] private MotionPose2D recoveryPose = new MotionPose2D();
        [SerializeField] private AnimationCurve phaseCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField, Range(0.05f, 0.95f)] private float recoveryPosePeak = 0.35f;

        internal Color MainWeaponColor => mainWeaponColor;
        internal Vector2 MainWeaponSize => mainWeaponSize;
        internal Vector2 MainWeaponVisualOffset => mainWeaponVisualOffset;
        internal bool ShowOffhandWeapon => showOffhandWeapon;
        internal Color OffhandWeaponColor => offhandWeaponColor;
        internal Vector2 OffhandWeaponSize => offhandWeaponSize;
        internal Vector2 OffhandWeaponVisualOffset => offhandWeaponVisualOffset;

        internal EvaluatedMotionPose2D Evaluate(AttackPhase phase, float phaseProgress)
        {
            float progress = phaseCurve != null
                ? Mathf.Clamp01(phaseCurve.Evaluate(Mathf.Clamp01(phaseProgress)))
                : Mathf.Clamp01(phaseProgress);
            EvaluatedMotionPose2D windup = windupPose.Evaluate();
            EvaluatedMotionPose2D active = activePose.Evaluate();
            EvaluatedMotionPose2D recovery = recoveryPose.Evaluate();

            switch (phase)
            {
                case AttackPhase.Windup:
                    return EvaluatedMotionPose2D.Lerp(
                        EvaluatedMotionPose2D.Identity,
                        windup,
                        progress);
                case AttackPhase.Active:
                    return EvaluatedMotionPose2D.Lerp(windup, active, progress);
                case AttackPhase.Recovery:
                    if (progress <= recoveryPosePeak)
                    {
                        return EvaluatedMotionPose2D.Lerp(
                            active,
                            recovery,
                            progress / recoveryPosePeak);
                    }

                    return EvaluatedMotionPose2D.Lerp(
                        recovery,
                        EvaluatedMotionPose2D.Identity,
                        (progress - recoveryPosePeak) / (1f - recoveryPosePeak));
                default:
                    return EvaluatedMotionPose2D.Identity;
            }
        }

        private void OnValidate()
        {
            mainWeaponSize = SanitizeSize(mainWeaponSize);
            offhandWeaponSize = SanitizeSize(offhandWeaponSize);
            recoveryPosePeak = Mathf.Clamp(recoveryPosePeak, 0.05f, 0.95f);
        }

        private static Vector2 SanitizeSize(Vector2 size)
        {
            return new Vector2(Mathf.Max(0.01f, size.x), Mathf.Max(0.01f, size.y));
        }
    }
}
