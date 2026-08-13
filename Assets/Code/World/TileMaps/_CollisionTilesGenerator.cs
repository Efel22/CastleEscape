using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// *****************************************************
//             Collision Tiles Generator
//  
//  ?: Sets the collision tiles of the CollisionTilemap 
//  ?: Called from '_RoomGenerator.cs' after Dungeon has been Generated
//  ?: (IN THE FUTURE?) Handles real time collision tile removal/addition
//
// *****************************************************
public class _CollisionTilesGenerator : MonoBehaviour
{
    
    // Main Tilemap for this script, it will hold ONLY a specific tile that is invisible but has
    // a 16x16p collision box
    [SerializeField] private Tilemap collisionTilemap;

    // Represents the collisionTile 
    // (*will probably be invisible on shipping builds)
    [SerializeField] private TileBase collisionTile; 

    // Tile that will be evaluated for collision creation 
    [SerializeField] private TileBase evaluatedTile; 


    // ************************
    // Create Collision Tiles
    // ************************
    public void CreateCollisionTiles(
        List<GeneratedRoom> allRooms,
        int roomSize,
        Tilemap wallTilemap
    )
    {

        // Safety Checks
        if (collisionTile == null) return;
        if (evaluatedTile == null) return;
        if (collisionTilemap == null) return;
        if (wallTilemap == null) return;
        if (allRooms.Count == 0) return;
        if (roomSize <= 0) return;

        // 1. Get the room's mainGridPosition and store (minBound/LPV = Lowest Possible Value)
        // (since all rooms generate UP-RIGHT), the mainGridPosition will suffice
        Vector2Int minBound = new Vector2Int(int.MaxValue,int.MaxValue);
        // 2. Get the highest possible X and Y Positions Value + roomSize and store (maxBound/HPV = Highest Possible Value)
        Vector2Int maxBound = new Vector2Int(int.MinValue,int.MinValue);

        // roomCtr = Counter
        for (int roomCtr = 0 ; roomCtr < allRooms.Count; roomCtr++)
        {
            
            // Get LPV.x
            if (allRooms[roomCtr].mainGridPosition.x < minBound.x)
            {
                minBound.x = allRooms[roomCtr].mainGridPosition.x; 
            }

            // Get LPV.y
            if (allRooms[roomCtr].mainGridPosition.y < minBound.y)
            {
                minBound.y = allRooms[roomCtr].mainGridPosition.y; 
            }
            
            // Get HPV.x
            if (allRooms[roomCtr].mainGridPosition.x > maxBound.x)
            {
                maxBound.x = allRooms[roomCtr].mainGridPosition.x; 
            }

            // Get HPV.y
            if (allRooms[roomCtr].mainGridPosition.y > maxBound.y)
            {
                maxBound.y = allRooms[roomCtr].mainGridPosition.y; 
            }

        }

        // Finish Setup of maxBound/HPV
        maxBound.x += roomSize;
        maxBound.y += roomSize;

        // 3. Iterate from the LPV to the HPV on the WallsTilemap and check if any of the tiles
        //    has air on one of its 8 neighboor cells, if so, place the collisionTile

        // Iterate through X-Axis...
        for (int i = minBound.x; i < maxBound.x; i++)
        {
            // Iterate through Y-Axis...
            for (int j = minBound.y; j < maxBound.y; j++)
            {
                // If the tile at 'Position i,j' is a WallTile AND is near a border then place it
                if (
                    wallTilemap.GetTile(new Vector3Int(i,j,0)) == evaluatedTile && 
                    IsTileBorder(wallTilemap, new Vector2Int(i,j))
                    )
                {
                    collisionTilemap.SetTile(new Vector3Int(i,j,0), collisionTile);
                }
            }
        }
    }

    
    // ************************
    // Is Tile Border
    // ************************
    public bool IsTileBorder(Tilemap wallTilemap, Vector2Int position)
    {
        
        if (wallTilemap.GetTile(new Vector3Int(position.x - 1, position.y - 1, 0)) != evaluatedTile)
            return true;
        if (wallTilemap.GetTile(new Vector3Int(position.x - 1, position.y    , 0)) != evaluatedTile)
            return true;
        if (wallTilemap.GetTile(new Vector3Int(position.x    , position.y - 1, 0)) != evaluatedTile)
            return true;
        if (wallTilemap.GetTile(new Vector3Int(position.x + 1, position.y + 1, 0)) != evaluatedTile)
            return true;
        if (wallTilemap.GetTile(new Vector3Int(position.x + 1, position.y    , 0)) != evaluatedTile)
            return true;
        if (wallTilemap.GetTile(new Vector3Int(position.x    , position.y + 1, 0)) != evaluatedTile)
            return true;
        if (wallTilemap.GetTile(new Vector3Int(position.x + 1, position.y - 1, 0)) != evaluatedTile)
            return true;
        if (wallTilemap.GetTile(new Vector3Int(position.x - 1, position.y + 1, 0)) != evaluatedTile)
            return true;

        return false;
    }

    // bool IsWithinBounds()
    // {
        
    // }

}
