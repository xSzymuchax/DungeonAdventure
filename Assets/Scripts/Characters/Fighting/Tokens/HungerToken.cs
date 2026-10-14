using UnityEngine;

public class HungerToken : Token, IToken
{
    public const string TypeId = "Hunger";

    int remainingTurns;

    public string TokenType => TypeId;
    public string Name() => "Głód";
    public int Strength { get; private set; }
    public int Duration => remainingTurns;
    public bool IsExpired => remainingTurns <= 0;

    static Sprite image;

    public string Description()
    {
        return "Postać ma większe zapotrzebowanie kalorii."
            + "\n\nSiła: " + Strength + ", Głód"
            + "\n\nPozostało tur: " + remainingTurns;
    }

    public Sprite TokenImage()
    {
        if (image == null)
            image = Resources.Load<Sprite>("Tokens/Hunger");
        return image;
    }

    public HungerToken(int strength, int durationTurns)
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
            player.SetSatiety(player.Satiety - Strength);

        remainingTurns--;
        DropIfExpired();
    }

    public bool Merge(IToken other)
    {
        if (other is not HungerToken incoming)
            return false;

        remainingTurns += incoming.Duration;
        return true;
    }
}
