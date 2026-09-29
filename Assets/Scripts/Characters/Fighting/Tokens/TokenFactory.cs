public static class TokenFactory
{
    public static IToken Create(string tokenType, int strength, int duration)
    {
        if (duration <= 0)
            return null;

        if (tokenType == BurningToken.TypeId)
            return new BurningToken(strength, duration);
        if (tokenType == SatietyToken.TypeId)
            return new SatietyToken(strength, duration);
        if (tokenType == HydrationToken.TypeId)
            return new HydrationToken(strength, duration);
        if (tokenType == SanityToken.TypeId)
            return new SanityToken(strength, duration);

        return null;
    }
}
