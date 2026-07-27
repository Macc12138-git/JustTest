using JustTest.Game.Run;
using NUnit.Framework;

namespace JustTest.Game.Tests.EditMode
{
    public sealed class CombatPositionSlotAllocatorTests
    {
        [Test]
        public void ParticipantsAreClampedToDistinctHorizontalZones()
        {
            CombatPositionSlotAllocator allocator = new CombatPositionSlotAllocator(
                0f,
                6f,
                0.1f,
                new[] { 10, 20, 30 });

            Assert.That(allocator.TryGetTarget(10, 6f, out float leftTarget), Is.True);
            Assert.That(allocator.TryGetTarget(20, 0f, out float middleTarget), Is.True);
            Assert.That(allocator.TryGetTarget(30, 0f, out float rightTarget), Is.True);
            Assert.That(leftTarget, Is.EqualTo(1.9f).Within(0.001f));
            Assert.That(middleTarget, Is.EqualTo(2.1f).Within(0.001f));
            Assert.That(rightTarget, Is.EqualTo(4.1f).Within(0.001f));
        }

        [Test]
        public void ParticipantCannotMoveBeyondItsZoneBoundary()
        {
            CombatPositionSlotAllocator allocator = new CombatPositionSlotAllocator(
                0f,
                6f,
                0.1f,
                new[] { 10, 20, 30 });

            Assert.That(allocator.CanMove(10, 0.15f, -1, 0.1f), Is.False);
            Assert.That(allocator.CanMove(10, 1f, -1, 0.1f), Is.True);
            Assert.That(allocator.CanMove(10, 1.85f, 1, 0.1f), Is.False);
        }

        [Test]
        public void InvalidGeometryDoesNotProduceTargets()
        {
            CombatPositionSlotAllocator allocator = new CombatPositionSlotAllocator(
                0f,
                1f,
                0.2f,
                new[] { 10, 20, 30 });

            Assert.That(allocator.IsValid, Is.False);
            Assert.That(allocator.TryGetTarget(10, 0.5f, out _), Is.False);
        }
    }
}
