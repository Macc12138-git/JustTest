using UnityEngine;

namespace JustTest.Game.Combat
{
    public sealed class CombatSandboxPoseReset2D : MonoBehaviour
    {
        [SerializeField] private DamageReceiver damageReceiver;
        [SerializeField] private Rigidbody2D body;

        private Vector2 initialPosition;
        private float initialRotation;
        private bool ready;

        private void Awake()
        {
            ready = damageReceiver != null && body != null;
            if (!ready)
            {
                Debug.LogError($"{nameof(CombatSandboxPoseReset2D)} is missing an Inspector reference.", this);
                enabled = false;
                return;
            }

            initialPosition = body.position;
            initialRotation = body.rotation;
        }

        private void OnEnable()
        {
            if (ready)
            {
                damageReceiver.CombatStateReset += ResetPose;
            }
        }

        private void OnDisable()
        {
            if (damageReceiver != null)
            {
                damageReceiver.CombatStateReset -= ResetPose;
            }
        }

        private void ResetPose()
        {
            body.position = initialPosition;
            body.rotation = initialRotation;
            body.velocity = Vector2.zero;
            body.angularVelocity = 0f;
            Physics2D.SyncTransforms();
        }
    }
}
