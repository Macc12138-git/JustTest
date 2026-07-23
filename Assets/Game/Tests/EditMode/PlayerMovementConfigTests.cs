using JustTest.Game.Player;
using NUnit.Framework;
using UnityEngine;

namespace JustTest.Game.Tests
{
    public sealed class PlayerMovementConfigTests
    {
        [Test]
        public void DefaultJumpValues_ReachConfiguredHeightAtApex()
        {
            PlayerMovementConfig config = ScriptableObject.CreateInstance<PlayerMovementConfig>();

            float height =
                config.JumpSpeed * config.TimeToJumpApex -
                0.5f * config.GravityMagnitude * config.TimeToJumpApex * config.TimeToJumpApex;

            Assert.That(height, Is.EqualTo(config.JumpHeight).Within(0.001f));
            Object.DestroyImmediate(config);
        }
    }
}
