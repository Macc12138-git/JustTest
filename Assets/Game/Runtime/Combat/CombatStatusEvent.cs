namespace JustTest.Game.Combat
{
    public readonly struct CombatStatusEvent
    {
        internal CombatStatusEvent(
            CombatStatusType statusType,
            int applicationId,
            float duration,
            bool wasRefresh)
        {
            StatusType = statusType;
            ApplicationId = applicationId;
            Duration = duration;
            WasRefresh = wasRefresh;
        }

        public CombatStatusType StatusType { get; }

        public int ApplicationId { get; }

        public float Duration { get; }

        public bool WasRefresh { get; }
    }
}
