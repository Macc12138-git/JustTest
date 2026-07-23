namespace JustTest.Game.Combat
{
    internal readonly struct CombatStatusSignal
    {
        internal CombatStatusSignal(
            CombatStatusController target,
            in CombatStatusEvent statusEvent)
        {
            Target = target;
            StatusEvent = statusEvent;
        }

        internal CombatStatusController Target { get; }

        internal CombatStatusEvent StatusEvent { get; }
    }
}
