namespace JustTest.Game.Run
{
    internal enum CombatWaveState
    {
        Idle = 0,
        Spawning = 1,
        WaitingForDefeat = 2,
        InterWaveDelay = 3,
        Completed = 4,
        Interrupted = 5
    }
}
