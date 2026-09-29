public class SanityToken : IToken
{
    public const string TypeId = "Sanity";

    int remainingTurns;

    public string TokenType => TypeId;
    public int Strength { get; }
    public int Duration => remainingTurns;
    public bool IsExpired => remainingTurns <= 0;

    public SanityToken(int strength, int durationTurns)
    {
        Strength = strength;
        remainingTurns = durationTurns;
    }

    public void Tick(IDamagable target)
    {
        if (IsExpired || target == null)
            return;

        if (target is PlayerCharacter player)
            player.SetSanity(player.Sanity + Strength);

        remainingTurns--;
    }
}
