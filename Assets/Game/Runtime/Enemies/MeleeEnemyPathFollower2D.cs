using System.Collections.Generic;
using UnityEngine;

namespace JustTest.Game.Enemies
{
    public sealed class MeleeEnemyPathFollower2D : MonoBehaviour
    {
        [SerializeField] private MeleeEnemyConfig config;
        [SerializeField] private PlatformNavigationGraph graph;
        [SerializeField] private MeleeEnemyMotor2D motor;
        [SerializeField] private Collider2D bodyCollider;

        private readonly PlatformPathfinder pathfinder = new();
        private readonly List<PlatformNavigationEdge> path = new();
        private int pathIndex;
        private int plannedDestinationNodeId = -1;
        private float nextReplanTime;
        private float lastProgressTime;
        private Vector2 lastProgressPosition;
        private int obstacleTraversalDirection;
        private int obstacleTraversalSourceNodeId = -1;
        private float obstacleTraversalExitX;
        private float obstacleTraversalStartedAt;
        private bool jumpIssued;
        private bool dropIssued;
        private bool dropDepartureConfirmed;
        private bool obstacleTraversalActive;
        private bool ready;

        internal bool HasPath => pathIndex < path.Count;

        private void Awake()
        {
            ready = config != null && graph != null && motor != null && bodyCollider != null;
            if (ready)
            {
                lastProgressPosition = transform.position;
                lastProgressTime = Time.time;
                return;
            }

            Debug.LogError($"{nameof(MeleeEnemyPathFollower2D)} is missing an Inspector reference.", this);
            enabled = false;
        }

        internal bool IsOnSamePlatform(Vector2 targetPosition)
        {
            return TryResolveNodes(targetPosition, out int currentNodeId, out int targetNodeId) &&
                   currentNodeId == targetNodeId;
        }

        internal bool Tick(Vector2 targetPosition)
        {
            if (!ready || !TryResolveNodes(targetPosition, out int currentNodeId, out int targetNodeId))
            {
                CancelObstacleTraversal();
                Stop();
                return false;
            }

            if (obstacleTraversalActive &&
                TryFollowObstacleTraversal(targetPosition, currentNodeId))
            {
                return true;
            }

            if (currentNodeId == targetNodeId)
            {
                ClearPath();
                MoveTowardX(targetPosition.x);
                int direction = GetHorizontalDirection(targetPosition.x);
                if (direction != 0 && motor.TryStartObstacleTraversal(direction, out float exitX))
                {
                    obstacleTraversalActive = true;
                    obstacleTraversalDirection = direction;
                    obstacleTraversalSourceNodeId = currentNodeId;
                    obstacleTraversalExitX = exitX;
                    obstacleTraversalStartedAt = Time.time;
                }

                return true;
            }

            bool pathInvalidAfterLanding =
                HasPath &&
                motor.IsGrounded &&
                currentNodeId != path[pathIndex].FromNodeId &&
                currentNodeId != path[pathIndex].ToNodeId;
            if (plannedDestinationNodeId != targetNodeId ||
                pathIndex >= path.Count ||
                (pathInvalidAfterLanding && Time.time >= nextReplanTime))
            {
                Replan(currentNodeId, targetNodeId);
            }

            if (!HasPath)
            {
                motor.SetHorizontalDirection(0f);
                return false;
            }

            FollowEdge(path[pathIndex]);
            DetectStuck();
            return true;
        }

        internal void Stop()
        {
            motor.SetHorizontalDirection(0f);
        }

        internal void ResetPath()
        {
            CancelObstacleTraversal();
            ClearPath();
            nextReplanTime = 0f;
            lastProgressPosition = transform.position;
            lastProgressTime = Time.time;
            motor.SetHorizontalDirection(0f);
        }

        private bool TryResolveNodes(Vector2 targetPosition, out int currentNodeId, out int targetNodeId)
        {
            currentNodeId = -1;
            targetNodeId = -1;
            Vector2 footPosition = new Vector2(bodyCollider.bounds.center.x, bodyCollider.bounds.min.y);
            return graph.TryFindClosestNode(footPosition, config.NodeSnapDistance, out currentNodeId) &&
                   graph.TryFindClosestNode(targetPosition, config.NodeSnapDistance, out targetNodeId);
        }

        private void Replan(int currentNodeId, int targetNodeId)
        {
            pathfinder.TryFindPath(graph, currentNodeId, targetNodeId, path);
            pathIndex = 0;
            plannedDestinationNodeId = targetNodeId;
            nextReplanTime = Time.time + config.ReplanInterval;
            jumpIssued = false;
            dropIssued = false;
            dropDepartureConfirmed = false;
        }

        private void FollowEdge(PlatformNavigationEdge edge)
        {
            Vector2 position = transform.position;
            bool onDestinationNode = TryGetCurrentNode(out int currentNodeId) &&
                                     currentNodeId == edge.ToNodeId &&
                                     motor.IsGrounded;
            if (onDestinationNode)
            {
                AdvanceEdge();
                return;
            }

            switch (edge.Action)
            {
                case PlatformNavigationAction.Walk:
                    MoveTowardX(edge.LandingPoint.x);
                    break;
                case PlatformNavigationAction.Jump:
                    FollowJump(edge, position);
                    break;
                case PlatformNavigationAction.Drop:
                    FollowDrop(edge, position, false);
                    break;
                case PlatformNavigationAction.DropThrough:
                    FollowDrop(edge, position, true);
                    break;
            }
        }

        private void FollowJump(PlatformNavigationEdge edge, Vector2 position)
        {
            if (!jumpIssued)
            {
                MoveTowardX(edge.TakeoffPoint.x);
                if (motor.IsGrounded && Mathf.Abs(position.x - edge.TakeoffPoint.x) <= config.WaypointTolerance)
                {
                    motor.Face(edge.LandingPoint.x >= position.x ? 1 : -1);
                    motor.RequestJump();
                    jumpIssued = true;
                }

                return;
            }

            MoveTowardX(edge.LandingPoint.x);
        }

        private void FollowDrop(PlatformNavigationEdge edge, Vector2 position, bool dropThrough)
        {
            if (!dropIssued)
            {
                MoveTowardX(edge.TakeoffPoint.x);
                if (Mathf.Abs(position.x - edge.TakeoffPoint.x) <= config.WaypointTolerance)
                {
                    if (dropThrough)
                    {
                        motor.RequestDropThrough();
                    }

                    dropIssued = true;
                }

                return;
            }

            if (!dropThrough && !dropDepartureConfirmed)
            {
                if (motor.IsGrounded)
                {
                    float departureDelta = edge.LandingPoint.x - edge.TakeoffPoint.x;
                    float departureDirection = Mathf.Abs(departureDelta) <= 0.001f
                        ? motor.FacingDirection
                        : Mathf.Sign(departureDelta);
                    motor.SetHorizontalDirection(departureDirection);
                    return;
                }

                dropDepartureConfirmed = true;
            }

            MoveTowardX(edge.LandingPoint.x);
        }

        private bool TryGetCurrentNode(out int nodeId)
        {
            Vector2 footPosition = new Vector2(bodyCollider.bounds.center.x, bodyCollider.bounds.min.y);
            return graph.TryFindClosestNode(footPosition, config.NodeSnapDistance, out nodeId);
        }

        private void MoveTowardX(float targetX)
        {
            float delta = targetX - transform.position.x;
            motor.SetHorizontalDirection(
                Mathf.Abs(delta) <= config.WaypointTolerance ? 0f : Mathf.Sign(delta));
        }

        private bool TryFollowObstacleTraversal(Vector2 targetPosition, int currentNodeId)
        {
            int targetDirection = GetHorizontalDirection(targetPosition.x);
            bool targetReversed = targetDirection != 0 && targetDirection != obstacleTraversalDirection;
            bool timedOut = Time.time - obstacleTraversalStartedAt >= config.StuckDuration * 3f;
            if (targetReversed || timedOut)
            {
                CancelObstacleTraversal();
                return false;
            }

            motor.SetHorizontalDirection(obstacleTraversalDirection);
            bool crossedExit = obstacleTraversalDirection > 0
                ? transform.position.x >= obstacleTraversalExitX
                : transform.position.x <= obstacleTraversalExitX;
            if (!crossedExit || !motor.IsGrounded || currentNodeId != obstacleTraversalSourceNodeId)
            {
                return true;
            }

            CancelObstacleTraversal();
            MoveTowardX(targetPosition.x);
            return true;
        }

        private int GetHorizontalDirection(float targetX)
        {
            float delta = targetX - transform.position.x;
            if (Mathf.Abs(delta) <= config.WaypointTolerance)
            {
                return 0;
            }

            return delta > 0f ? 1 : -1;
        }

        private void CancelObstacleTraversal()
        {
            obstacleTraversalActive = false;
            obstacleTraversalDirection = 0;
            obstacleTraversalSourceNodeId = -1;
            obstacleTraversalExitX = 0f;
            obstacleTraversalStartedAt = 0f;
        }

        private void AdvanceEdge()
        {
            pathIndex++;
            jumpIssued = false;
            dropIssued = false;
            dropDepartureConfirmed = false;
            lastProgressPosition = transform.position;
            lastProgressTime = Time.time;
        }

        private void DetectStuck()
        {
            Vector2 position = transform.position;
            if (Vector2.Distance(position, lastProgressPosition) >= config.StuckMovementThreshold)
            {
                lastProgressPosition = position;
                lastProgressTime = Time.time;
                return;
            }

            if (Time.time - lastProgressTime < config.StuckDuration)
            {
                return;
            }

            ClearPath();
            nextReplanTime = 0f;
            lastProgressPosition = position;
            lastProgressTime = Time.time;
        }

        private void ClearPath()
        {
            path.Clear();
            pathIndex = 0;
            plannedDestinationNodeId = -1;
            jumpIssued = false;
            dropIssued = false;
            dropDepartureConfirmed = false;
        }
    }
}
