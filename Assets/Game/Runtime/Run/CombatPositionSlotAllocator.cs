using System;
using System.Collections.Generic;

namespace JustTest.Game.Run
{
    internal sealed class CombatPositionSlotAllocator
    {
        private readonly Dictionary<int, PositionSlot> slots = new Dictionary<int, PositionSlot>();

        internal CombatPositionSlotAllocator(
            float minimumX,
            float maximumX,
            float innerPadding,
            IReadOnlyList<int> participantIds)
        {
            if (!IsFinite(minimumX) ||
                !IsFinite(maximumX) ||
                !IsFinite(innerPadding) ||
                maximumX <= minimumX ||
                innerPadding < 0f ||
                participantIds == null ||
                participantIds.Count == 0)
            {
                return;
            }

            float slotWidth = (maximumX - minimumX) / participantIds.Count;
            if (slotWidth <= innerPadding * 2f)
            {
                return;
            }

            for (int index = 0; index < participantIds.Count; index++)
            {
                int participantId = participantIds[index];
                if (participantId == 0 || slots.ContainsKey(participantId))
                {
                    slots.Clear();
                    return;
                }

                float slotMinimum = minimumX + slotWidth * index + innerPadding;
                float slotMaximum = minimumX + slotWidth * (index + 1) - innerPadding;
                slots.Add(participantId, new PositionSlot(slotMinimum, slotMaximum));
            }

            IsValid = true;
        }

        internal bool IsValid { get; }

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
