using System.Collections.Generic;
using System.Runtime.ExceptionServices;
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
    [SerializeField] private int roomCount = 10;

    // Gap between rooms.
    [SerializeField] private int tileGapBetweenRooms = 1;

    // Tile used to create halls.
    [SerializeField] private TileBase hallTile;
    
    // Hall Width and Height added to the Halls
    [SerializeField, Min(1)] private int hallSize; 


    // ************************
    // Tracking Lists 
    // ************************

    private List<GeneratedRoom> generatedRooms = new List<GeneratedRoom>(); 

    // ************************
    // References
    // ************************

    [SerializeField] private RoomData[] rooms;

    [SerializeField] private Grid mainGrid;

    [SerializeField] private Tilemap mainTilemap;

    [SerializeField] private Transform player;

    
    // ************************
    // Start
    // ************************

    void Start()
    {
        GenerateDungeon();
    }


    // ************************
    // Dungeon Generation
    // ************************
    void GenerateDungeon()
    {

        // Generate the Initial Room at 0,0
        GenerateRoomAt(new Vector2Int(0,0), Direction.Down, GetRandomRoom());

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
                generatedRooms[roomsGeneratedCounter - 1].mainGridPosition + GetDirectionOffset("up");

            if (!IsPositionOccupied(PlaceRoomAt_Pos_Temp))
            {
                availablePositions.Add(PlaceRoomAt_Pos_Temp);
                availableDirections.Add(Direction.Up);
            }
            
            // CHECK FOR DOWN:
            PlaceRoomAt_Pos_Temp = 
                generatedRooms[roomsGeneratedCounter - 1].mainGridPosition + GetDirectionOffset("down");

            if (!IsPositionOccupied(PlaceRoomAt_Pos_Temp))
            {
                availablePositions.Add(PlaceRoomAt_Pos_Temp);
                availableDirections.Add(Direction.Down);
            }
            
            // CHECK FOR LEFT:
            PlaceRoomAt_Pos_Temp = 
                generatedRooms[roomsGeneratedCounter - 1].mainGridPosition + GetDirectionOffset("left");

            if (!IsPositionOccupied(PlaceRoomAt_Pos_Temp)) 
            {
                availablePositions.Add(PlaceRoomAt_Pos_Temp);
                availableDirections.Add(Direction.Left);
            }
            
            // CHECK FOR RIGHT:
            PlaceRoomAt_Pos_Temp = 
                generatedRooms[roomsGeneratedCounter - 1].mainGridPosition + GetDirectionOffset("right");

            if (!IsPositionOccupied(PlaceRoomAt_Pos_Temp))
            {
                availablePositions.Add(PlaceRoomAt_Pos_Temp);
                availableDirections.Add(Direction.Right);
            }

            // If no available positions... get out of this loop
            if (availablePositions.Count <= 0 ) break;

            int chosenIndex = Random.Range(0, availablePositions.Count);

            // Generate a random room at the new position with the offsets 
            GenerateRoomAt(
                availablePositions[chosenIndex],
                availableDirections[chosenIndex], 
                GetRandomRoom());

        }

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
    RoomData GetRandomRoom() { return rooms[Random.Range(0, rooms.Length)]; }

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
    Vector2Int GetDirectionOffset(string direction)
    {
        direction = direction.ToLowerInvariant();

        switch (direction)
        {
            // Up    (0, +Y)
            case "up":
                return new Vector2Int(0, roomSize + tileGapBetweenRooms);

            // Down  (0, -Y)
            case "down":
                return new Vector2Int(0, -(roomSize + tileGapBetweenRooms));

            // Left  (-X, 0)
            case "left":
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
                int i = AnchorA_MainGridPos.x - (hallSize + 1); 
                i <= AnchorA_MainGridPos.x + (hallSize + 1); 
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

            // *NOTE: This is the OPPOSITE from UP() ^, start and stop tile positions are inverted here
            // Where does it start placing Tiles on the Y Axis?:
            int startPlacingTile_Pos = RoomB_MainGridPos.y; 

            // Where does it stop placing Tiles on the Y Axis?:
            int stopPlacingTile_Pos = RoomA_MainGridPos.y + roomSize;

            // How much in the X axis does it move            
            for (
                int i = AnchorA_MainGridPos.x - (hallSize + 1); 
                i <= AnchorA_MainGridPos.x + (hallSize + 1); 
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
            // Store the Anchor Points positions
            Vector2Int AnchorA_Pos = RoomA.roomTemplate.anchorPosition;

            // Store the Rooms Positions in the MAIN GRID
            Vector2Int RoomA_MainGridPos = RoomA.mainGridPosition;
            Vector2Int RoomB_MainGridPos = RoomB.mainGridPosition;

            // Now, we need the Anchor Position in the MAIN GRID for ROOM A
            Vector2Int AnchorA_MainGridPos = AnchorA_Pos + RoomA_MainGridPos;

            // Where does it start placing Tiles on the X Axis?:
            int startPlacingTile_Pos = RoomA_MainGridPos.x + roomSize;

            // Where does it stop placing Tiles on the Y Axis?:
            int stopPlacingTile_Pos = RoomB_MainGridPos.x;

            // How much in the X axis does it move            
            for (
                int i = startPlacingTile_Pos;
                i < stopPlacingTile_Pos;
                i++
            )
            {
                // How in the Y axis does it move
                for(
                    
                    int j = AnchorA_MainGridPos.y - (hallSize + 1); 
                    j <= AnchorA_MainGridPos.y + (hallSize + 1); 
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
        //              LEFT
        // *******************************
        if (directionFrom == Direction.Left)
        {
            // Store the Anchor Points positions
            Vector2Int AnchorA_Pos = RoomA.roomTemplate.anchorPosition;

            // Store the Rooms Positions in the MAIN GRID
            Vector2Int RoomA_MainGridPos = RoomA.mainGridPosition;
            Vector2Int RoomB_MainGridPos = RoomB.mainGridPosition;

            // Now, we need the Anchor Position in the MAIN GRID for ROOM A
            Vector2Int AnchorA_MainGridPos = AnchorA_Pos + RoomA_MainGridPos;

            // Where does it start placing Tiles on the X Axis?:
            int startPlacingTile_Pos = RoomB_MainGridPos.x;

            // Where does it stop placing Tiles on the Y Axis?:
            int stopPlacingTile_Pos = RoomA_MainGridPos.x + roomSize;

            // How much in the X axis does it move            
            for (
                int i = startPlacingTile_Pos;
                i < stopPlacingTile_Pos;
                i++
            )
            {
                // How in the Y axis does it move
                for(
                    
                    int j = AnchorA_MainGridPos.y - (hallSize + 1); 
                    j <= AnchorA_MainGridPos.y + (hallSize + 1); 
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

    }

    string DetermineDirection(Vector2Int directionFrom)
    {
        if (directionFrom.y > 0)
        {
            return "up";
        }
        else if (directionFrom.y < 0)
        {
            return "down";
        }
        else if (directionFrom.x < 0)
        {
            return "left";
        }
        else // (directionFrom.y > 0)
        {
            return "right";
        }
    }

}

