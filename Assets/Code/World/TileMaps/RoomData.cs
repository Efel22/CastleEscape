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

    [Header("General Settings")] 

    // The room's main Tilemap. (walls)
    public Tilemap tilemap;

    // Decoration Tilemap
    public Tilemap decorTilemap;

    [Header("Anchor Settings")] 

    // Position of the room's entrance.
    public Vector2Int anchorPosition;

    [Header("Decoration Settings")] 

    // Removal chance for each individual decorTile 
    // (1f = No Decor) 
    // (0f = All Decor) 
    // (0.5f = Medium amount of Decor)
    [Range(0f,1f)]
    [SerializeField] public float decorRemovalChance = 0.5f;

    [Header("Torch Settings")] 

    // Where to spawn the Torch Prefabs
    public List<Vector2Int> TorchPositions = new List<Vector2Int>();  

}