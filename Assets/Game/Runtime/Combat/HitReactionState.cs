namespace JustTest.Game.Combat
{
    internal sealed class HitReactionState
    {
        internal bool IsActive { get; private set; }

        internal float RemainingDuration { get; private set; }

        internal HitReactionData Current { get; private set; }

        internal bool Start(in HitReactionData reaction)
        {
            if (!reaction.IsValid || !reaction.CausesReaction)
            {
                return false;
            }

            Current = reaction;
            RemainingDuration = reaction.HitStunDuration;
            IsActive = RemainingDuration > 0f;
            return true;
        }

        internal bool Tick(float deltaTime)
        {
            if (!IsActive || deltaTime <= 0f)
            {
                return false;
            }

            RemainingDuration -= deltaTime;
            if (RemainingDuration > 0f)
            {
                return false;
            }

            Clear();
            return true;
        }

        internal bool Clear()
        {
            bool changed = IsActive || RemainingDuration > 0f || Current.CausesReaction;
            IsActive = false;
            RemainingDuration = 0f;
            Current = default;
            return changed;
        }
    }
}
