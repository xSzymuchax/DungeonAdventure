using UnityEngine;

public class HydrationToken : Token, IToken
{
    public const string TypeId = "Hydration";

    int remainingTurns;

    public string TokenType => TypeId;
    public string Name() => "Napicie";
    public int Strength { get; private set; }
    public int Duration => remainingTurns;
    public bool IsExpired => remainingTurns <= 0;

    static Sprite image;

    public string Description()
    {
        return "Postać jest napojona."
            + "\n\nSiła: " + Strength + ", Napicie"
            + "\n\nPozostało tur: " + remainingTurns;
    }

    public Sprite TokenImage()
    {
        if (image == null)
            image = Resources.Load<Sprite>("Tokens/Hydration");
        return image;
    }

    public HydrationToken(int strength, int durationTurns)
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
            player.SetHydration(player.Hydration + Strength);

        remainingTurns--;
        DropIfExpired();
    }

    public bool Merge(IToken other)
    {
        if (other is not HydrationToken incoming)
            return false;

        remainingTurns += incoming.Duration;
        return true;
    }
}
