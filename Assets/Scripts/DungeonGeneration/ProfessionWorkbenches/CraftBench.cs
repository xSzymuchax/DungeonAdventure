using System.Collections;

public abstract class CraftBench : TileObject
{
    public abstract void Open(PlayerCharacter player);
    public abstract bool CanCraft();
    public abstract void ClosePanel();
    public abstract bool Craft();
    public virtual IEnumerator AfterCraft()
    {
        yield break;
    }
    public virtual void ReturnAll(PlayerInventory inventory) {}
}
