using UnityEngine;

public class FrozenToken : Token, IToken
{
    public const string TypeId = "Frozen";
    public const int FreezeTurns = 5;
    public const int ColdDamage = 2;

    int remainingTurns;

    public string TokenType => TypeId;
    public string Name() => "Zamrożenie";
    public int Strength => ColdDamage;
    public int Duration => remainingTurns;
    public bool IsExpired => remainingTurns <= 0;

    static Sprite image;

    public string Description()
    {
        return "Postać zamarzła. Nie może podejmować działań aż do odmrożenia."
            + "\n\nSiła: " + ColdDamage + ", " + DamageTypes.Label(DamageType.Cold)
            + "\n\nPozostało tur: " + remainingTurns;
    }

    public Sprite TokenImage()
    {
        if (image == null)
            image = Resources.Load<Sprite>("Tokens/Frozen");
        return image;
    }

    public FrozenToken()
        : this(0, FreezeTurns)
    {
    }

    public FrozenToken(int strength, int durationTurns)
    {
        remainingTurns = durationTurns;
    }

    public void Tick(IDamagable target)
    {
        if (IsExpired || target == null)
        {
            DropIfExpired();
            return;
        }

        DamageResult result = DamageCalculator.Direct(target, DamageType.Cold, ColdDamage);
        if (result.Amount > 0)
            target.TakeDamage(result.Amount, result.Type, result.Scale);
        remainingTurns--;
        DropIfExpired();
    }

    public bool Merge(IToken other)
    {
        if (other is FrozenToken || other is ChillToken)
        {
            remainingTurns = FreezeTurns;
            return true;
        }

        if (other is BurningToken)
        {
            RemoveTokens(token => token is BurningToken || token is ChillToken || token is FrozenToken);
            return true;
        }

        return false;
    }
}
