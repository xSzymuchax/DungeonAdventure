using UnityEngine;

public class ChillToken : Token, IToken
{
    public const string TypeId = "Chill";
    public const int TurnsToFreeze = 20;

    int remainingTurns;

    public string TokenType => TypeId;
    public string Name() => "Oziębienie";
    public int Strength { get; private set; }
    public int Duration => remainingTurns;
    public bool IsExpired => remainingTurns <= 0;

    static Sprite image;

    public string Description()
    {
        return "Postać jest wychłodzona. Działa wolniej."
            + "\n\nPozostało tur: " + remainingTurns;
    }

    public Sprite TokenImage()
    {
        if (image == null)
            image = Resources.Load<Sprite>("Tokens/Chill");
        return image;
    }

    public ChillToken(int strength, int durationTurns)
    {
        Strength = strength < 0 ? 0 : strength;
        remainingTurns = durationTurns;
    }

    public override void OnApply()
    {
        if (remainingTurns >= TurnsToFreeze)
            BecomeFrozen();
    }

    public void Tick(IDamagable target)
    {
        if (IsExpired)
        {
            DropIfExpired();
            return;
        }

        remainingTurns--;
        DropIfExpired();
    }

    public bool Merge(IToken other)
    {
        if (other is ChillToken incoming)
        {
            if (HostIsFrozen())
                return false;

            remainingTurns += incoming.Duration;
            if (remainingTurns >= TurnsToFreeze)
                BecomeFrozen();
            return true;
        }

        if (other is BurningToken)
        {
            RemoveTokens(token => token is BurningToken || token is ChillToken || token is FrozenToken);
            return true;
        }

        return false;
    }

    bool HostIsFrozen()
    {
        if (Host == null)
            return false;

        var active = Host.ActiveTokens;
        for (int i = 0; i < active.Count; i++)
        {
            if (active[i] is FrozenToken)
                return true;
        }

        return false;
    }

    void BecomeFrozen()
    {
        Character host = Host;
        if (host == null)
            return;

        host.RemoveToken(this);
        host.AddToken(new FrozenToken());
    }
}
