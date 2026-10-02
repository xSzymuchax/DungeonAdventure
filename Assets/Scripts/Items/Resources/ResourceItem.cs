public class ResourceItem : Item
{
    public override ItemKind Kind => ItemKind.Resource;
    public override bool TracksDurability => false;

    public ResourceItem(string id, string displayName) : base(id, displayName)
    {
    }

    public override Item Copy()
    {
        ResourceItem copy = new ResourceItem(Id, DisplayName);
        copy.FillFrom(this);
        return copy;
    }
}
