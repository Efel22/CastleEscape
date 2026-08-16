using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using Unity.Mathematics;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.Tilemaps;

// This allows for separation of the Unity Instance and Prefabs
// ?: If I were to edit any value of the mainGridPosition (Ex: RoomData[1]) instances then
//    they'd change for ALL of those mainGridPosition[1] instances instead!
// ?: Allows for per-room mainGridPosition editing 
public class GeneratedRoom
{
    
    // Holds Room Tile Data and Anchor Position
    public RoomData roomTemplate;

    // This value is set when the room is generated 
    public Vector2Int mainGridPosition;

    // Stores the direction from which the room was generated from 
    public Direction generatedFromDirection; 

    // Using a constructor so code is cleaner
    public GeneratedRoom(RoomData _roomTemplate, Vector2Int _mainGridPosition, Direction _generatedFromDirection)
    {
        this.roomTemplate = _roomTemplate;
        this.mainGridPosition = _mainGridPosition;
        this.generatedFromDirection = _generatedFromDirection;
    }

}

public enum Direction
{
    Up,
    Down,
    Left,
    Right
}

public class _RoomGenerator : MonoBehaviour
{
    // ************************
    // Scenery Settings
    // ************************

    [SerializeField] private SceneryData[] sceneries;

    // Holds the Chosen Scenery's Index
    private int chosenSceneryIndex;

    // ************************
    // Room Settings
    // ************************
    [Header("Room Settings")] 
     
    [SerializeField] private int roomSize = 15;

    // How many rooms to generate.
    // Number MUST go from 2-9
    // ?: If roomCount > 9 then it opens the possibility of a 
    //    room not having a validDirection and not generate an
    //    exit tile
    [Range(2,9)]
    [SerializeField] private int roomCount = 5;

    // Gap between rooms.
    [Range(1,5)]
    [SerializeField] private int tileGapBetweenRooms = 1;

    [Header("Hall Settings")] 

    // Tile used to create halls.
    private TileBase hallTile;
    
    // Hall Width and Height added to the Halls
    
    [Range(1,3)]
    [SerializeField] private int hallSize; 

    // ************************
    // Tracking Lists 
    // ************************

    private List<GeneratedRoom> generatedRooms = new List<GeneratedRoom>(); 

    // ************************
    // References
    // ************************

    [Header("References")] 

    private RoomData[] rooms;
    private SpawnRoomData[] spawnRooms;
    private EndRoomData[] endRooms;

    [SerializeField] private Grid mainGrid;

    [SerializeField] private Tilemap mainTilemap;
    [SerializeField] private Tilemap decorTilemap;

    [SerializeField] private Transform player;

    [SerializeField] private _CollisionTilesGenerator collisionTileGenComp;
    [SerializeField] private GameObject exitEntityRef;
    [SerializeField] private Camera mainCamera;

    // ************************
    // Start
    // ************************

    void Start()
    {

        // ************************************
        //            ERROR LOGGING
        // ************************************
        if (mainTilemap == null)
        {
            Debug.LogError("mainTilemap is NULL"); return;
        }
        if (collisionTileGenComp == null)
        {
            Debug.LogError("collisionTileGenComp is NULL"); return;
        }
        if (exitEntityRef == null)
        {
            Debug.LogError("exitEntityRef is NULL"); return;
        }
        if (sceneries == null)
        {
            Debug.LogError("sceneries is NULL"); return;
        }

        
        GenerateDungeon();
    }

    // ************************
    // Choose Scenery
    // ************************

    void ChooseScenery()
    {
        chosenSceneryIndex = UnityEngine.Random.Range(0,sceneries.Length);
        
        // ************************************
        //            ERROR LOGGING
        // ************************************
        if (sceneries[chosenSceneryIndex].rooms.Length == 0)
        {
            Debug.LogError("Chosen Scenery's Generic Rooms is EMPTY"); return;
        }
        if (sceneries[chosenSceneryIndex].spawnRooms.Length == 0)
        {
            Debug.LogError("Chosen Scenery's Spawn Rooms is EMPTY"); return;
        }
        if (sceneries[chosenSceneryIndex].endRooms.Length == 0)
        {
            Debug.LogError("Chosen Scenery's End Rooms is EMPTY"); return;
        }
        if (sceneries[chosenSceneryIndex].hallTile == null)
        {
            Debug.LogError("Chosen Scenery's hallTile is NULL"); return;
        }

        // ?: Copy the hall tile into the current hallTile
        //    to be used
        hallTile = sceneries[chosenSceneryIndex].hallTile;

        // ?: Copy each type of Room from the Scenery into 
        //    the current rooms to be used 

        rooms = sceneries[chosenSceneryIndex].rooms;
        spawnRooms = sceneries[chosenSceneryIndex].spawnRooms;
        endRooms = sceneries[chosenSceneryIndex].endRooms;

        SetBackgroundColor();
    }

    // ************************
    // Set Background Color
    // ************************
    private void SetBackgroundColor()
    {
        
        // ************************************
        //            ERROR LOGGING
        // ************************************
        if (sceneries[chosenSceneryIndex].backgroundColor == null)
        {
            Debug.LogError("Chosen Scenery's backgroundColor is NULL"); return;
        }
        if (mainCamera == null)
        {
            Debug.LogError("mainCamera is NULL"); return;
        }

        mainCamera.backgroundColor = sceneries[chosenSceneryIndex].backgroundColor;
        
    }


    // ************************
    // Generate Dungeon
    // ************************
    public void GenerateDungeon()
    {
        
        // ********************************************
        //      INITIAL ROOM GEN. & PLAYER SPAWN
        // ********************************************

        ChooseScenery();

        // Reset state from any previous generation
        generatedRooms.Clear();

        // Store the first room as it will be used to teleport the player to its
        // spawn location value (exclusive to SpawnRoomData class itself)
        RoomData initialSpawnRoom = GetRandomRoom(RoomType.Spawn);

        // Generate the Initial Room at 0,0
        GenerateRoomAt(new Vector2Int(0,0), Direction.Down, initialSpawnRoom);

        // Cast the room (since its RoomData) to SpawnRoomData to access the player spawn position
        Vector2Int spawnPos = ((SpawnRoomData)initialSpawnRoom).playerSpawnPosition;

        // Set the player's position to the spawn Position's values
        player.position = mainGrid.CellToWorld(new Vector3Int(spawnPos.x, spawnPos.y, 0));

        // *************************************
        //       SETUP VALUES FOR ROOM GEN.
        // *************************************

        // *REQUIRED in Direction Check Foreach Loop! Stores all the possible directions
        Direction[] directions = {Direction.Up, Direction.Down, Direction.Left, Direction.Right};

        // Create a list of the available Positions
        List<Vector2Int> availablePositions = new List<Vector2Int>();
        
        // Stores the directions from which the room was generated from chosen
        // when generating the room (used in hall generation)
        List<Direction> availableDirections = new List<Direction>(); 

        // Determine what kind of room should generate...
        RoomType roomType = RoomType.Generic;

        // **************************************
        //             ROOM GENERATION
        // **************************************
        for (
            int roomsGeneratedCounter = 1; 
            roomsGeneratedCounter < roomCount; 
            roomsGeneratedCounter++
        )
        {
            
            // Check each direction and add position AND direction if valid
            foreach (Direction currentDirection in directions)
            {
                Vector2Int PlaceRoomAtPos_Check = 
                    generatedRooms[roomsGeneratedCounter - 1].mainGridPosition + GetDirectionOffset(currentDirection);
                
                if (!IsPositionOccupied(PlaceRoomAtPos_Check))
                {
                    availablePositions.Add(PlaceRoomAtPos_Check);
                    availableDirections.Add(currentDirection);
                }
            }

            // If no available positions, then break from loop
            if (availablePositions.Count == 0) break;

            int chosenIndex = UnityEngine.Random.Range(0, availablePositions.Count);

            // If this is the LAST room...  then
            if (generatedRooms.Count == roomCount - 1) 
                roomType = RoomType.End;

            // Generate a random room at the new position with the offsets 
            GenerateRoomAt(
                availablePositions[chosenIndex],
                availableDirections[chosenIndex], 
                GetRandomRoom(roomType));

            // Clear the Lists 
            // (*Clearing the lists is better for perfomance rather than allocating them INSIDE this loop)
            availablePositions.Clear();
            availableDirections.Clear();

        }

        // **************************************
        //            HALL GENERATION
        // **************************************
        for (
            int hallsGeneratedCounter = 1; 
            hallsGeneratedCounter < generatedRooms.Count; // All rooms may NOT generate, so don't use 'roomCount'
            hallsGeneratedCounter++
        )
        {
            
            Direction directionFrom_RoomB = generatedRooms[hallsGeneratedCounter].generatedFromDirection;
            GeneratedRoom RoomA = generatedRooms[hallsGeneratedCounter - 1];
            GeneratedRoom RoomB = generatedRooms[hallsGeneratedCounter];

            GenerateHall(directionFrom_RoomB, RoomA, RoomB);
        }
        
    }

    
    // ************************
    // Get Random Room
    // ************************
    RoomData GetRandomRoom(RoomType room = RoomType.Generic) { 
        
        switch (room)
        {
            
            case RoomType.Spawn:
                return spawnRooms[UnityEngine.Random.Range(0, spawnRooms.Length)]; 

            case RoomType.End:
                return endRooms[UnityEngine.Random.Range(0, endRooms.Length)]; 

            default:
                return rooms[UnityEngine.Random.Range(0, rooms.Length)]; 
        }

    }

    // ************************
    // Is Position Occupied
    // ************************    
    bool IsPositionOccupied(Vector2Int posToCheck)
    {

        foreach (GeneratedRoom room in generatedRooms)
        {
            if (room.mainGridPosition == posToCheck)
            {
                return true;
            }
        }

        return false;
    }

    // ************************
    // Get Direction Offset
    // ************************
    Vector2Int GetDirectionOffset(Direction direction)
    {

        switch (direction)
        {
            // Up    (0, +Y)
            case Direction.Up:
                return new Vector2Int(0, roomSize + tileGapBetweenRooms);

            // Down  (0, -Y)
            case Direction.Down:
                return new Vector2Int(0, -(roomSize + tileGapBetweenRooms));

            // Left  (-X, 0)
            case Direction.Left:
                return new Vector2Int(-(roomSize + tileGapBetweenRooms), 0);

            // Right (+X, 0)
            default:
                return new Vector2Int(roomSize + tileGapBetweenRooms, 0 );
        }
    }

    // ************************
    // Random Direction Offset
    // ************************
    Vector2Int GetRandomDirectionOffset()
    {
        int direction = UnityEngine.Random.Range(0, 4);

        switch (direction)
        {
            // Up    (0, +Y)
            case 0:
                return new Vector2Int(0, roomSize + tileGapBetweenRooms);

            // Down  (0, -Y)
            case 1:
                return new Vector2Int(0, -(roomSize + tileGapBetweenRooms));

            // Left  (-X, 0)
            case 2:
                return new Vector2Int(-(roomSize + tileGapBetweenRooms), 0);

            // Right (+X, 0)
            default:
                return new Vector2Int(roomSize + tileGapBetweenRooms, 0 );
        }
        
        // int direction = Random.Range(0, 4);

        // switch (direction)
        // {
        //     case 0:
        //         return Vector2Int.up;

        //     case 1:
        //         return Vector2Int.down;

        //     case 2:
        //         return Vector2Int.left;

        //     default:
        //         return Vector2Int.right;
        // }
    }


    // ************************
    // Generate Room
    // ************************
    public void GenerateRoomAt(Vector2Int placeRoomAt, Direction directionFrom, RoomData roomToPlace)
    {
        // Make sure the room exists.
        if (roomToPlace == null) return;

        // *********************************
        //         ROOM COPY SETUP 
        // *********************************

        // Copy the room's 15x15 tiles.
        for (int posx_room = 0; posx_room < roomSize; posx_room++)
        {
            for (int posy_room = 0; posy_room < roomSize; posy_room++)
            {
                // Position of the tile inside the room.
                Vector3Int positionInsideRoom =
                    new Vector3Int(posx_room, posy_room, 0);

                
                // Get the tile from the room.
                TileBase tile = roomToPlace.tilemap.GetTile(positionInsideRoom);

                // Get the DECOR tile from the room
                TileBase decorTile = roomToPlace.decorTilemap.GetTile(positionInsideRoom);

                // Position of the tile in the MainGrid.
                Vector3Int mainGridPosition =
                    new Vector3Int(
                        placeRoomAt.x + posx_room,
                        placeRoomAt.y + posy_room,
                        0
                    );

                // --- WALL TILE PLACEMENT ---
                if (tile != null)
                {
                    mainTilemap.SetTile(
                        mainGridPosition,
                        tile
                    );
                }

                // --- DECOR TILE PLACEMENT ---
                if (
                    decorTile != null &&
                    UnityEngine.Random.Range(0f,1f) > roomToPlace.decorRemovalChance
                )
                {
                    decorTilemap.SetTile(
                        mainGridPosition,
                        decorTile
                    );
                }
            }
        }

        // Add the room to place to the generated rooms list
        // and store its location in the main grid
        GeneratedRoom roomToRegister = new GeneratedRoom(roomToPlace, placeRoomAt, directionFrom);
        generatedRooms.Add(roomToRegister);

        // Check if the room is an EndRoom...
        if (roomToPlace is EndRoomData endRoomData)
        {
            // Store the endRoomTile's position in the room
            Vector2Int exitTilePos_inRoom = endRoomData.exitTilePosition;

            // Store the endRoomTile's position in the mainGrid
            Vector3Int exitTilePos_mainGrid = new Vector3Int(
                placeRoomAt.x + exitTilePos_inRoom.x,
                placeRoomAt.y + exitTilePos_inRoom.y,
                0
            );

            // Convert to WORLD space
            Vector3 exitTilePos_World = mainGrid.CellToWorld(exitTilePos_mainGrid);

            // Spawn the Exit Entity Ref
            Instantiate(exitEntityRef, exitTilePos_World, Quaternion.identity);

        }

    }

    public void GenerateHall(Direction directionFrom, GeneratedRoom RoomA, GeneratedRoom RoomB
    )
    {
        // Used to determine from which X,Y -> W,Z positions the SetTilesBlock()
        // function will fill
        BoundsInt bounds = new BoundsInt();

        // Now, we need the Anchor Positions in the MAIN GRID
        // *REMEMBER RoomA = Previous-Room & RoomB = Current-Room
        Vector2Int AnchorA_MainGridPos = RoomA.roomTemplate.anchorPosition + RoomA.mainGridPosition;

        switch (directionFrom)
        {
            // *******************************
            //              UP
            // *******************************
            case Direction.Up:

                bounds.SetMinMax(
                    new Vector3Int(
                        AnchorA_MainGridPos.x - hallSize,            // minX (this value is INCLUSIVE in SetMinMax() )
                        RoomA.mainGridPosition.y + roomSize,         // minY (this value is INCLUSIVE in SetMinMax() )
                        0), // *REQUIRED for 2D, else it won't work
                    new Vector3Int(
                        AnchorA_MainGridPos.x + (hallSize + 1),      // maxX (this value is EXCLUSIVE in SetMinMax() )
                        RoomB.mainGridPosition.y,                    // maxY (this value is EXCLUSIVE in SetMinMax() )
                        1) // *REQUIRED for 2D, else it won't work
                );

                break;

            // *******************************
            //             DOWN
            // *******************************
            case Direction.Down:

                bounds.SetMinMax(
                    new Vector3Int(
                        AnchorA_MainGridPos.x - (hallSize),          // minX (this value is INCLUSIVE in SetMinMax() )
                        RoomB.mainGridPosition.y + roomSize,         // minY (this value is INCLUSIVE in SetMinMax() )
                        0), // *REQUIRED for 2D, else it won't work
                    new Vector3Int(
                        AnchorA_MainGridPos.x + (hallSize + 1),      // maxX (this value is EXCLUSIVE in SetMinMax() )
                        RoomA.mainGridPosition.y,                    // maxY (this value is EXCLUSIVE in SetMinMax() )
                        1) // *REQUIRED for 2D, else it won't work
                );

                break;

            // *******************************
            //             LEFT
            // *******************************
            case Direction.Left:

                bounds.SetMinMax(
                    new Vector3Int(
                        RoomB.mainGridPosition.x + roomSize,          // minX (this value is INCLUSIVE in SetMinMax() )
                        AnchorA_MainGridPos.y - (hallSize),           // minY (this value is INCLUSIVE in SetMinMax() )
                        0), // *REQUIRED for 2D, else it won't work
                    new Vector3Int(
                        RoomA.mainGridPosition.x,                     // maxX (this value is EXCLUSIVE in SetMinMax() )
                        AnchorA_MainGridPos.y + (hallSize + 1),       // maxY (this value is EXCLUSIVE in SetMinMax() )
                        1) // *REQUIRED for 2D, else it won't work
                );

                break;
            
            // *******************************
            //         RIGHT (DEFAULT)
            // *******************************
            default:

                bounds.SetMinMax(
                    new Vector3Int(
                        RoomA.mainGridPosition.x + roomSize,          // minX (this value is INCLUSIVE in SetMinMax() )
                        AnchorA_MainGridPos.y - (hallSize),           // minY (this value is INCLUSIVE in SetMinMax() )
                        0), // *REQUIRED for 2D, else it won't work
                    new Vector3Int(
                        RoomB.mainGridPosition.x,                     // maxX (this value is EXCLUSIVE in SetMinMax() )
                        AnchorA_MainGridPos.y + (hallSize + 1),       // maxY (this value is EXCLUSIVE in SetMinMax() )
                        1) // *REQUIRED for 2D, else it won't work
                );  

                break;
        }

        // *REQUIRED for SetTilesBlock(), the size is WIDTH * HEIGHT
        TileBase[] tiles = new TileBase[bounds.size.x * bounds.size.y];

        // Since only the hallTile is going to be used for halls, fill all of 'tiles'
        // with the hallTile *REQUIRED for SetTilesBlock()
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i] = hallTile;
        }

        // Final Step, after the bounds have been calculated, set the tiles onto the tilemap
        // ?: Why use SetTileBlock()? Unity Documentation says its more performant this way
        // https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Tilemaps.Tilemap.SetTilesBlock.html
        mainTilemap.SetTilesBlock(bounds, tiles);

    }

}

