namespace JustTest.Game.Combat
{
    internal sealed class CombatStatusState
    {
        private const int StatusSlotCount = (int)CombatStatusType.Stunned + 1;

        private readonly ActiveStatus[] activeStatuses = new ActiveStatus[StatusSlotCount];
        private int nextApplicationId = 1;

        internal int ActiveCount { get; private set; }

        internal bool Apply(
            in CombatStatusApplication application,
            float timestamp,
            out CombatStatusEvent statusEvent)
        {
            statusEvent = default;
            if (!application.IsValid || !application.HasStatus || !IsFinite(timestamp))
            {
                return false;
            }

            int index = (int)application.StatusType;
            bool wasRefresh = activeStatuses[index].IsActive;
            int applicationId = NextApplicationId();
            activeStatuses[index] = new ActiveStatus(
                applicationId,
                application.Duration,
                timestamp + application.Duration);
            if (!wasRefresh)
            {
                ActiveCount++;
            }

            statusEvent = new CombatStatusEvent(
                application.StatusType,
                applicationId,
                application.Duration,
                wasRefresh);
            return true;
        }

        internal bool IsActive(CombatStatusType statusType)
        {
            return IsSupportedStatus(statusType) && activeStatuses[(int)statusType].IsActive;
        }

        internal float GetRemainingDuration(CombatStatusType statusType, float timestamp)
        {
            if (!IsActive(statusType) || !IsFinite(timestamp))
            {
                return 0f;
            }

            return UnityEngine.Mathf.Max(
                0f,
                activeStatuses[(int)statusType].ExpiresAt - timestamp);
        }

        internal int GetApplicationId(CombatStatusType statusType)
        {
            return IsActive(statusType)
                ? activeStatuses[(int)statusType].ApplicationId
                : 0;
        }

        internal bool TryExpireNext(float timestamp, out CombatStatusEvent statusEvent)
        {
            statusEvent = default;
            if (!IsFinite(timestamp))
            {
                return false;
            }

            for (int index = 1; index < activeStatuses.Length; index++)
            {
                ActiveStatus activeStatus = activeStatuses[index];
                if (!activeStatus.IsActive || timestamp < activeStatus.ExpiresAt)
                {
                    continue;
                }

                return RemoveAt(index, out statusEvent);
            }

            return false;
        }

        internal bool Remove(
            CombatStatusType statusType,
            int expectedApplicationId,
            out CombatStatusEvent statusEvent)
        {
            statusEvent = default;
            if (!IsActive(statusType))
            {
                return false;
            }

            ActiveStatus activeStatus = activeStatuses[(int)statusType];
            if (expectedApplicationId != 0 && activeStatus.ApplicationId != expectedApplicationId)
            {
                return false;
            }

            return RemoveAt((int)statusType, out statusEvent);
        }

        internal bool TryClearNext(out CombatStatusEvent statusEvent)
        {
            for (int index = 1; index < activeStatuses.Length; index++)
            {
                if (activeStatuses[index].IsActive)
                {
                    return RemoveAt(index, out statusEvent);
                }
            }

            statusEvent = default;
            return false;
        }

        private bool RemoveAt(int index, out CombatStatusEvent statusEvent)
        {
            ActiveStatus activeStatus = activeStatuses[index];
            if (!activeStatus.IsActive)
            {
                statusEvent = default;
                return false;
            }

            statusEvent = new CombatStatusEvent(
                (CombatStatusType)index,
                activeStatus.ApplicationId,
                activeStatus.Duration,
                false);
            activeStatuses[index] = default;
            ActiveCount--;
            return true;
        }

        private int NextApplicationId()
        {
            int applicationId = nextApplicationId;
            nextApplicationId = nextApplicationId == int.MaxValue ? 1 : nextApplicationId + 1;
            return applicationId;
        }

        private static bool IsSupportedStatus(CombatStatusType statusType)
        {
            return statusType >= CombatStatusType.Unbalanced && statusType <= CombatStatusType.Stunned;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private readonly struct ActiveStatus
        {
            internal ActiveStatus(int applicationId, float duration, float expiresAt)
            {
                ApplicationId = applicationId;
                Duration = duration;
                ExpiresAt = expiresAt;
                IsActive = true;
            }

            internal int ApplicationId { get; }

            internal float Duration { get; }

            internal float ExpiresAt { get; }

            internal bool IsActive { get; }
        }
    }
}
