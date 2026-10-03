using System.Collections;
using System.Collections.Generic;

public class Scroll : Item
{
    public override ItemKind Kind => ItemKind.Scroll;
    public override bool IsConsumable => true;
    public override bool TracksDurability => false;
    public List<Effect> Effects { get; } = new();
    public int SpellLevel { get; set; } = 1;

    public Scroll(string id, string displayName) : base(id, displayName)
    {
    }

    public IEnumerator UseScroll(PlayerCharacter player, Position2D target)
    {
        ItemGenerator.EnsureEffects(this);
        ArmSpellLevel();
        try
        {
            yield return Effect.ApplyAll(Effects, player, target);
        }
        finally
        {
            DisarmSpellLevel();
        }
    }

    public override Item Copy()
    {
        Scroll copy = new Scroll(Id, DisplayName);
        copy.FillFrom(this);
        copy.SpellLevel = SpellLevel;
        copy.Effects.AddRange(Effects);
        return copy;
    }

    void ArmSpellLevel()
    {
        for (int i = 0; i < Effects.Count; i++)
        {
            if (Effects[i] is SkillEffect effect && effect.skill != null)
                effect.skill.BeginCast(SpellLevel);
        }
    }

    void DisarmSpellLevel()
    {
        for (int i = 0; i < Effects.Count; i++)
        {
            if (Effects[i] is SkillEffect effect && effect.skill != null)
                effect.skill.EndCast();
        }
    }
}
