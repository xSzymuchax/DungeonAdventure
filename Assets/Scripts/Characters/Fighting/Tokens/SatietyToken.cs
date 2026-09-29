public class SatietyToken : IToken
{
    public const string TypeId = "Satiety";

    int remainingTurns;

    public string TokenType => TypeId;
    public int Strength { get; }
    public int Duration => remainingTurns;
    public bool IsExpired => remainingTurns <= 0;

    public SatietyToken(int strength, int durationTurns)
    {
        Strength = strength;
        remainingTurns = durationTurns;
    }

    public void Tick(IDamagable target)
    {
        if (IsExpired || target == null)
            return;

        if (target is PlayerCharacter player)
            player.SetSatiety(player.Satiety + Strength);

        remainingTurns--;
    }
}
