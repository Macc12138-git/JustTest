namespace JustTest.Game.Combat
{
    internal readonly struct HitResolution
    {
        internal HitResolution(in HitRequest request, in HitResult result)
        {
            Request = request;
            Result = result;
        }

        internal HitRequest Request { get; }

        internal HitResult Result { get; }
    }
}
