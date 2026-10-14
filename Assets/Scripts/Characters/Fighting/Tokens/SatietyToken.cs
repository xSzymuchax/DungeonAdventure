using UnityEngine;

public class SatietyToken : Token, IToken
{
    public const string TypeId = "Satiety";

    int remainingTurns;

    public string TokenType => TypeId;
    public string Name() => "Najedzenie";
    public int Strength { get; private set; }
    public int Duration => remainingTurns;
    public bool IsExpired => remainingTurns <= 0;

    static Sprite image;

    public string Description()
    {
        return "Postać czuje się najedzona."
            + "\n\nSiła: " + Strength + ", Najedzenie"
            + "\n\nPozostało tur: " + remainingTurns;
    }

    public Sprite TokenImage()
    {
        if (image == null)
            image = Resources.Load<Sprite>("Tokens/Satiety");
        return image;
    }

    public SatietyToken(int strength, int durationTurns)
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
            player.SetSatiety(player.Satiety + Strength);

        remainingTurns--;
        DropIfExpired();
    }

    public bool Merge(IToken other)
    {
        if (other is not SatietyToken incoming)
            return false;

        remainingTurns += incoming.Duration;
        return true;
    }
}
