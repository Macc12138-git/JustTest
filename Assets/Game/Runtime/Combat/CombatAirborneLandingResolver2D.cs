using UnityEngine;

namespace JustTest.Game.Combat
{
    public sealed class CombatAirborneLandingResolver2D : MonoBehaviour
    {
        [SerializeField] private CombatStatusController statusController;
        [SerializeField] private Rigidbody2D body;
        [SerializeField] private CombatAirborneLandingConfig config;

        private int airborneApplicationId;
        private float airborneStartedAt;
        private bool ready;

        private void Awake()
        {
            ready = statusController != null && body != null && config != null;
            if (ready)
            {
                return;
            }

            Debug.LogError($"{nameof(CombatAirborneLandingResolver2D)} is missing an Inspector reference.", this);
            enabled = false;
        }

        private void OnEnable()
        {
            if (!ready)
            {
                return;
            }

            statusController.StatusApplied += OnStatusApplied;
            statusController.StatusEnded += OnStatusEnded;
        }

        private void OnDisable()
        {
            if (statusController != null)
            {
                statusController.StatusApplied -= OnStatusApplied;
                statusController.StatusEnded -= OnStatusEnded;
            }

            airborneApplicationId = 0;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            TryEndAirborne(collision);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            TryEndAirborne(collision);
        }

        private void OnStatusApplied(CombatStatusEvent statusEvent)
        {
            if (statusEvent.StatusType != CombatStatusType.Airborne)
            {
                return;
            }

            airborneApplicationId = statusEvent.ApplicationId;
            airborneStartedAt = Time.time;
        }

        private void OnStatusEnded(CombatStatusEvent statusEvent)
        {
            if (statusEvent.StatusType == CombatStatusType.Airborne &&
                statusEvent.ApplicationId == airborneApplicationId)
            {
                airborneApplicationId = 0;
            }
        }

        private void TryEndAirborne(Collision2D collision)
        {
            if (airborneApplicationId == 0 ||
                !statusController.IsActive(CombatStatusType.Airborne) ||
                Time.time - airborneStartedAt < config.MinimumAirborneDuration ||
                body.velocity.y > 0f ||
                !IsGroundLayer(collision.gameObject.layer))
            {
                return;
            }

            for (int index = 0; index < collision.contactCount; index++)
            {
                if (collision.GetContact(index).normal.y < config.MinimumGroundNormalY)
                {
                    continue;
                }

                statusController.RemoveStatus(
                    CombatStatusType.Airborne,
                    airborneApplicationId);
                return;
            }
        }

        private bool IsGroundLayer(int layer)
        {
            return (config.GroundLayers.value & (1 << layer)) != 0;
        }
    }
}
