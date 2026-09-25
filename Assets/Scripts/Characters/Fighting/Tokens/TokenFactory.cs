public static class TokenFactory
{
    public static IToken Create(string tokenType, int strength, int duration)
    {
        if (duration <= 0)
            return null;

        if (tokenType == BurningToken.TypeId)
            return new BurningToken(strength, duration);

        return null;
    }
}
