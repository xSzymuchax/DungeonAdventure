using UnityEngine;

public class LowSatietyToken : Token, IToken
{
    public const string TypeId = "LowSatiety";

    public string TokenType => TypeId;
    public string Name() => "Lekki głód";
    public int Strength => 0;
    public int Duration => 1;
    public bool IsExpired => false;

    static Sprite image;

    public string Description()
    {
        return "Postać odczuwa lekki głód. Zdrowie wraca wolniej.";
    }

    public Sprite TokenImage()
    {
        if (image == null)
            image = Resources.Load<Sprite>("Tokens/LowSatiety");
        return image;
    }

    public LowSatietyToken(int strength, int durationTurns)
    {
    }

    public void Tick(IDamagable target)
    {
    }

    public bool Merge(IToken other)
    {
        return other is LowSatietyToken;
    }
}
