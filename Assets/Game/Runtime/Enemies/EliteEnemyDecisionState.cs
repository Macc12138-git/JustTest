namespace JustTest.Game.Enemies
{
    public enum EliteEnemyDecisionState
    {
        Dormant = 0,
        Appearance = 1,
        Observe = 2,
        Reposition = 3,
        Attack = 4,
        AttackRecovery = 5,
        Controlled = 6,
        PlayerDefeated = 7,
        Dead = 8
    }
}
