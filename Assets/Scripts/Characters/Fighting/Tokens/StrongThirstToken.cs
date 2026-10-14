using UnityEngine;

public class StrongThirstToken : Token, IToken
{
    public const string TypeId = "StrongThirst";

    public string TokenType => TypeId;
    public string Name() => "Silne pragnienie";
    public int Strength => 0;
    public int Duration => 1;
    public bool IsExpired => false;

    static Sprite image;

    public string Description()
    {
        return "Postać odczuwa silne pragnienie. Mana nie wraca.";
    }

    public Sprite TokenImage()
    {
        if (image == null)
            image = Resources.Load<Sprite>("Tokens/StrongThirst");
        return image;
    }

    public StrongThirstToken(int strength, int durationTurns)
    {
    }

    public void Tick(IDamagable target)
    {
    }

    public bool Merge(IToken other)
    {
        return other is StrongThirstToken;
    }
}
