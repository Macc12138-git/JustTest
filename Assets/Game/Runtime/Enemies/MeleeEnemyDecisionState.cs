namespace JustTest.Game.Enemies
{
    public enum MeleeEnemyDecisionState
    {
        Dormant = 0,
        Observe = 1,
        Reposition = 2,
        WaitingForTurn = 3,
        Attack = 4,
        Controlled = 5,
        PlayerDefeated = 6,
        Dead = 7
    }
}
