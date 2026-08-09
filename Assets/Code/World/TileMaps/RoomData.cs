using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomData : MonoBehaviour
{
    
    public Grid grid;
    public Tilemap tilemap;

    public TileBase upTile;
    public TileBase downTile;
    public TileBase leftTile;
    public TileBase rightTile;

    public TileBase replaceArrowsWithTile;

    public TileBase startRoomTile;
    public TileBase endRoomTile;

    public Vector3Int GetStartPosition()
    {
        foreach (Vector3Int position in tilemap.cellBounds.allPositionsWithin)
        {
            if (tilemap.GetTile(position) == startRoomTile)
            {
                return position;
            }
        }

        return Vector3Int.zero;
    }

}
