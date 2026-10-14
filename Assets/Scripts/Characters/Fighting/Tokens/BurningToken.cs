using UnityEngine;

public class BurningToken : Token, IToken
{
    public const string TypeId = "Burning";

    public string TokenType => TypeId;
    public string Name() => "Podpalenie";
    public int Strength => DamagePerTurn;
    public int Duration => RemainingTurns;
    public int DamagePerTurn { get; }
    public int RemainingTurns { get; private set; }
    public bool IsExpired => RemainingTurns <= 0;

    static Sprite image;

    public string Description()
    {
        int low = DamagePerTurn - 1;
        int high = DamagePerTurn + 1;
        string power = low == high ? low.ToString() : low + "–" + high;
        return "Postać płonie."
            + "\n\nSiła: " + power + ", " + DamageTypes.Label(DamageType.Fire)
            + "\n\nPozostało tur: " + RemainingTurns;
    }

    public Sprite TokenImage()
    {
        if (image == null)
            image = Resources.Load<Sprite>("Tokens/Burning");
        return image;
    }

    public BurningToken(int damagePerTurn, int durationTurns) : this()
    {
    }

    public BurningToken()
    {
        DamagePerTurn = 3;
        RemainingTurns = 20;
    }

    public void Tick(IDamagable target)
    {
        if (IsExpired || target == null)
        {
            DropIfExpired();
            return;
        }

        int damage = Random.Range(DamagePerTurn - 1, DamagePerTurn + 1);
        DamageResult result = DamageCalculator.Direct(target, DamageType.Fire, damage);
        if (result.Amount > 0)
            target.TakeDamage(result.Amount, result.Type, result.Scale);
        RemainingTurns--;
        DropIfExpired();
    }

    public bool Merge(IToken other)
    {
        if (other is BurningToken incoming)
        {
            RemainingTurns = Mathf.Min(RemainingTurns + incoming.Duration, 20);
            return true;
        }

        if (other is ChillToken || other is FrozenToken)
        {
            RemoveTokens(token => token is BurningToken || token is ChillToken || token is FrozenToken);
            return true;
        }

        return false;
    }
}
