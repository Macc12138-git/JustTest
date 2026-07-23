using JustTest.Game.Player;
using NUnit.Framework;
using UnityEngine;

namespace JustTest.Game.Tests
{
    public sealed class PlayerFacingIndicatorConfigTests
    {
        [Test]
        public void Defaults_CreateReadableHeadMountedIndicator()
        {
            PlayerFacingIndicatorConfig config =
                ScriptableObject.CreateInstance<PlayerFacingIndicatorConfig>();

            Assert.That(config.Visible, Is.True);
            Assert.That(config.LocalOffset.y, Is.EqualTo(1.25f));
            Assert.That(config.Scale, Is.EqualTo(1f));
            Assert.That(config.Color.a, Is.EqualTo(1f));
            Assert.That(config.SortingOrder, Is.EqualTo(30));

            Object.DestroyImmediate(config);
        }
    }
}
