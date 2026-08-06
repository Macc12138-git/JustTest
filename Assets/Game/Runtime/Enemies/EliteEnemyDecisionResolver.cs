using JustTest.Game.Combat;

namespace JustTest.Game.Enemies
{
    internal enum EliteEnemyDecision
    {
        Observe = 0,
        Reposition = 1,
        QuickSlash = 2,
        HeavySmash = 3,
        DashCleave = 4
    }

    internal readonly struct EliteEnemyDecisionParameters
    {
        internal EliteEnemyDecisionParameters(
            float attackVerticalTolerance,
            float quickAttackRange,
            float heavyAttackRange,
            float dashMinimumDistance,
            float dashMaximumDistance,
            float preferredMinimumDistance,
            float preferredMaximumDistance,
            float heavyOpportunityDuration)
        {
            AttackVerticalTolerance = attackVerticalTolerance;
            QuickAttackRange = quickAttackRange;
            HeavyAttackRange = heavyAttackRange;
            DashMinimumDistance = dashMinimumDistance;
            DashMaximumDistance = dashMaximumDistance;
            PreferredMinimumDistance = preferredMinimumDistance;
            PreferredMaximumDistance = preferredMaximumDistance;
            HeavyOpportunityDuration = heavyOpportunityDuration;
        }

        internal float AttackVerticalTolerance { get; }
        internal float QuickAttackRange { get; }
        internal float HeavyAttackRange { get; }
        internal float DashMinimumDistance { get; }
        internal float DashMaximumDistance { get; }
        internal float PreferredMinimumDistance { get; }
        internal float PreferredMaximumDistance { get; }
        internal float HeavyOpportunityDuration { get; }
    }

    internal readonly struct EliteEnemyDecisionInput
    {
        internal EliteEnemyDecisionInput(
            float horizontalDistance,
            float verticalDistance,
            AttackPhase targetAttackPhase,
            bool targetRolling,
            bool recoveryOpportunityAvailable,
            bool quickAttackReady,
            bool heavyAttackReady,
            bool dashAttackReady,
            bool passiveAttackDue,
            float closePresenceDuration)
        {
            HorizontalDistance = horizontalDistance;
            VerticalDistance = verticalDistance;
            TargetAttackPhase = targetAttackPhase;
            TargetRolling = targetRolling;
            RecoveryOpportunityAvailable = recoveryOpportunityAvailable;
            QuickAttackReady = quickAttackReady;
            HeavyAttackReady = heavyAttackReady;
            DashAttackReady = dashAttackReady;
            PassiveAttackDue = passiveAttackDue;
            ClosePresenceDuration = closePresenceDuration;
        }

        internal float HorizontalDistance { get; }
        internal float VerticalDistance { get; }
        internal AttackPhase TargetAttackPhase { get; }
        internal bool TargetRolling { get; }
        internal bool RecoveryOpportunityAvailable { get; }
        internal bool QuickAttackReady { get; }
        internal bool HeavyAttackReady { get; }
        internal bool DashAttackReady { get; }
        internal bool PassiveAttackDue { get; }
        internal float ClosePresenceDuration { get; }
    }

    internal sealed class EliteEnemyDecisionResolver
    {
        private readonly EliteEnemyDecisionParameters parameters;

        internal EliteEnemyDecisionResolver(in EliteEnemyDecisionParameters parameters)
        {
            this.parameters = parameters;
        }

        internal EliteEnemyDecision Resolve(in EliteEnemyDecisionInput input)
        {
            if (input.VerticalDistance > parameters.AttackVerticalTolerance ||
                input.TargetRolling ||
                input.TargetAttackPhase == AttackPhase.Windup ||
                input.TargetAttackPhase == AttackPhase.Active)
            {
                return EliteEnemyDecision.Observe;
            }

            if (input.RecoveryOpportunityAvailable &&
                input.QuickAttackReady &&
                input.HorizontalDistance <= parameters.QuickAttackRange)
            {
                return EliteEnemyDecision.QuickSlash;
            }

            if (input.DashAttackReady &&
                input.HorizontalDistance >= parameters.DashMinimumDistance &&
                input.HorizontalDistance <= parameters.DashMaximumDistance)
            {
                return EliteEnemyDecision.DashCleave;
            }

            if (input.HeavyAttackReady &&
                input.HorizontalDistance <= parameters.HeavyAttackRange &&
                input.ClosePresenceDuration >= parameters.HeavyOpportunityDuration)
            {
                return EliteEnemyDecision.HeavySmash;
            }

            if (input.PassiveAttackDue &&
                input.QuickAttackReady)
            {
                return input.HorizontalDistance <= parameters.QuickAttackRange
                    ? EliteEnemyDecision.QuickSlash
                    : EliteEnemyDecision.Reposition;
            }

            if (input.HorizontalDistance < parameters.PreferredMinimumDistance ||
                input.HorizontalDistance > parameters.PreferredMaximumDistance)
            {
                return EliteEnemyDecision.Reposition;
            }

            return EliteEnemyDecision.Observe;
        }
    }
}
