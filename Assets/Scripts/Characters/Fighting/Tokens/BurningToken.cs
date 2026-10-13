using UnityEngine;

public class BurningToken : IToken
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
        return "Co turę zadaje obrażenia od ognia."
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
            return;

        int damage = Random.Range(DamagePerTurn - 1, DamagePerTurn + 1);
        DamageResult result = DamageCalculator.Direct(target, DamageType.Fire, damage);
        if (result.Amount > 0)
            target.TakeDamage(result.Amount, result.Type, result.Scale);
        RemainingTurns--;
    }
}
