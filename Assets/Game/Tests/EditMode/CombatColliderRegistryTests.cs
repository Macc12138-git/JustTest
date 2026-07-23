using JustTest.Game.Combat;
using NUnit.Framework;

namespace JustTest.Game.Tests
{
    public sealed class CombatColliderRegistryTests
    {
        private CombatColliderRegistry registry;

        [SetUp]
        public void SetUp()
        {
            registry = CombatColliderRegistry.Instance;
            registry.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            registry.Clear();
        }

        [Test]
        public void Register_MakesHurtboxResolvable()
        {
            FakeHurtbox hurtbox = new FakeHurtbox();

            bool registered = registry.Register(10, hurtbox);
            bool resolved = registry.TryResolve(10, out ICombatHurtbox resolvedHurtbox);

            Assert.That(registered, Is.True);
            Assert.That(resolved, Is.True);
            Assert.That(resolvedHurtbox, Is.SameAs(hurtbox));
        }

        [Test]
        public void Register_DoesNotReplaceDifferentHurtboxForSameCollider()
        {
            FakeHurtbox firstHurtbox = new FakeHurtbox();
            FakeHurtbox secondHurtbox = new FakeHurtbox();
            registry.Register(10, firstHurtbox);

            bool registered = registry.Register(10, secondHurtbox);
            registry.TryResolve(10, out ICombatHurtbox resolvedHurtbox);

            Assert.That(registered, Is.False);
            Assert.That(resolvedHurtbox, Is.SameAs(firstHurtbox));
        }

        [Test]
        public void Unregister_IgnoresRequestFromDifferentHurtbox()
        {
            FakeHurtbox registeredHurtbox = new FakeHurtbox();
            FakeHurtbox differentHurtbox = new FakeHurtbox();
            registry.Register(10, registeredHurtbox);

            registry.Unregister(10, differentHurtbox);

            Assert.That(registry.TryResolve(10, out _), Is.True);
        }

        [Test]
        public void Unregister_RemovesMatchingRegistration()
        {
            FakeHurtbox hurtbox = new FakeHurtbox();
            registry.Register(10, hurtbox);

            registry.Unregister(10, hurtbox);

            Assert.That(registry.TryResolve(10, out _), Is.False);
            Assert.That(registry.Count, Is.Zero);
        }

        [Test]
        public void TryResolve_RemovesUnavailableHurtbox()
        {
            FakeHurtbox hurtbox = new FakeHurtbox { IsAvailable = false };
            registry.Register(10, hurtbox);

            bool resolved = registry.TryResolve(10, out _);

            Assert.That(resolved, Is.False);
            Assert.That(registry.Count, Is.Zero);
        }

        private sealed class FakeHurtbox : ICombatHurtbox
        {
            public bool IsAvailable { get; set; } = true;

            public HitResult ReceiveHit(AttackInstance attackInstance)
            {
                return default;
            }
        }
    }
}
