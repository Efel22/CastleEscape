using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// This allows for separation of the Unity Instance and Prefabs
// ?: If I were to edit any value of the mainGridPosition (Ex: RoomData[1]) instances then
//    they'd change for ALL of those mainGridPosition[1] instances instead!
// ?: Allows for per-room mainGridPosition editing 
public class GeneratedRoom
{
    
    public RoomData roomTemplate;

    // This value is set when the room is generated 
    public Vector2Int mainGridPosition;

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
        GenerateRoomAt(new Vector2Int(0,0), GetRandomRoom());

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

            // CHECK FOR UP:
            PlaceRoomAt_Pos_Temp = 
                generatedRooms[roomsGeneratedCounter - 1].mainGridPosition + GetDirectionOffset("up");

            if (!IsPositionOccupied(PlaceRoomAt_Pos_Temp)) availablePositions.Add(PlaceRoomAt_Pos_Temp);
            
            // CHECK FOR DOWN:
            PlaceRoomAt_Pos_Temp = 
                generatedRooms[roomsGeneratedCounter - 1].mainGridPosition + GetDirectionOffset("down");

            if (!IsPositionOccupied(PlaceRoomAt_Pos_Temp)) availablePositions.Add(PlaceRoomAt_Pos_Temp);
            
            // CHECK FOR LEFT:
            PlaceRoomAt_Pos_Temp = 
                generatedRooms[roomsGeneratedCounter - 1].mainGridPosition + GetDirectionOffset("left");

            if (!IsPositionOccupied(PlaceRoomAt_Pos_Temp)) availablePositions.Add(PlaceRoomAt_Pos_Temp);
            
            // CHECK FOR RIGHT:
            PlaceRoomAt_Pos_Temp = 
                generatedRooms[roomsGeneratedCounter - 1].mainGridPosition + GetDirectionOffset("right");

            if (!IsPositionOccupied(PlaceRoomAt_Pos_Temp)) availablePositions.Add(PlaceRoomAt_Pos_Temp);

            // If no available positions... get out of this loop
            if (availablePositions.Count <= 0 ) break;

            // Generate a random room at the new position with the offsets 
            GenerateRoomAt(availablePositions[Random.Range(0, availablePositions.Count)], GetRandomRoom());

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

    public void GenerateRoomAt(Vector2Int placeRoomAt, RoomData roomToPlace)
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

        GeneratedRoom roomToRegister = new GeneratedRoom();
        roomToRegister.roomTemplate = roomToPlace;
        roomToRegister.mainGridPosition = placeRoomAt;
        generatedRooms.Add(roomToRegister);
    }

    

}
