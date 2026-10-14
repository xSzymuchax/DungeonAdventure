using UnityEngine;

public class DehydratedToken : Token, IToken
{
    public const string TypeId = "Dehydrated";

    public string TokenType => TypeId;
    public string Name() => "Odwodnienie";
    public int Strength => 0;
    public int Duration => 1;
    public bool IsExpired => false;

    static Sprite image;

    public string Description()
    {
        return "Postać jest odwodniona. Pragnienie zabiera manę.";
    }

    public Sprite TokenImage()
    {
        if (image == null)
            image = Resources.Load<Sprite>("Tokens/Dehydrated");
        return image;
    }

    public DehydratedToken(int strength, int durationTurns)
    {
    }

    public void Tick(IDamagable target)
    {
    }

    public bool Merge(IToken other)
    {
        return other is DehydratedToken;
    }
}
