namespace JustTest.Game.Combat
{
    internal sealed class PlayerAttackComboState
    {
        private int nextStepIndex;
        private float expiresAt = float.NegativeInfinity;
        private bool hasContinuation;

        internal bool IsContinuationQueued { get; private set; }

        internal int ResolveStartStep(float timestamp)
        {
            Expire(timestamp);
            return hasContinuation ? nextStepIndex : 0;
        }

        internal void MarkStepStarted()
        {
            hasContinuation = false;
            expiresAt = float.NegativeInfinity;
            IsContinuationQueued = false;
        }

        internal bool QueueContinuation()
        {
            if (IsContinuationQueued)
            {
                return false;
            }

            IsContinuationQueued = true;
            return true;
        }

        internal void MarkStepCompleted(
            int completedStepIndex,
            int stepCount,
            bool loopAfterFinalStep,
            float timestamp,
            float resetDelay)
        {
            IsContinuationQueued = false;
            int candidate = completedStepIndex + 1;
            if (candidate >= stepCount)
            {
                if (!loopAfterFinalStep || stepCount <= 0)
                {
                    Reset();
                    return;
                }

                candidate = 0;
            }

            nextStepIndex = candidate;
            expiresAt = timestamp + resetDelay;
            hasContinuation = true;
        }

        internal void Reset()
        {
            nextStepIndex = 0;
            expiresAt = float.NegativeInfinity;
            hasContinuation = false;
            IsContinuationQueued = false;
        }

        private void Expire(float timestamp)
        {
            if (hasContinuation && timestamp > expiresAt)
            {
                Reset();
            }
        }
    }
}
