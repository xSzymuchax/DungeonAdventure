public static class TokenFactory
{
    public static IToken Create(string tokenType, int strength, int duration)
    {
        if (duration <= 0)
            return null;

        if (tokenType == BurningToken.TypeId)
            return new BurningToken(strength, duration);
        if (tokenType == ChillToken.TypeId)
            return new ChillToken(strength, duration);
        if (tokenType == FrozenToken.TypeId)
            return new FrozenToken(strength, duration);
        if (tokenType == PoisonToken.TypeId)
            return new PoisonToken(strength, duration);
        if (tokenType == BleedingToken.TypeId)
            return new BleedingToken(strength, duration);
        if (tokenType == SatietyToken.TypeId)
            return new SatietyToken(strength, duration);
        if (tokenType == HydrationToken.TypeId)
            return new HydrationToken(strength, duration);
        if (tokenType == SanityToken.TypeId)
            return new SanityToken(strength, duration);
        if (tokenType == HungerToken.TypeId)
            return new HungerToken(strength, duration);
        if (tokenType == ThirstToken.TypeId)
            return new ThirstToken(strength, duration);
        if (tokenType == MadnessToken.TypeId)
            return new MadnessToken(strength, duration);
        if (tokenType == LowSatietyToken.TypeId)
            return new LowSatietyToken(strength, duration);
        if (tokenType == StrongHungerToken.TypeId)
            return new StrongHungerToken(strength, duration);
        if (tokenType == StarvingToken.TypeId)
            return new StarvingToken(strength, duration);
        if (tokenType == LowHydrationToken.TypeId)
            return new LowHydrationToken(strength, duration);
        if (tokenType == StrongThirstToken.TypeId)
            return new StrongThirstToken(strength, duration);
        if (tokenType == DehydratedToken.TypeId)
            return new DehydratedToken(strength, duration);

        return null;
    }
}
