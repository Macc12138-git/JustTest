namespace JustTest.Game.Input
{
    internal sealed class BufferedButton
    {
        private float pressedAt = float.NegativeInfinity;
        private bool consumed = true;

        internal void Press(float timestamp)
        {
            pressedAt = timestamp;
            consumed = false;
        }

        internal bool IsAvailable(float timestamp, float bufferDuration)
        {
            return !consumed &&
                   timestamp >= pressedAt &&
                   timestamp - pressedAt <= bufferDuration;
        }

        internal bool TryConsume(float timestamp, float bufferDuration)
        {
            if (!IsAvailable(timestamp, bufferDuration))
            {
                return false;
            }

            consumed = true;
            return true;
        }

        internal void Consume()
        {
            consumed = true;
        }

        internal void Clear()
        {
            pressedAt = float.NegativeInfinity;
            consumed = true;
        }
    }
}
