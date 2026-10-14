using UnityEngine;

public class ThirstToken : Token, IToken
{
    public const string TypeId = "Thirst";

    int remainingTurns;

    public string TokenType => TypeId;
    public string Name() => "Pragnienie";
    public int Strength { get; private set; }
    public int Duration => remainingTurns;
    public bool IsExpired => remainingTurns <= 0;

    static Sprite image;

    public string Description()
    {
        return "Postać odczuwa duże pragnienie."
            + "\n\nSiła: " + Strength + ", Pragnienie"
            + "\n\nPozostało tur: " + remainingTurns;
    }

    public Sprite TokenImage()
    {
        if (image == null)
            image = Resources.Load<Sprite>("Tokens/Thirst");
        return image;
    }

    public ThirstToken(int strength, int durationTurns)
    {
        Strength = 1;
        remainingTurns = durationTurns;
    }

    public void Tick(IDamagable target)
    {
        if (IsExpired || target == null)
        {
            DropIfExpired();
            return;
        }

        if (target is PlayerCharacter player)
            player.SetHydration(player.Hydration - Strength);

        remainingTurns--;
        DropIfExpired();
    }

    public bool Merge(IToken other)
    {
        if (other is not ThirstToken incoming)
            return false;

        remainingTurns += incoming.Duration;
        return true;
    }
}
