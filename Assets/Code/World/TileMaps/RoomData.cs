using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomData : MonoBehaviour
{
    // The room's Tilemap.
    public Tilemap tilemap;

    // Position of the room's entrance.
    public Vector2Int anchorPosition;
}