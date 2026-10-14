using UnityEngine;

public class StarvingToken : Token, IToken
{
    public const string TypeId = "Starving";

    public string TokenType => TypeId;
    public string Name() => "Wygłodzenie";
    public int Strength => 0;
    public int Duration => 1;
    public bool IsExpired => false;

    static Sprite image;

    public string Description()
    {
        return "Postać jest wygłodzona. Głód zbija zdrowie.";
    }

    public Sprite TokenImage()
    {
        if (image == null)
            image = Resources.Load<Sprite>("Tokens/Starving");
        return image;
    }

    public StarvingToken(int strength, int durationTurns)
    {
    }

    public void Tick(IDamagable target)
    {
    }

    public bool Merge(IToken other)
    {
        return other is StarvingToken;
    }
}
