namespace JustTest.Game.Enemies
{
    public enum RangedEnemyDecisionState
    {
        Dormant = 0,
        Observe = 1,
        Reposition = 2,
        Retreat = 3,
        WaitingForTurn = 4,
        Attack = 5,
        Controlled = 6,
        PlayerDefeated = 7,
        Dead = 8
    }
}
