using UnityEngine;

public readonly struct DamageResult
{
    public readonly bool Dodged;
    public readonly int Amount;
    public readonly float Scale;
    public readonly DamageType Type;

    public DamageResult(bool dodged, int amount, float scale, DamageType type)
    {
        Dodged = dodged;
        Amount = amount;
        Scale = scale;
        Type = type;
    }
}

public static class DamageCalculator
{
    public static int MinPotential(float baseAmount)
    {
        return Bound(baseAmount, Consts.DAMAGE_ROLL_LOW);
    }

    public static int MaxPotential(float baseAmount)
    {
        return Bound(baseAmount, Consts.DAMAGE_ROLL_HIGH);
    }

    public static float AttackBase(DamageType type, float power, int attack, float amplify)
    {
        if (DamageTypes.IsMagical(type))
            return Mathf.Max(0f, power) * (1f + amplify / 100f);

        return attack;
    }

    public static int Roll(float baseAmount)
    {
        if (baseAmount <= 0f)
            return 0;

        float rolled = baseAmount * Random.Range(Consts.DAMAGE_ROLL_LOW, Consts.DAMAGE_ROLL_HIGH);
        return Mathf.FloorToInt(rolled + 0.5f);
    }

    public static DamageResult Melee(Character attacker, IDamagable target, DamageType type, float power)
    {
        CharacterStats stats = Stats(attacker);
        int attack = stats != null ? stats.Damage : 0;
        float baseAmount = AttackBase(type, power, attack, Amplify(attacker));

        return Strike(attacker, target, type, baseAmount);
    }

    public static DamageResult Projectile(Character attacker, IDamagable target, float thrownDamage)
    {
        return Strike(attacker, target, DamageType.Physical, Mathf.Max(0f, thrownDamage));
    }

    public static bool WearsArmor(DamageType type)
    {
        return !DamageTypes.IsUnavoidable(type);
    }

    public static DamageResult Direct(IDamagable target, DamageType type, float amount)
    {
        int mitigated = Mitigate(target, type, amount);
        return new DamageResult(false, mitigated, 1f, type);
    }

    static DamageResult Strike(Character attacker, IDamagable target, DamageType type, float baseAmount)
    {
        if (Dodged(attacker, target, type))
            return new DamageResult(true, 0, 1f, type);

        int min = MinPotential(baseAmount);
        int max = MaxPotential(baseAmount);
        int rolled = Roll(baseAmount);
        bool critical = Critical(attacker);
        float scale = critical ? Consts.CRITICAL_PLATE_SCALE : PlateScale(rolled, min, max);
        if (critical)
            rolled *= 2;
        int amount = Mitigate(target, type, rolled);
        return new DamageResult(false, amount, scale, type);
    }

    static float PlateScale(int rolled, int min, int max)
    {
        if (max <= min)
            return 1f;

        float t = Mathf.InverseLerp(min, max, rolled);
        return Mathf.Lerp(Consts.DAMAGE_ROLL_LOW, Consts.DAMAGE_ROLL_HIGH, t);
    }

    static int Bound(float baseAmount, float factor)
    {
        if (baseAmount <= 0f)
            return 0;

        return Mathf.FloorToInt(baseAmount * factor + 0.5f);
    }

    static bool Critical(Character attacker)
    {
        CharacterStats stats = Stats(attacker);
        if (stats == null || stats.CriticalChance <= 0)
            return false;

        return Random.value * 100f < stats.CriticalChance;
    }

    static bool Dodged(Character attacker, IDamagable target, DamageType type)
    {
        if (type != DamageType.Physical || target is not Character defender)
            return false;

        CharacterStats stats = defender.GetStats();
        if (stats == null || stats.Dodge <= 0)
            return false;

        int counter = 0;
        CharacterStats attackerStats = Stats(attacker);
        if (attackerStats != null)
            counter = attackerStats.CounterDodge;

        int chance = Mathf.Max(0, stats.Dodge - counter);
        if (chance <= 0 || Random.value * 100f >= chance)
            return false;

        Debug.Log(defender.name + " dodged");
        return true;
    }

    static int Mitigate(IDamagable target, DamageType type, float amount)
    {
        if (amount <= 0f)
            return 0;

        CharacterStats stats = target is Character character ? character.GetStats() : null;
        if (stats != null && stats.Resistance(type) <= -1)
            amount *= 2f;

        if (stats != null && stats.Resistance(type) >= 1)
            return 0;

        if (DamageTypes.IsUnavoidable(type))
            return Round(amount);

        if (!DamageTypes.IsMagical(type))
        {
            if (stats == null)
                return Round(amount);

            float physical = amount - stats.Defense;
            if (physical <= 0f || stats.Block <= 0)
                return Round(physical);

            if (Random.value * 100f < stats.Block)
                physical *= 0.5f;
            return Round(physical);
        }

        float percent = stats != null ? Mathf.Clamp(stats.MagicResistance, 0f, 100f) : 0f;
        float kept = amount * (100f - percent) / 100f;
        return Mathf.CeilToInt(kept);
    }

    static int Round(float amount)
    {
        if (amount <= 0f)
            return 0;
        return Mathf.FloorToInt(amount + 0.5f);
    }

    static float Amplify(Character attacker)
    {
        CharacterStats stats = Stats(attacker);
        return stats != null ? stats.MagicAmplify : 0f;
    }

    static CharacterStats Stats(Character character)
    {
        return character != null ? character.GetStats() : null;
    }
}
