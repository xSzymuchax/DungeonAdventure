using UnityEngine;

public class StrongHungerToken : Token, IToken
{
    public const string TypeId = "StrongHunger";

    public string TokenType => TypeId;
    public string Name() => "Silny głód";
    public int Strength => 0;
    public int Duration => 1;
    public bool IsExpired => false;

    static Sprite image;

    public string Description()
    {
        return "Postać odczuwa silny głód. Zdrowie nie wraca.";
    }

    public Sprite TokenImage()
    {
        if (image == null)
            image = Resources.Load<Sprite>("Tokens/StrongHunger");
        return image;
    }

    public StrongHungerToken(int strength, int durationTurns)
    {
    }

    public void Tick(IDamagable target)
    {
    }

    public bool Merge(IToken other)
    {
        return other is StrongHungerToken;
    }
}
