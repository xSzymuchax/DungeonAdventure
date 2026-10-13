using UnityEngine;

public class SatietyToken : IToken
{
    public const string TypeId = "Satiety";

    int remainingTurns;

    public string TokenType => TypeId;
    public string Name() => "Najedzenie";
    public int Strength { get; }
    public int Duration => remainingTurns;
    public bool IsExpired => remainingTurns <= 0;

    static Sprite image;

    public string Description()
    {
        return "Zwiększa sytość."
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
        Strength = strength;
        remainingTurns = durationTurns;
    }

    public void Tick(IDamagable target)
    {
        if (IsExpired || target == null)
            return;

        if (target is PlayerCharacter player)
            player.SetSatiety(player.Satiety + Strength);

        remainingTurns--;
    }
}
