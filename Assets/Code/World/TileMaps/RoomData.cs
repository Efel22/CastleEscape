using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public enum RoomType
{
    Spawn,
    End,
    Generic
}

public class RoomData : MonoBehaviour
{

    // The room's main Tilemap. (walls)
    public Tilemap tilemap;

    // Decoration Tilemap
    public Tilemap decorTilemap;

    // Position of the room's entrance.
    public Vector2Int anchorPosition;

    // Where to spawn the Torch Prefabs
    public List<Vector2Int> TorchPositions = new List<Vector2Int>();  

}