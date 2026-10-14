using UnityEngine;

public class MadnessToken : Token, IToken
{
    public const string TypeId = "Madness";

    int remainingTurns;

    public string TokenType => TypeId;
    public string Name() => "Obłęd";
    public int Strength { get; private set; }
    public int Duration => remainingTurns;
    public bool IsExpired => remainingTurns <= 0;

    static Sprite image;

    public string Description()
    {
        return "Postać popada w obłęd."
            + "\n\nSiła: " + Strength + ", Obłęd"
            + "\n\nPozostało tur: " + remainingTurns;
    }

    public Sprite TokenImage()
    {
        if (image == null)
            image = Resources.Load<Sprite>("Tokens/Madness");
        return image;
    }

    public MadnessToken(int strength, int durationTurns)
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
            player.SetSanity(player.Sanity - Strength);

        remainingTurns--;
        DropIfExpired();
    }

    public bool Merge(IToken other)
    {
        if (other is not MadnessToken incoming)
            return false;

        remainingTurns += incoming.Duration;
        return true;
    }
}
