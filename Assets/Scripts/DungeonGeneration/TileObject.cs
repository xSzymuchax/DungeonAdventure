using UnityEngine;

public class TileObject : MonoBehaviour
{
    public Position2D Position { get; private set; }
    public TileInfo Tile { get; private set; }

    public void Place(TileInfo tile)
    {
        if (tile == null || Tile == tile)
            return;

        Lift();
        if (tile.tileObject != null && tile.tileObject != this)
            tile.tileObject.Lift();

        Position = tile.position;
        Tile = tile;
        tile.SetTileObject(this);
    }

    public void Lift()
    {
        Detach();
        transform.SetParent(null, true);
    }

    void OnDestroy()
    {
        Detach();
    }

    void Detach()
    {
        if (Tile == null)
            return;

        Tile.ClearTileObject(this);
        Tile = null;
    }
}
