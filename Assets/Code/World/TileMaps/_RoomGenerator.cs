using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
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
    // Room Settings
    // ************************

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

    // Tile used to create halls.
    [SerializeField] private TileBase hallTile;
    
    // Hall Width and Height added to the Halls
    [SerializeField, Min(1)] private int hallSize; 

    // Removal chance for each individual decorTile 
    // (1f = No Decor) 
    // (0f = All Decor) 
    // (0.5f = Medium amount of Decor)
    [Range(0f,1f)]
    [SerializeField] private float decorRemovalChance = 0.5f;

    // // Used to represent decoration in the map
    // [SerializeField] private TileBase decoTile;

    // ************************
    // Tracking Lists 
    // ************************

    private List<GeneratedRoom> generatedRooms = new List<GeneratedRoom>(); 

    // ************************
    // References
    // ************************

    [SerializeField] private RoomData[] rooms;
    [SerializeField] private SpawnRoomData[] spawnRooms;
    [SerializeField] private EndRoomData[] endRooms;

    [SerializeField] private Grid mainGrid;

    [SerializeField] private Tilemap mainTilemap;
    [SerializeField] private Tilemap decorTilemap;

    [SerializeField] private Transform player;

    [SerializeField] private _CollisionTilesGenerator collisionTileGenComp;

    // ************************
    // Start
    // ************************

    void Start()
    {
        GenerateDungeon();
        if (collisionTileGenComp != null) 
            collisionTileGenComp.CreateCollisionTiles(generatedRooms, roomSize, mainTilemap);
    }


    // ************************
    // Dungeon Generation
    // ************************
    void GenerateDungeon()
    {
        
        // ********************************************
        //      INITIAL ROOM GEN. & PLAYER SPAWN
        // ********************************************

        // Store the first room as it will be used to teleport the player to its
        // spawn location value (exclusive to SpawnRoomData class itself)
        RoomData initialSpawnRoom = GetRandomRoom(RoomType.Spawn);

        // Generate the Initial Room at 0,0
        GenerateRoomAt(new Vector2Int(0,0), Direction.Down, initialSpawnRoom);

        // Cast the room (since its RoomData) to SpawnRoomData to access the player spawn position
        Vector2Int spawnPos = ((SpawnRoomData)initialSpawnRoom).playerSpawnPosition;

        // Set the player's position to the spawn Position's values
        player.position = mainGrid.CellToWorld(new Vector3Int(spawnPos.x, spawnPos.y, 0));

        // **************************************
        //             ROOM GENERATION
        // **************************************
        for (
            int roomsGeneratedCounter = 1; 
            roomsGeneratedCounter < roomCount; 
            roomsGeneratedCounter++
        )
        {
            
            // Create a list of the available Positions
            List<Vector2Int> availablePositions = new List<Vector2Int>();
            
            // Stores a temporary value for a 2D Position 
            // (will be overwritten multiple times)
            Vector2Int PlaceRoomAt_Pos_Temp;

            // Stores the directions from which the room was generated from chosen
            // when generating the room (used in hall generation)
            List<Direction> availableDirections = new List<Direction>(); 

            // CHECK FOR UP:
            PlaceRoomAt_Pos_Temp = 
                generatedRooms[roomsGeneratedCounter - 1].mainGridPosition + GetDirectionOffset(Direction.Up);

            if (!IsPositionOccupied(PlaceRoomAt_Pos_Temp))
            {
                availablePositions.Add(PlaceRoomAt_Pos_Temp);
                availableDirections.Add(Direction.Up);
            }
            
            // CHECK FOR DOWN:
            PlaceRoomAt_Pos_Temp = 
                generatedRooms[roomsGeneratedCounter - 1].mainGridPosition + GetDirectionOffset(Direction.Down);

            if (!IsPositionOccupied(PlaceRoomAt_Pos_Temp))
            {
                availablePositions.Add(PlaceRoomAt_Pos_Temp);
                availableDirections.Add(Direction.Down);
            }
            
            // CHECK FOR LEFT:
            PlaceRoomAt_Pos_Temp = 
                generatedRooms[roomsGeneratedCounter - 1].mainGridPosition + GetDirectionOffset(Direction.Left);

            if (!IsPositionOccupied(PlaceRoomAt_Pos_Temp)) 
            {
                availablePositions.Add(PlaceRoomAt_Pos_Temp);
                availableDirections.Add(Direction.Left);
            }
            
            // CHECK FOR RIGHT:
            PlaceRoomAt_Pos_Temp = 
                generatedRooms[roomsGeneratedCounter - 1].mainGridPosition + GetDirectionOffset(Direction.Right);

            if (!IsPositionOccupied(PlaceRoomAt_Pos_Temp))
            {
                availablePositions.Add(PlaceRoomAt_Pos_Temp);
                availableDirections.Add(Direction.Right);
            }

            // If no available positions, then break from loop
            if (availablePositions.Count == 0) break;

            int chosenIndex = Random.Range(0, availablePositions.Count);

            // Determine what kind of room should generate...
            RoomType roomType = RoomType.Generic;

            // If this is the LAST room...  then
            if (generatedRooms.Count == roomCount - 1) 
                roomType = RoomType.End;

            // Generate a random room at the new position with the offsets 
            GenerateRoomAt(
                availablePositions[chosenIndex],
                availableDirections[chosenIndex], 
                GetRandomRoom(roomType));

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
                return spawnRooms[Random.Range(0, spawnRooms.Length)]; 

            case RoomType.End:
                return endRooms[Random.Range(0, endRooms.Length)]; 

            default:
                return rooms[Random.Range(0, rooms.Length)]; 
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
        int direction = Random.Range(0, 4);

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

                // Copy the tile if one exists.
                if (tile != null)
                {
                    mainTilemap.SetTile(
                        mainGridPosition,
                        tile
                    );
                }

                // bool isTallDecorTile = false;

                // if ()

                if (
                    decorTile != null &&
                    Random.Range(0f,1f) > decorRemovalChance
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
    }

    public void GenerateHall(Direction directionFrom, GeneratedRoom RoomA, GeneratedRoom RoomB
    )
    {

        // *******************************
        //              UP
        // *******************************
        if (directionFrom == Direction.Up)
        {
            // Store the Anchor Points positions
            Vector2Int AnchorA_Pos = RoomA.roomTemplate.anchorPosition;

            // Store the Rooms Positions in the MAIN GRID
            Vector2Int RoomA_MainGridPos = RoomA.mainGridPosition;
            Vector2Int RoomB_MainGridPos = RoomB.mainGridPosition;

            // Now, we need the Anchor Positions in the MAIN GRID
            Vector2Int AnchorA_MainGridPos = AnchorA_Pos + RoomA_MainGridPos;

            // Where does it start placing Tiles on the Y Axis?:
            int startPlacingTile_Pos = RoomA_MainGridPos.y + roomSize;

            // Where does it stop placing Tiles on the Y Axis?:
            int stopPlacingTile_Pos = RoomB_MainGridPos.y;

            // How much in the X axis does it move            
            for (
                int i = AnchorA_MainGridPos.x - (hallSize); 
                i <= AnchorA_MainGridPos.x + (hallSize); 
                i++
            )
            {
                // How in the Y axis does it move
                for(
                    int j = startPlacingTile_Pos;
                    j < stopPlacingTile_Pos;
                    j++
                )
                {
                    // Place the tile if one exists.
                    if (hallTile != null)
                    {
                        mainTilemap.SetTile(
                            new Vector3Int(i, j),
                            hallTile
                        );
                    }   
                }
            }
        }

        // *******************************
        //              DOWN
        // *******************************
        if (directionFrom == Direction.Down)
        {
            // Store the Anchor Points positions
            Vector2Int AnchorA_Pos = RoomA.roomTemplate.anchorPosition;

            // Store the Rooms Positions in the MAIN GRID
            Vector2Int RoomA_MainGridPos = RoomA.mainGridPosition;
            Vector2Int RoomB_MainGridPos = RoomB.mainGridPosition;

            // Now, we need the Anchor Positions in the MAIN GRID
            Vector2Int AnchorA_MainGridPos = AnchorA_Pos + RoomA_MainGridPos;

            // *anchor A Position > anchor B position

            // Where does it start placing Tiles on the Y Axis?:
            int startPlacingTile_Pos = RoomB_MainGridPos.y + roomSize; 

            // Where does it stop placing Tiles on the Y Axis?:
            int stopPlacingTile_Pos = RoomA_MainGridPos.y;

            // How much in the X axis does it move            
            for (
                int i = AnchorA_MainGridPos.x - (hallSize); 
                i <= AnchorA_MainGridPos.x + (hallSize); 
                i++
            )
            {
                // How in the Y axis does it move
                for(
                    int j = startPlacingTile_Pos;
                    j < stopPlacingTile_Pos;
                    j++
                )
                {
                    // Place the tile if one exists.
                    if (hallTile != null)
                    {
                        mainTilemap.SetTile(
                            new Vector3Int(i, j),
                            hallTile
                        );
                    }   
                }
            }
        }

        // *******************************
        //              RIGHT
        // *******************************
        if (directionFrom == Direction.Right)
        {

            // We need the Anchor Position in the MAIN GRID for ROOM A
            Vector2Int AnchorA_MainGridPos = RoomA.roomTemplate.anchorPosition + RoomA.mainGridPosition;

            // --- X AXIS ---
            // Where does it start placing Tiles on the X Axis?:
            int startPlacingTile_Pos_onX = RoomA.mainGridPosition.x + roomSize;

            // Where does it stop placing Tiles on the X Axis?:
            int stopPlacingTile_Pos_onX = RoomB.mainGridPosition.x;

            // --- Y AXIS ---
            // Where does it start placing Tiles on the Y Axis?:
            int startPlacingTile_Pos_onY = AnchorA_MainGridPos.y - (hallSize);

            // Where does it stop placing Tiles on the Y Axis?:
            int stopPlacingTile_Pos_onY = AnchorA_MainGridPos.y + (hallSize);

            // How much in the X axis does it move            
            for (
                int i = startPlacingTile_Pos_onX;
                i < stopPlacingTile_Pos_onX;
                i++
            )
            {
                // How in the Y axis does it move
                for(
                    
                    int j = startPlacingTile_Pos_onY; 
                    j <= stopPlacingTile_Pos_onY; 
                    j++
                )
                {
                    // Place the tile if one exists.
                    if (hallTile != null)
                    {
                        mainTilemap.SetTile(
                            new Vector3Int(i, j, 0),
                            hallTile
                        );
                    }   
                }
            }
        }

        // *******************************
        //              LEFT
        // *******************************
        if (directionFrom == Direction.Left)
        {

            // We need the Anchor Position in the MAIN GRID for ROOM A
            Vector2Int AnchorA_MainGridPos = RoomA.roomTemplate.anchorPosition + RoomA.mainGridPosition;

            // --- X AXIS ---
            // Where does it start placing Tiles on the X Axis?:
            int startPlacingTile_Pos_onX = RoomB.mainGridPosition.x + roomSize;

            // Where does it stop placing Tiles on the X Axis?:
            int stopPlacingTile_Pos_onX = RoomA.mainGridPosition.x;

            // --- Y AXIS ---
            // Where does it start placing Tiles on the Y Axis?:
            int startPlacingTile_Pos_onY = AnchorA_MainGridPos.y - (hallSize);

            // Where does it stop placing Tiles on the Y Axis?:
            int stopPlacingTile_Pos_onY = AnchorA_MainGridPos.y + (hallSize);

            // How much in the X axis does it move            
            for (
                int i = startPlacingTile_Pos_onX;
                i < stopPlacingTile_Pos_onX;
                i++
            )
            {
                // How in the Y axis does it move
                for(
                    
                    int j = startPlacingTile_Pos_onY; 
                    j <= stopPlacingTile_Pos_onY; 
                    j++
                )
                {
                    // Place the tile if one exists.
                    if (hallTile != null)
                    {
                        mainTilemap.SetTile(
                            new Vector3Int(i, j, 0),
                            hallTile
                        );
                    }   
                }
            }
        }

    }

}

