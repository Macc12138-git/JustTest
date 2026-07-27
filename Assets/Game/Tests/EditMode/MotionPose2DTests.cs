using JustTest.Game.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace JustTest.Game.Tests
{
    public sealed class MotionPose2DTests
    {
        [Test]
        public void ComposeAdditive_AddsOffsetsAndRotationsAndMultipliesScales()
        {
            EvaluatedMotionPose2D basePose = new EvaluatedMotionPose2D(
                new Vector2(1f, 2f),
                10f,
                new Vector2(2f, 3f),
                new Vector2(3f, 4f),
                20f,
                new Vector2(4f, 5f),
                new Vector2(5f, 6f),
                30f,
                new Vector2(6f, 7f));
            EvaluatedMotionPose2D feedbackPose = new EvaluatedMotionPose2D(
                new Vector2(-0.25f, 0.5f),
                -2f,
                new Vector2(0.5f, 2f),
                new Vector2(0.75f, -0.5f),
                -4f,
                new Vector2(2f, 0.5f),
                new Vector2(-1f, 1.5f),
                -6f,
                new Vector2(0.25f, 3f));

            EvaluatedMotionPose2D result = EvaluatedMotionPose2D.ComposeAdditive(
                basePose,
                feedbackPose);

            Assert.That(result.BodyOffset, Is.EqualTo(new Vector2(0.75f, 2.5f)));
            Assert.That(result.BodyRotation, Is.EqualTo(8f));
            Assert.That(result.BodyScale, Is.EqualTo(new Vector2(1f, 6f)));
            Assert.That(result.MainWeaponOffset, Is.EqualTo(new Vector2(3.75f, 3.5f)));
            Assert.That(result.MainWeaponRotation, Is.EqualTo(16f));
            Assert.That(result.MainWeaponScale, Is.EqualTo(new Vector2(8f, 2.5f)));
            Assert.That(result.OffhandWeaponOffset, Is.EqualTo(new Vector2(4f, 7.5f)));
            Assert.That(result.OffhandWeaponRotation, Is.EqualTo(24f));
            Assert.That(result.OffhandWeaponScale, Is.EqualTo(new Vector2(1.5f, 21f)));
        }
    }
}
