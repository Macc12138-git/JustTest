using System;

namespace JustTest.Game.Combat
{
    internal sealed class AttackTimeline
    {
        private const float TimeComparisonTolerance = 0.000001f;

        private readonly float windupDuration;
        private readonly float activeDuration;
        private readonly float recoveryDuration;

        private float phaseElapsed;

        internal AttackTimeline(
            float windupDuration,
            float activeDuration,
            float recoveryDuration)
        {
            if (windupDuration <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(windupDuration));
            }

            if (activeDuration <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(activeDuration));
            }

            if (recoveryDuration <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(recoveryDuration));
            }

            this.windupDuration = windupDuration;
            this.activeDuration = activeDuration;
            this.recoveryDuration = recoveryDuration;
        }

        internal event Action<AttackPhase, AttackPhase> PhaseChanged;

        internal AttackPhase Phase { get; private set; } = AttackPhase.Idle;

        internal bool IsRunning => Phase != AttackPhase.Idle;

        internal bool Start()
        {
            if (IsRunning)
            {
                return false;
            }

            phaseElapsed = 0f;
            SetPhase(AttackPhase.Windup);
            return true;
        }

        internal void Tick(float deltaTime)
        {
            if (!IsRunning || deltaTime <= 0f)
            {
                return;
            }

            float remainingTime = deltaTime;
            while (IsRunning && remainingTime > TimeComparisonTolerance)
            {
                float phaseDuration = GetPhaseDuration(Phase);
                float timeUntilTransition = Math.Max(0f, phaseDuration - phaseElapsed);
                if (remainingTime + TimeComparisonTolerance < timeUntilTransition)
                {
                    phaseElapsed += remainingTime;
                    return;
                }

                remainingTime = Math.Max(0f, remainingTime - timeUntilTransition);
                AdvancePhase();
            }
        }

        internal bool Cancel()
        {
            if (!IsRunning)
            {
                return false;
            }

            phaseElapsed = 0f;
            SetPhase(AttackPhase.Idle);
            return true;
        }

        private float GetPhaseDuration(AttackPhase phase)
        {
            return phase switch
            {
                AttackPhase.Windup => windupDuration,
                AttackPhase.Active => activeDuration,
                AttackPhase.Recovery => recoveryDuration,
                _ => 0f
            };
        }

        private void AdvancePhase()
        {
            phaseElapsed = 0f;
            SetPhase(Phase switch
            {
                AttackPhase.Windup => AttackPhase.Active,
                AttackPhase.Active => AttackPhase.Recovery,
                AttackPhase.Recovery => AttackPhase.Idle,
                _ => AttackPhase.Idle
            });
        }

        private void SetPhase(AttackPhase nextPhase)
        {
            if (Phase == nextPhase)
            {
                return;
            }

            AttackPhase previousPhase = Phase;
            Phase = nextPhase;
            PhaseChanged?.Invoke(previousPhase, nextPhase);
        }
    }
}
