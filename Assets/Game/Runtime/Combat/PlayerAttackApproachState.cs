using System.Collections.Generic;
using JustTest.Game.Enemies;
using JustTest.Game.Run;
using JustTest.Game.Weapons;
using UnityEngine;

namespace JustTest.Game.Combat
{
    internal sealed class PlayerAttackApproachState
    {
        private const float ComparisonTolerance = 0.0001f;

        private readonly List<CombatEnemyRuntime2D> candidates =
            new List<CombatEnemyRuntime2D>(8);

        private CombatEnemyRuntime2D target;
        private int targetLeaseId;
        private float targetRetainUntil = float.NegativeInfinity;
        private float stepOriginPlayerCenterX;
        private int stepDirection = 1;
        private bool stepActive;
        private bool holdPosition;

        internal bool HasTarget => target != null;
        internal Transform TargetTransform => target != null ? target.transform : null;

        internal bool PrepareStep(
            CombatPlatformController2D combatPlatform,
            Collider2D playerCollider,
            WeaponBasicComboStep step,
            int facingDirection,
            int inputDirection,
            float timestamp,
            out int resolvedDirection)
        {
            ResetStep();
            resolvedDirection = ResolveFallbackDirection(facingDirection, inputDirection);
            if (combatPlatform == null ||
                playerCollider == null ||
                step == null ||
                !step.TargetAssistEnabled)
            {
                return false;
            }

            if (timestamp > targetRetainUntil ||
                !IsTargetValid(
                    combatPlatform,
                    playerCollider.bounds,
                    target,
                    targetLeaseId,
                    step.TargetRetainDistance,
                    step.MaximumTargetVerticalDifference))
            {
                ClearTarget();
            }

            int directionalIntent =
                step.AllowDirectionalRetarget && (inputDirection == -1 || inputDirection == 1)
                    ? inputDirection
                    : 0;
            if (target != null)
            {
                int currentTargetDirection = GetTargetDirection(
                    playerCollider.bounds,
                    target.TargetingCollider.bounds,
                    facingDirection);
                if (directionalIntent != 0 && directionalIntent != currentTargetDirection)
                {
                    if (TrySelectTarget(
                            combatPlatform,
                            playerCollider.bounds,
                            step,
                            facingDirection,
                            directionalIntent,
                            true,
                            out CombatEnemyRuntime2D directionalTarget,
                            out _))
                    {
                        SetTarget(directionalTarget, timestamp, step.TargetRetentionDuration);
                    }
                    else
                    {
                        ClearTarget();
                        resolvedDirection = directionalIntent;
                        return false;
                    }
                }
                else if (TrySelectTarget(
                             combatPlatform,
                             playerCollider.bounds,
                             step,
                             facingDirection,
                             directionalIntent,
                             directionalIntent != 0,
                             out CombatEnemyRuntime2D candidate,
                             out float candidateScore))
                {
                    float currentScore = CalculateTargetScore(
                        playerCollider.bounds,
                        target.TargetingCollider.bounds,
                        facingDirection,
                        step.RearTargetPenalty);
                    if (!ReferenceEquals(candidate, target) &&
                        ShouldSwitchTarget(
                            currentScore,
                            candidateScore,
                            step.RetargetThreshold,
                            false))
                    {
                        SetTarget(candidate, timestamp, step.TargetRetentionDuration);
                    }
                }
            }
            else if (TrySelectTarget(
                         combatPlatform,
                         playerCollider.bounds,
                         step,
                         facingDirection,
                         directionalIntent,
                         directionalIntent != 0,
                         out CombatEnemyRuntime2D selectedTarget,
                         out _))
            {
                SetTarget(selectedTarget, timestamp, step.TargetRetentionDuration);
            }

            if (target == null)
            {
                return false;
            }

            Collider2D targetCollider = target.TargetingCollider;
            int targetDirection = GetTargetDirection(
                playerCollider.bounds,
                targetCollider.bounds,
                resolvedDirection);
            if (step.AllowAutoTurn)
            {
                resolvedDirection = targetDirection;
            }

            stepOriginPlayerCenterX = playerCollider.bounds.center.x;
            stepDirection = resolvedDirection;
            stepActive = true;
            holdPosition = false;
            targetRetainUntil = timestamp + step.TargetRetentionDuration;
            return true;
        }

        internal bool TryResolveWarpVelocity(
            CombatPlatformController2D combatPlatform,
            Collider2D playerCollider,
            BoxCollider2D attackCollider,
            WeaponBasicComboStep step,
            AttackPhase phase,
            float phaseProgress,
            float deltaTime,
            out float horizontalVelocity)
        {
            horizontalVelocity = 0f;
            if (!stepActive ||
                target == null ||
                combatPlatform == null ||
                playerCollider == null ||
                attackCollider == null ||
                step == null)
            {
                return false;
            }

            if (holdPosition)
            {
                horizontalVelocity = 0f;
                return true;
            }

            Collider2D targetCollider = target.TargetingCollider;
            if (!IsTargetValid(
                    combatPlatform,
                    playerCollider.bounds,
                    target,
                    targetLeaseId,
                    step.TargetRetainDistance,
                    step.MaximumTargetVerticalDifference) ||
                targetCollider == null)
            {
                ClearTarget();
                return false;
            }

            Bounds playerBounds = playerCollider.bounds;
            Bounds targetBounds = targetCollider.bounds;
            float signedCenterDifference =
                (targetBounds.center.x - playerBounds.center.x) * stepDirection;
            if (signedCenterDifference <= 0f)
            {
                horizontalVelocity = 0f;
                return true;
            }

            float attackReach = CalculateAttackReach(
                attackCollider,
                playerBounds.center.x,
                stepDirection);
            if (attackReach <= 0f ||
                !combatPlatform.TryGetPlayerHorizontalLimits(
                    out float minimumCenterX,
                    out float maximumCenterX))
            {
                ClearTarget();
                return false;
            }

            float targetNearEdge = stepDirection > 0
                ? targetBounds.min.x
                : targetBounds.max.x;
            float desiredPlayerCenterX =
                targetNearEdge - stepDirection * (attackReach - step.TargetOverlapDepth);
            float requestedTravel = Mathf.Clamp(
                (desiredPlayerCenterX - stepOriginPlayerCenterX) * stepDirection,
                0f,
                step.MaximumAssistTravelDistance);
            desiredPlayerCenterX = Mathf.Clamp(
                stepOriginPlayerCenterX + requestedTravel * stepDirection,
                minimumCenterX,
                maximumCenterX);
            requestedTravel = Mathf.Max(
                0f,
                (desiredPlayerCenterX - stepOriginPlayerCenterX) * stepDirection);

            float movementProgress = step.EvaluateMovementCurve(phase, phaseProgress);
            float availableTravel = requestedTravel * movementProgress;
            float actualTravel = Mathf.Max(
                0f,
                (playerBounds.center.x - stepOriginPlayerCenterX) * stepDirection);
            horizontalVelocity = CalculateWarpVelocity(
                step.MaximumWarpSpeed,
                stepDirection,
                availableTravel,
                actualTravel,
                step.GetTargetCorrectionStrength(phase),
                deltaTime);
            return true;
        }

        internal float CalculateWarpVelocity(
            float maximumWarpSpeed,
            int direction,
            float availableTravel,
            float actualTravel,
            float correctionStrength,
            float deltaTime)
        {
            float remainingTravel = availableTravel - actualTravel;
            float strength = Mathf.Clamp01(correctionStrength);
            if (maximumWarpSpeed <= 0f ||
                remainingTravel <= 0f ||
                strength <= 0f ||
                deltaTime <= 0f ||
                (direction != -1 && direction != 1))
            {
                return 0f;
            }

            float speedLimit = maximumWarpSpeed * strength;
            float speedWithoutOvershoot = remainingTravel / deltaTime;
            return Mathf.Min(speedLimit, speedWithoutOvershoot) * direction;
        }

        internal bool ShouldSwitchTarget(
            float currentScore,
            float candidateScore,
            float retargetThreshold,
            bool directionalOverride)
        {
            if (directionalOverride)
            {
                return true;
            }

            if (!IsFinite(currentScore) || !IsFinite(candidateScore))
            {
                return false;
            }

            return candidateScore < currentScore * Mathf.Clamp01(retargetThreshold);
        }

        internal void TickRetention(
            CombatPlatformController2D combatPlatform,
            float timestamp)
        {
            if (stepActive || target == null)
            {
                return;
            }

            if (timestamp > targetRetainUntil ||
                combatPlatform == null ||
                !combatPlatform.IsLivingEnemy(target, targetLeaseId))
            {
                ClearTarget();
            }
        }

        internal void HoldPosition()
        {
            if (stepActive && target != null)
            {
                holdPosition = true;
            }
        }

        internal void EndStep(float timestamp, float retentionDuration)
        {
            if (target != null)
            {
                targetRetainUntil = Mathf.Max(
                    targetRetainUntil,
                    timestamp + Mathf.Max(0f, retentionDuration));
            }

            ResetStep();
        }

        internal void ClearTarget()
        {
            target = null;
            targetLeaseId = 0;
            targetRetainUntil = float.NegativeInfinity;
            ResetStep();
            candidates.Clear();
        }

        private bool TrySelectTarget(
            CombatPlatformController2D combatPlatform,
            Bounds playerBounds,
            WeaponBasicComboStep step,
            int facingDirection,
            int preferredDirection,
            bool restrictToPreferredDirection,
            out CombatEnemyRuntime2D selectedTarget,
            out float selectedScore)
        {
            selectedTarget = null;
            selectedScore = float.PositiveInfinity;
            if (!combatPlatform.TryCopyLivingEnemiesTo(candidates))
            {
                return false;
            }

            int bestParticipantId = int.MaxValue;
            for (int index = 0; index < candidates.Count; index++)
            {
                CombatEnemyRuntime2D candidate = candidates[index];
                Collider2D targetCollider = candidate != null
                    ? candidate.TargetingCollider
                    : null;
                if (!IsTargetValid(
                        combatPlatform,
                        playerBounds,
                        candidate,
                        candidate != null ? candidate.LeaseId : 0,
                        step.TargetLockDistance,
                        step.MaximumTargetVerticalDifference) ||
                    targetCollider == null)
                {
                    continue;
                }

                int candidateDirection = GetTargetDirection(
                    playerBounds,
                    targetCollider.bounds,
                    facingDirection);
                if (restrictToPreferredDirection && candidateDirection != preferredDirection)
                {
                    continue;
                }

                float score = CalculateTargetScore(
                    playerBounds,
                    targetCollider.bounds,
                    facingDirection,
                    step.RearTargetPenalty);
                int participantId = candidate.ParticipantId;
                if (score > selectedScore + ComparisonTolerance ||
                    (Mathf.Abs(score - selectedScore) <= ComparisonTolerance &&
                     participantId >= bestParticipantId))
                {
                    continue;
                }

                selectedTarget = candidate;
                selectedScore = score;
                bestParticipantId = participantId;
            }

            return selectedTarget != null;
        }

        private bool IsTargetValid(
            CombatPlatformController2D combatPlatform,
            Bounds playerBounds,
            CombatEnemyRuntime2D candidate,
            int expectedLeaseId,
            float maximumDistance,
            float maximumVerticalDifference)
        {
            if (candidate == null ||
                !combatPlatform.IsLivingEnemy(candidate, expectedLeaseId))
            {
                return false;
            }

            Collider2D targetCollider = candidate.TargetingCollider;
            if (targetCollider == null || !targetCollider.enabled)
            {
                return false;
            }

            Bounds targetBounds = targetCollider.bounds;
            float verticalDifference = Mathf.Abs(
                targetBounds.center.y - playerBounds.center.y);
            float horizontalGap = CalculateHorizontalGap(playerBounds, targetBounds);
            return verticalDifference <= maximumVerticalDifference &&
                   horizontalGap <= maximumDistance &&
                   combatPlatform.IsPlayerTargetPathClear(
                       playerBounds.center,
                       targetBounds.center);
        }

        private float CalculateTargetScore(
            Bounds playerBounds,
            Bounds targetBounds,
            int facingDirection,
            float rearTargetPenalty)
        {
            float horizontalGap = CalculateHorizontalGap(playerBounds, targetBounds);
            float verticalDifference = Mathf.Abs(
                targetBounds.center.y - playerBounds.center.y);
            int direction = GetTargetDirection(playerBounds, targetBounds, facingDirection);
            float directionPenalty = direction == facingDirection ? 0f : rearTargetPenalty;
            return horizontalGap + verticalDifference * 0.5f + directionPenalty;
        }

        private float CalculateAttackReach(
            BoxCollider2D attackCollider,
            float playerCenterX,
            int direction)
        {
            float attackCenterX = attackCollider.transform
                .TransformPoint(attackCollider.offset)
                .x;
            float attackHalfWidth =
                attackCollider.size.x *
                0.5f *
                Mathf.Abs(attackCollider.transform.lossyScale.x);
            float forwardEdgeX = attackCenterX + attackHalfWidth * direction;
            return (forwardEdgeX - playerCenterX) * direction;
        }

        private static float CalculateHorizontalGap(Bounds playerBounds, Bounds targetBounds)
        {
            if (targetBounds.min.x > playerBounds.max.x)
            {
                return targetBounds.min.x - playerBounds.max.x;
            }

            if (playerBounds.min.x > targetBounds.max.x)
            {
                return playerBounds.min.x - targetBounds.max.x;
            }

            return 0f;
        }

        private static int GetTargetDirection(
            Bounds playerBounds,
            Bounds targetBounds,
            int fallbackDirection)
        {
            float difference = targetBounds.center.x - playerBounds.center.x;
            if (Mathf.Abs(difference) <= ComparisonTolerance)
            {
                return fallbackDirection == -1 ? -1 : 1;
            }

            return difference < 0f ? -1 : 1;
        }

        private static int ResolveFallbackDirection(int facingDirection, int inputDirection)
        {
            if (inputDirection == -1 || inputDirection == 1)
            {
                return inputDirection;
            }

            return facingDirection == -1 ? -1 : 1;
        }

        private void SetTarget(
            CombatEnemyRuntime2D selectedTarget,
            float timestamp,
            float retentionDuration)
        {
            target = selectedTarget;
            targetLeaseId = selectedTarget != null ? selectedTarget.LeaseId : 0;
            targetRetainUntil = selectedTarget != null
                ? timestamp + Mathf.Max(0f, retentionDuration)
                : float.NegativeInfinity;
        }

        private void ResetStep()
        {
            stepOriginPlayerCenterX = 0f;
            stepDirection = 1;
            stepActive = false;
            holdPosition = false;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
