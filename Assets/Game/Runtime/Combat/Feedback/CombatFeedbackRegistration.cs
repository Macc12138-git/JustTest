namespace JustTest.Game.Combat
{
    internal readonly struct CombatFeedbackRegistration
    {
        internal CombatFeedbackRegistration(int id)
        {
            Id = id;
        }

        internal int Id { get; }
        internal bool IsValid => Id != 0;
    }
}
