using UnityEngine;

public class LowHydrationToken : Token, IToken
{
    public const string TypeId = "LowHydration";

    public string TokenType => TypeId;
    public string Name() => "Lekkie pragnienie";
    public int Strength => 0;
    public int Duration => 1;
    public bool IsExpired => false;

    static Sprite image;

    public string Description()
    {
        return "Postać odczuwa lekkie pragnienie. Mana wraca wolniej.";
    }

    public Sprite TokenImage()
    {
        if (image == null)
            image = Resources.Load<Sprite>("Tokens/LowHydration");
        return image;
    }

    public LowHydrationToken(int strength, int durationTurns)
    {
    }

    public void Tick(IDamagable target)
    {
    }

    public bool Merge(IToken other)
    {
        return other is LowHydrationToken;
    }
}
