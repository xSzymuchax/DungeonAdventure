using UnityEngine;

public class HydrationToken : IToken
{
    public const string TypeId = "Hydration";

    int remainingTurns;

    public string TokenType => TypeId;
    public string Name() => "Napicie";
    public int Strength { get; }
    public int Duration => remainingTurns;
    public bool IsExpired => remainingTurns <= 0;

    static Sprite image;

    public string Description()
    {
        return "Przywraca nawodnienie."
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
        Strength = strength;
        remainingTurns = durationTurns;
    }

    public void Tick(IDamagable target)
    {
        if (IsExpired || target == null)
            return;

        if (target is PlayerCharacter player)
            player.SetHydration(player.Hydration + Strength);

        remainingTurns--;
    }
}
