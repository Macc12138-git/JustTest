namespace JustTest.Game.Enemies
{
    public enum MeleeEnemyDecisionState
    {
        Idle = 0,
        DirectChase = 1,
        PathChase = 2,
        AttackCooldown = 3,
        Attack = 4,
        Controlled = 5,
        Dead = 6
    }
}
