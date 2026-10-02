using System.Collections;
using System.Collections.Generic;

public class Scroll : Item
{
    public override ItemKind Kind => ItemKind.Scroll;
    public override bool IsConsumable => true;
    public override bool TracksDurability => false;
    public List<Effect> Effects { get; } = new();

    public Scroll(string id, string displayName) : base(id, displayName)
    {
    }

    public IEnumerator UseScroll(PlayerCharacter player, Position2D target)
    {
        ItemGenerator.EnsureEffects(this);
        yield return Effect.ApplyAll(Effects, player, target);
    }

    public override Item Copy()
    {
        Scroll copy = new Scroll(Id, DisplayName);
        copy.FillFrom(this);
        copy.Effects.AddRange(Effects);
        return copy;
    }
}
