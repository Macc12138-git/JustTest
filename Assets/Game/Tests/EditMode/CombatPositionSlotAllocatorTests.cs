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

        [Test]
        public void DynamicParticipantsRepartitionAfterRemoval()
        {
            CombatPositionSlotAllocator allocator = new CombatPositionSlotAllocator(
                0f,
                6f,
                0.1f);

            Assert.That(allocator.Register(10, 1f), Is.True);
            Assert.That(allocator.Register(20, 3f), Is.True);
            Assert.That(allocator.Register(30, 5f), Is.True);
            Assert.That(allocator.Unregister(20), Is.True);

            Assert.That(allocator.TryGetTarget(10, 6f, out float leftTarget), Is.True);
            Assert.That(allocator.TryGetTarget(30, 0f, out float rightTarget), Is.True);
            Assert.That(leftTarget, Is.EqualTo(2.9f).Within(0.001f));
            Assert.That(rightTarget, Is.EqualTo(3.1f).Within(0.001f));
            Assert.That(allocator.ParticipantCount, Is.EqualTo(2));
        }

        [Test]
        public void DynamicRegistrationRejectsDuplicateParticipant()
        {
            CombatPositionSlotAllocator allocator = new CombatPositionSlotAllocator(
                0f,
                6f,
                0.1f);

            Assert.That(allocator.Register(10, 1f), Is.True);
            Assert.That(allocator.Register(10, 2f), Is.False);
            Assert.That(allocator.ParticipantCount, Is.EqualTo(1));
        }

        [Test]
        public void DynamicParticipantsAreOrderedByRegistrationPosition()
        {
            CombatPositionSlotAllocator allocator = new CombatPositionSlotAllocator(
                0f,
                6f,
                0.1f);

            Assert.That(allocator.Register(30, 5f), Is.True);
            Assert.That(allocator.Register(20, 3f), Is.True);
            Assert.That(allocator.Register(10, 1f), Is.True);

            Assert.That(allocator.TryGetTarget(10, 6f, out float leftTarget), Is.True);
            Assert.That(allocator.TryGetTarget(20, 0f, out float middleTarget), Is.True);
            Assert.That(allocator.TryGetTarget(30, 0f, out float rightTarget), Is.True);
            Assert.That(leftTarget, Is.EqualTo(1.9f).Within(0.001f));
            Assert.That(middleTarget, Is.EqualTo(2.1f).Within(0.001f));
            Assert.That(rightTarget, Is.EqualTo(4.1f).Within(0.001f));
        }
    }
}
