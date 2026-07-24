using UnityEngine;

namespace JustTest.Game.Presentation
{
    public sealed class CharacterVisualRig2D : MonoBehaviour
    {
        [SerializeField] private Transform rigRoot;
        [SerializeField] private Transform bodyRoot;
        [SerializeField] private Transform mainWeaponPivot;
        [SerializeField] private Transform mainWeaponVisual;
        [SerializeField] private SpriteRenderer mainWeaponRenderer;
        [SerializeField] private Transform offhandWeaponPivot;
        [SerializeField] private Transform offhandWeaponVisual;
        [SerializeField] private SpriteRenderer offhandWeaponRenderer;

        private Vector3 rigBaseScale;
        private Vector3 bodyBasePosition;
        private Vector3 bodyBaseScale;
        private float bodyBaseRotation;
        private Vector3 mainPivotBasePosition;
        private Vector3 mainPivotBaseScale;
        private float mainPivotBaseRotation;
        private Vector3 offhandPivotBasePosition;
        private Vector3 offhandPivotBaseScale;
        private float offhandPivotBaseRotation;
        private bool ready;

        private void Awake()
        {
            ready =
                rigRoot != null &&
                bodyRoot != null &&
                mainWeaponPivot != null &&
                mainWeaponVisual != null &&
                mainWeaponRenderer != null &&
                offhandWeaponPivot != null &&
                offhandWeaponVisual != null &&
                offhandWeaponRenderer != null;
            if (!ready)
            {
                Debug.LogError($"{nameof(CharacterVisualRig2D)} is missing an Inspector reference.", this);
                enabled = false;
                return;
            }

            rigBaseScale = rigRoot.localScale;
            bodyBasePosition = bodyRoot.localPosition;
            bodyBaseScale = bodyRoot.localScale;
            bodyBaseRotation = bodyRoot.localEulerAngles.z;
            mainPivotBasePosition = mainWeaponPivot.localPosition;
            mainPivotBaseScale = mainWeaponPivot.localScale;
            mainPivotBaseRotation = mainWeaponPivot.localEulerAngles.z;
            offhandPivotBasePosition = offhandWeaponPivot.localPosition;
            offhandPivotBaseScale = offhandWeaponPivot.localScale;
            offhandPivotBaseRotation = offhandWeaponPivot.localEulerAngles.z;
        }

        private void OnDisable()
        {
            if (ready)
            {
                ApplyPose(EvaluatedMotionPose2D.Identity, float.PositiveInfinity, 0f);
            }
        }

        internal void SetFacing(int direction)
        {
            if (!ready)
            {
                return;
            }

            int facing = direction < 0 ? -1 : 1;
            rigRoot.localScale = new Vector3(
                Mathf.Abs(rigBaseScale.x) * facing,
                rigBaseScale.y,
                rigBaseScale.z);
        }

        internal void ApplyWeaponStyle(CombatMotionProfile profile)
        {
            if (!ready || profile == null)
            {
                SetWeaponsVisible(false, false);
                return;
            }

            mainWeaponRenderer.enabled = true;
            mainWeaponRenderer.color = profile.MainWeaponColor;
            mainWeaponVisual.localPosition = new Vector3(
                profile.MainWeaponVisualOffset.x,
                profile.MainWeaponVisualOffset.y,
                mainWeaponVisual.localPosition.z);
            mainWeaponVisual.localScale = new Vector3(
                profile.MainWeaponSize.x,
                profile.MainWeaponSize.y,
                1f);

            offhandWeaponRenderer.enabled = profile.ShowOffhandWeapon;
            if (!profile.ShowOffhandWeapon)
            {
                return;
            }

            offhandWeaponRenderer.color = profile.OffhandWeaponColor;
            offhandWeaponVisual.localPosition = new Vector3(
                profile.OffhandWeaponVisualOffset.x,
                profile.OffhandWeaponVisualOffset.y,
                offhandWeaponVisual.localPosition.z);
            offhandWeaponVisual.localScale = new Vector3(
                profile.OffhandWeaponSize.x,
                profile.OffhandWeaponSize.y,
                1f);
        }

        internal void ApplyPose(
            in EvaluatedMotionPose2D pose,
            float blendSpeed,
            float deltaTime)
        {
            if (!ready)
            {
                return;
            }

            float blend = float.IsPositiveInfinity(blendSpeed)
                ? 1f
                : Mathf.Clamp01(Mathf.Max(0f, blendSpeed) * Mathf.Max(0f, deltaTime));
            ApplyTransform(
                bodyRoot,
                bodyBasePosition,
                bodyBaseRotation,
                bodyBaseScale,
                pose.BodyOffset,
                pose.BodyRotation,
                pose.BodyScale,
                blend);
            ApplyTransform(
                mainWeaponPivot,
                mainPivotBasePosition,
                mainPivotBaseRotation,
                mainPivotBaseScale,
                pose.MainWeaponOffset,
                pose.MainWeaponRotation,
                pose.MainWeaponScale,
                blend);
            ApplyTransform(
                offhandWeaponPivot,
                offhandPivotBasePosition,
                offhandPivotBaseRotation,
                offhandPivotBaseScale,
                pose.OffhandWeaponOffset,
                pose.OffhandWeaponRotation,
                pose.OffhandWeaponScale,
                blend);
        }

        private void SetWeaponsVisible(bool mainVisible, bool offhandVisible)
        {
            mainWeaponRenderer.enabled = mainVisible;
            offhandWeaponRenderer.enabled = offhandVisible;
        }

        private static void ApplyTransform(
            Transform target,
            Vector3 basePosition,
            float baseRotation,
            Vector3 baseScale,
            Vector2 offset,
            float rotation,
            Vector2 scale,
            float blend)
        {
            Vector3 targetPosition = basePosition + new Vector3(offset.x, offset.y, 0f);
            Vector3 targetScale = new Vector3(
                baseScale.x * scale.x,
                baseScale.y * scale.y,
                baseScale.z);
            target.localPosition = Vector3.LerpUnclamped(target.localPosition, targetPosition, blend);
            target.localRotation = Quaternion.Euler(
                0f,
                0f,
                Mathf.LerpAngle(target.localEulerAngles.z, baseRotation + rotation, blend));
            target.localScale = Vector3.LerpUnclamped(target.localScale, targetScale, blend);
        }
    }
}
