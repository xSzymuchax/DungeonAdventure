using UnityEngine;

public class BleedingToken : Token, IToken
{
    public const string TypeId = "Bleeding";

    float wound;

    public string TokenType => TypeId;
    public string Name() => "Krwawienie";
    public int Strength => FullPoint(wound);
    public int Duration => wound > 0f ? 1 : 0;
    public bool IsExpired => wound <= 0f;

    static Sprite image;

    public string Description()
    {
        return "Postać krwawi."
            + "\n\nSiła: " + FullPoint(wound) + ", " + DamageTypes.Label(DamageType.Bleeding)
            + "\n\nTrwa do zagojenia.";
    }

    public Sprite TokenImage()
    {
        if (image == null)
            image = Resources.Load<Sprite>("Tokens/Bleeding");
        return image;
    }

    public BleedingToken(int strength, int durationTurns)
    {
        wound = strength < 0 ? 0f : strength;
    }

    public void Tick(IDamagable target)
    {
        if (IsExpired || target == null)
        {
            DropIfExpired();
            return;
        }

        int damage = FullPoint(wound);
        DamageResult result = DamageCalculator.Direct(target, DamageType.Bleeding, damage);
        if (result.Amount > 0)
            target.TakeDamage(result.Amount, result.Type, result.Scale);

        wound -= Mathf.Log(wound + 1f) / 2.5f;
        if (FullPoint(wound) <= 0)
            wound = 0f;
        DropIfExpired();
    }

    public bool Merge(IToken other)
    {
        if (other is not BleedingToken incoming)
            return false;

        if (incoming.wound > wound)
            wound = incoming.wound;
        return true;
    }

    static int FullPoint(float value)
    {
        if (float.IsNaN(value) || value <= 0f)
            return 0;
        return Mathf.FloorToInt(value + 0.5f);
    }
}
