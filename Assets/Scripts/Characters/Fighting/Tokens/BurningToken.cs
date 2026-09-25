public class BurningToken : IToken
{
    public const string TypeId = "Burning";

    public string TokenType => TypeId;
    public int Strength => DamagePerTurn;
    public int Duration => RemainingTurns;
    public int DamagePerTurn { get; }
    public int RemainingTurns { get; private set; }
    public bool IsExpired => RemainingTurns <= 0;

    public BurningToken(int damagePerTurn, int durationTurns)
    {
        DamagePerTurn = damagePerTurn;
        RemainingTurns = durationTurns;
    }

    public void Tick(IDamagable target)
    {
        if (IsExpired || target == null)
            return;

        target.TakeDamage(DamagePerTurn);
        RemainingTurns--;
    }
}
