using System.Collections.Generic;

public class Ammunition : Item
{
    public override ItemKind Kind => ItemKind.Ammunition;
    public int Damage { get; set; }
    public WeaponKind Launcher { get; set; }
    public List<OnHitEffect> OnHit { get; } = new();
    public int TotalMax => Count * PieceMax;

    int PieceMax => MaxDurability < 1 ? Consts.DEFAULT_DURABILITY : MaxDurability;

    public Ammunition(string id, string displayName) : base(id, displayName)
    {
    }

    public void Merge(Ammunition other)
    {
        if (other == null || other.Count < 1)
            return;

        int piece = PieceMax;
        int wear = Wear(piece) + other.Wear(piece);
        Count += other.Count;
        Durability = Count * piece - wear;
        Collapse();
    }

    public Ammunition SplitOne()
    {
        if (Count <= 1)
            return null;

        int piece = PieceMax;
        int wear = Wear(piece);
        Count--;
        int remainMax = Count * piece;
        if (wear > remainMax)
            wear = remainMax;
        Durability = remainMax - wear;

        Ammunition one = new Ammunition(Id, DisplayName);
        one.ViewPrefab = ViewPrefab;
        one.MaxDurability = piece;
        one.Durability = piece;
        one.Count = 1;
        one.Damage = Damage;
        one.Launcher = Launcher;
        CopyOnHit(one);
        return one;
    }

    public bool SpendUse()
    {
        Durability--;
        if (Durability > 0)
            return true;

        Count = 0;
        Durability = 0;
        return false;
    }

    public void RestoreDurability(int savedDurability, int savedMax)
    {
        if (savedMax > 0)
            MaxDurability = savedMax;
        if (MaxDurability < 1)
            MaxDurability = Consts.DEFAULT_DURABILITY;
        if (Count < 1)
            Count = 1;

        int total = Count * MaxDurability;
        Durability = savedMax <= 0 ? total : UnityEngine.Mathf.Clamp(savedDurability, 0, total);
        Collapse();
    }

    public override Item Copy()
    {
        Ammunition copy = new Ammunition(Id, DisplayName);
        copy.FillFrom(this);
        copy.Damage = Damage;
        copy.Launcher = Launcher;
        CopyOnHit(copy);
        copy.Count = Count;
        copy.Durability = Durability;
        return copy;
    }

    void CopyOnHit(Ammunition target)
    {
        for (int i = 0; i < OnHit.Count; i++)
        {
            if (OnHit[i] != null)
                target.OnHit.Add(OnHit[i]);
        }
    }

    void Collapse()
    {
        int piece = PieceMax;
        int wear = Wear(piece);
        int count = Count;
        while (count > 0 && wear >= piece)
        {
            count--;
            wear -= piece;
        }

        Count = count;
        Durability = count * piece - wear;
    }

    int Wear(int piece)
    {
        int wear = Count * piece - Durability;
        return wear < 0 ? 0 : wear;
    }
}
