namespace JustTest.Game.Combat
{
    public readonly struct CombatStatusApplication
    {
        public CombatStatusApplication(CombatStatusType statusType, float duration)
        {
            StatusType = statusType;
            Duration = duration;
        }

        public CombatStatusType StatusType { get; }

        public float Duration { get; }

        public bool HasStatus => StatusType != CombatStatusType.None;

        internal bool IsValid
        {
            get
            {
                if (float.IsNaN(Duration) || float.IsInfinity(Duration))
                {
                    return false;
                }

                return StatusType == CombatStatusType.None
                    ? Duration == 0f
                    : IsSupportedStatus(StatusType) && Duration > 0f;
            }
        }

        private static bool IsSupportedStatus(CombatStatusType statusType)
        {
            return statusType == CombatStatusType.Unbalanced ||
                   statusType == CombatStatusType.Airborne ||
                   statusType == CombatStatusType.Stunned;
        }
    }
}
