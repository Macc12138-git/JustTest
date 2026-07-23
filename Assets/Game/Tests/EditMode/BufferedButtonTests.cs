using JustTest.Game.Input;
using NUnit.Framework;

namespace JustTest.Game.Tests
{
    public sealed class BufferedButtonTests
    {
        [Test]
        public void Press_IsAvailableUntilBufferExpires()
        {
            BufferedButton button = new BufferedButton();

            button.Press(1f);

            Assert.That(button.IsAvailable(1.09f, 0.1f), Is.True);
            Assert.That(button.IsAvailable(1.11f, 0.1f), Is.False);
        }

        [Test]
        public void Consume_PreventsSecondUse()
        {
            BufferedButton button = new BufferedButton();
            button.Press(2f);

            Assert.That(button.TryConsume(2.05f, 0.1f), Is.True);
            Assert.That(button.TryConsume(2.06f, 0.1f), Is.False);
        }

        [Test]
        public void NewPress_RearmsConsumedButton()
        {
            BufferedButton button = new BufferedButton();
            button.Press(3f);
            button.Consume();

            button.Press(4f);

            Assert.That(button.IsAvailable(4f, 0.1f), Is.True);
        }
    }
}
