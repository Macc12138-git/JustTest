using JustTest.Game.Combat;

namespace JustTest.Game.Weapons
{
    internal sealed class WeaponQteOpportunityState
    {
        private const int ValidCandidateMask = (1 << WeaponLoadoutState.Capacity) - 1;

        internal bool IsOpen => CandidateMask != 0;

        internal int TargetInstanceId { get; private set; }

        internal CombatStatusType StatusType { get; private set; }

        internal int ApplicationId { get; private set; }

        internal int CandidateMask { get; private set; }

        internal bool Open(
            int targetInstanceId,
            in CombatStatusEvent statusEvent,
            int candidateMask)
        {
            Clear();
            candidateMask &= ValidCandidateMask;
            if (targetInstanceId == 0 ||
                statusEvent.ApplicationId == 0 ||
                statusEvent.StatusType == CombatStatusType.None ||
                candidateMask == 0)
            {
                return false;
            }

            TargetInstanceId = targetInstanceId;
            StatusType = statusEvent.StatusType;
            ApplicationId = statusEvent.ApplicationId;
            CandidateMask = candidateMask;
            return true;
        }

        internal bool IsCandidate(int slotIndex)
        {
            return slotIndex >= 0 &&
                   slotIndex < WeaponLoadoutState.Capacity &&
                   (CandidateMask & 1 << slotIndex) != 0;
        }

        internal bool TrySelect(int slotIndex)
        {
            if (!IsCandidate(slotIndex))
            {
                return false;
            }

            Clear();
            return true;
        }

        internal bool TryEnd(
            int targetInstanceId,
            CombatStatusType statusType,
            int applicationId)
        {
            if (!IsOpen ||
                TargetInstanceId != targetInstanceId ||
                StatusType != statusType ||
                ApplicationId != applicationId)
            {
                return false;
            }

            Clear();
            return true;
        }

        internal bool Clear()
        {
            bool changed = IsOpen;
            TargetInstanceId = 0;
            StatusType = CombatStatusType.None;
            ApplicationId = 0;
            CandidateMask = 0;
            return changed;
        }
    }
}
