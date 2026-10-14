using UnityEngine;

public class PoisonToken : Token, IToken
{
    public const string TypeId = "Poison";

    int remainingTurns;

    public string TokenType => TypeId;
    public string Name() => "Zatrucie";
    public int Strength { get; private set; }
    public int Duration => remainingTurns;
    public bool IsExpired => remainingTurns <= 0;

    static Sprite image;

    public string Description()
    {
        return "Postać jest zatruta."
            + "\n\nSiła: " + Strength + ", " + DamageTypes.Label(DamageType.Poison)
            + "\n\nPozostało tur: " + remainingTurns;
    }

    public Sprite TokenImage()
    {
        if (image == null)
            image = Resources.Load<Sprite>("Tokens/Poison");
        return image;
    }

    public PoisonToken(int strength, int durationTurns)
    {
        Strength = strength < 0 ? 0 : strength;
        remainingTurns = durationTurns;
    }

    public void Tick(IDamagable target)
    {
        if (IsExpired || target == null)
        {
            DropIfExpired();
            return;
        }

        if (Random.value < 0.2f)
        {
            DamageResult result = DamageCalculator.Direct(target, DamageType.Poison, Strength);
            if (result.Amount > 0)
                target.TakeDamage(result.Amount, result.Type, result.Scale);
        }

        remainingTurns--;
        DropIfExpired();
    }

    public bool Merge(IToken other)
    {
        if (other is not PoisonToken incoming)
            return false;

        Strength += incoming.Strength;
        remainingTurns += incoming.Duration;
        return true;
    }
}
