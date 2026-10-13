using System.Collections;
using UnityEngine;

public abstract class CraftBench : TileObject
{
    public virtual string Title => "Stół";

    public virtual string Description()
    {
        return "";
    }

    public virtual Sprite Picture()
    {
        return Portrait.Of(this);
    }

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
