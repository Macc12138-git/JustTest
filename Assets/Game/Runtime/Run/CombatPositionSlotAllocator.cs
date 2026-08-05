using System;
using System.Collections.Generic;

namespace JustTest.Game.Run
{
    internal sealed class CombatPositionSlotAllocator
    {
        private readonly float minimumX;
        private readonly float maximumX;
        private readonly float innerPadding;
        private readonly List<int> orderedParticipantIds = new List<int>();
        private readonly Dictionary<int, float> participantPositions =
            new Dictionary<int, float>();
        private readonly Dictionary<int, PositionSlot> slots = new Dictionary<int, PositionSlot>();

        internal CombatPositionSlotAllocator(
            float minimumX,
            float maximumX,
            float innerPadding)
        {
            if (!IsFinite(minimumX) ||
                !IsFinite(maximumX) ||
                !IsFinite(innerPadding) ||
                maximumX <= minimumX ||
                innerPadding < 0f)
            {
                return;
            }

            this.minimumX = minimumX;
            this.maximumX = maximumX;
            this.innerPadding = innerPadding;
            IsValid = true;
        }

        internal CombatPositionSlotAllocator(
            float minimumX,
            float maximumX,
            float innerPadding,
            IReadOnlyList<int> participantIds)
            : this(minimumX, maximumX, innerPadding)
        {
            if (!IsValid ||
                participantIds == null ||
                participantIds.Count == 0)
            {
                IsValid = false;
                return;
            }

            for (int index = 0; index < participantIds.Count; index++)
            {
                int participantId = participantIds[index];
                if (participantId == 0 || orderedParticipantIds.Contains(participantId))
                {
                    orderedParticipantIds.Clear();
                    IsValid = false;
                    return;
                }

                orderedParticipantIds.Add(participantId);
                participantPositions.Add(participantId, index);
            }

            IsValid = RebuildSlots();
        }

        internal bool IsValid { get; private set; }

        internal int ParticipantCount => orderedParticipantIds.Count;

        internal bool Register(int participantId, float currentX)
        {
            if (!IsValid ||
                participantId == 0 ||
                !IsFinite(currentX) ||
                orderedParticipantIds.Contains(participantId))
            {
                return false;
            }

            int insertionIndex = orderedParticipantIds.Count;
            for (int index = 0; index < orderedParticipantIds.Count; index++)
            {
                float existingPosition = participantPositions[orderedParticipantIds[index]];
                if (currentX < existingPosition)
                {
                    insertionIndex = index;
                    break;
                }
            }

            orderedParticipantIds.Insert(insertionIndex, participantId);
            participantPositions.Add(participantId, currentX);
            if (RebuildSlots())
            {
                return true;
            }

            orderedParticipantIds.Remove(participantId);
            participantPositions.Remove(participantId);
            RebuildSlots();
            return false;
        }

        internal bool Unregister(int participantId)
        {
            if (!IsValid || !orderedParticipantIds.Remove(participantId))
            {
                return false;
            }

            participantPositions.Remove(participantId);
            RebuildSlots();
            return true;
        }

        internal bool TryGetTarget(int participantId, float desiredX, out float targetX)
        {
            targetX = desiredX;
            if (!IsValid ||
                !IsFinite(desiredX) ||
                !slots.TryGetValue(participantId, out PositionSlot slot))
            {
                return false;
            }

            targetX = Math.Min(slot.MaximumX, Math.Max(slot.MinimumX, desiredX));
            return true;
        }

        internal bool CanMove(int participantId, float currentX, int direction, float tolerance)
        {
            if (!IsValid ||
                !IsFinite(currentX) ||
                !IsFinite(tolerance) ||
                tolerance < 0f ||
                (direction != -1 && direction != 1) ||
                !slots.TryGetValue(participantId, out PositionSlot slot))
            {
                return false;
            }

            return direction < 0
                ? currentX > slot.MinimumX + tolerance
                : currentX < slot.MaximumX - tolerance;
        }

        private bool RebuildSlots()
        {
            slots.Clear();
            if (orderedParticipantIds.Count == 0)
            {
                return true;
            }

            float slotWidth = (maximumX - minimumX) / orderedParticipantIds.Count;
            if (slotWidth <= innerPadding * 2f)
            {
                return false;
            }

            for (int index = 0; index < orderedParticipantIds.Count; index++)
            {
                float slotMinimum = minimumX + slotWidth * index + innerPadding;
                float slotMaximum = minimumX + slotWidth * (index + 1) - innerPadding;
                slots.Add(
                    orderedParticipantIds[index],
                    new PositionSlot(slotMinimum, slotMaximum));
            }

            return true;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private readonly struct PositionSlot
        {
            internal PositionSlot(float minimumX, float maximumX)
            {
                MinimumX = minimumX;
                MaximumX = maximumX;
            }

            internal float MinimumX { get; }
            internal float MaximumX { get; }
        }
    }
}
