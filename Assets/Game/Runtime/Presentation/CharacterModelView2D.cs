using UnityEngine;

namespace JustTest.Game.Presentation
{
    public sealed class CharacterModelView2D : MonoBehaviour
    {
        [SerializeField] private GameObject[] whiteboxObjects;
        [SerializeField] private GameObject modelRoot;
        [SerializeField] private Transform facingRoot;
        [SerializeField] private Transform feedbackRoot;
        [SerializeField] private Transform weaponFeedbackPivot;
        [SerializeField] private Animator animator;
        [SerializeField] private WeaponVisual2D weaponVisual;
        [SerializeField] private bool artworkFacesRight = true;

        private bool[] whiteboxInitialActiveStates;
        private Vector3 facingBaseScale;
        private Vector3 feedbackBasePosition;
        private float weaponFeedbackBaseRotation;
        private int facingDirection = 1;
        private bool ready;

        internal Animator Animator => animator;
        internal WeaponVisual2D WeaponVisual => weaponVisual;
        internal int FacingDirection => facingDirection;
        internal bool IsModelVisible { get; private set; }

        private void Awake()
        {
            ready =
                whiteboxObjects != null &&
                whiteboxObjects.Length > 0 &&
                modelRoot != null &&
                facingRoot != null &&
                feedbackRoot != null &&
                weaponFeedbackPivot != null &&
                animator != null &&
                weaponVisual != null;
            if (ready)
            {
                whiteboxInitialActiveStates = new bool[whiteboxObjects.Length];
                for (int index = 0; index < whiteboxObjects.Length; index++)
                {
                    if (whiteboxObjects[index] == null)
                    {
                        ready = false;
                        break;
                    }

                    whiteboxInitialActiveStates[index] = whiteboxObjects[index].activeSelf;
                }
            }

            if (!ready)
            {
                Debug.LogError($"{nameof(CharacterModelView2D)} is missing an Inspector reference.", this);
                enabled = false;
                return;
            }

            facingBaseScale = facingRoot.localScale;
            feedbackBasePosition = feedbackRoot.localPosition;
            weaponFeedbackBaseRotation = weaponFeedbackPivot.localEulerAngles.z;
        }

        private void OnDisable()
        {
            ClearFeedbackPose();
        }

        internal void SetModelVisible(bool visible)
        {
            if (!ready)
            {
                return;
            }

            IsModelVisible = visible;
            modelRoot.SetActive(visible);
            for (int index = 0; index < whiteboxObjects.Length; index++)
            {
                whiteboxObjects[index].SetActive(
                    visible ? false : whiteboxInitialActiveStates[index]);
            }
        }

        internal void SetFacing(int direction)
        {
            if (!ready)
            {
                return;
            }

            facingDirection = direction < 0 ? -1 : 1;
            float artworkDirection = artworkFacesRight ? 1f : -1f;
            facingRoot.localScale = new Vector3(
                Mathf.Abs(facingBaseScale.x) * facingDirection * artworkDirection,
                facingBaseScale.y,
                facingBaseScale.z);
        }

        internal void SetFeedbackPose(float bodyOffsetX, float weaponRotation)
        {
            if (!ready)
            {
                return;
            }

            feedbackRoot.localPosition = feedbackBasePosition + new Vector3(bodyOffsetX, 0f, 0f);
            weaponFeedbackPivot.localRotation = Quaternion.Euler(
                0f,
                0f,
                weaponFeedbackBaseRotation + weaponRotation);
        }

        internal void ClearFeedbackPose()
        {
            if (!ready)
            {
                return;
            }

            feedbackRoot.localPosition = feedbackBasePosition;
            weaponFeedbackPivot.localRotation = Quaternion.Euler(
                0f,
                0f,
                weaponFeedbackBaseRotation);
        }
    }
}
