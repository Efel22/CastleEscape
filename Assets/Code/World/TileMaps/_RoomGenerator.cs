using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;



public class _RoomGenerator : MonoBehaviour
{
    // ************************
    // Room Settings
    // ************************

    [SerializeField] private int roomSize = 15;

    // How many rooms to generate.
    [SerializeField] private int roomCount = 10;

    // Chance for a branch to be created.
    [Range(0f, 1f)]
    [SerializeField] private float branchChance = 0.25f;

    // Gap between rooms.
    [SerializeField] private int tileGapBetweenRooms = 1;

    // Tile used to create halls.
    [SerializeField] private TileBase hallTile;


    // ************************
    // References
    // ************************

    [SerializeField] private RoomData[] rooms;

    [SerializeField] private Grid mainGrid;

    [SerializeField] private Tilemap mainTilemap;

    [SerializeField] private Transform player;


    // ************************
    // Generated Room Positions
    // ************************

    // Keeps track of where rooms already exist.
    private List<Vector2Int> generatedPositions = new List<Vector2Int>();
    private List<RoomData> generatedRooms = new List<RoomData>();


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
        // First room always starts at (0,0).
        Vector2Int startPosition = Vector2Int.zero;
        RoomData firstRoom = rooms[0];

        GenerateRoomAt(startPosition, firstRoom);

        generatedPositions.Add(startPosition);
        generatedRooms.Add(firstRoom);


        // ************************
        // Player Spawn
        // ************************

        Vector2Int anchorPosition =
            startPosition + firstRoom.anchorPosition;

        Vector3 playerWorldPosition =
            mainGrid.GetCellCenterWorld(
                new Vector3Int(
                    anchorPosition.x,
                    anchorPosition.y,
                    0
                )
            );

        player.position = playerWorldPosition;


        // ************************
        // Generate Remaining Rooms
        // ************************

        while (generatedPositions.Count < roomCount)
        {
            // Pick a random existing room.
            int randomIndex =
                Random.Range(0, generatedPositions.Count);

            Vector2Int currentPosition =
                generatedPositions[randomIndex];

            RoomData currentRoom =
                generatedRooms[randomIndex];


            // Pick a random direction.
            Vector2Int direction =
                GetRandomDirection();


            // Calculate new room position.
            Vector2Int newPosition =
                currentPosition +
                direction * (roomSize + tileGapBetweenRooms);


            // Don't generate on an existing room.
            if (generatedPositions.Contains(newPosition))
                continue;


            // Pick a random room.
            RoomData newRoom =
                rooms[Random.Range(0, rooms.Length)];


            // Generate the room.
            GenerateRoomAt(newPosition, newRoom);


            // Generate the hall.
            GenerateHall(
                currentPosition,
                currentRoom,
                newPosition,
                newRoom,
                direction
            );


            // Remember the room.
            generatedPositions.Add(newPosition);
            generatedRooms.Add(newRoom);
        }
    }


    // ************************
    // Random Direction
    // ************************

    Vector2Int GetRandomDirection()
    {
        int direction = Random.Range(0, 4);

        switch (direction)
        {
            case 0:
                return Vector2Int.up;

            case 1:
                return Vector2Int.down;

            case 2:
                return Vector2Int.left;

            default:
                return Vector2Int.right;
        }
    }


    // ************************
    // Generate Room
    // ************************

    public bool GenerateRoomAt(Vector2Int roomPosition, RoomData roomToPlace)
    {
        // Make sure the room exists.
        if (roomToPlace == null)
            return false;


        // Copy the room's 15x15 tiles.
        for (int x = 0; x < roomSize; x++)
        {
            for (int y = 0; y < roomSize; y++)
            {
                // Position of the tile inside the room.
                Vector3Int sourcePosition =
                    new Vector3Int(x, y, 0);


                // Position of the tile in the MainGrid.
                Vector3Int destinationPosition =
                    new Vector3Int(
                        roomPosition.x + x,
                        roomPosition.y + y,
                        0
                    );


                // Get the tile from the room.
                TileBase tile =
                    roomToPlace.tilemap.GetTile(sourcePosition);


                // Copy the tile if one exists.
                if (tile != null)
                {
                    mainTilemap.SetTile(
                        destinationPosition,
                        tile
                    );
                }
            }
        }

        return true;
    }

    // ************************
    // Generate Hall
    // ************************

    // Creates a 3-tile-wide hall between two rooms.
    void GenerateHall(
        Vector2Int roomAPosition,
        RoomData roomA,
        Vector2Int roomBPosition,
        RoomData roomB,
        Vector2Int direction)
    {
        // ************************
        // Horizontal Hall
        // ************************

        if (direction == Vector2Int.right ||
            direction == Vector2Int.left)
        {
            // Use the anchor's Y position.
            int y;

            if (direction == Vector2Int.right)
            {
                // Room A connects from its right side.
                y = roomAPosition.y + roomA.anchorPosition.y;
            }
            else
            {
                // Room B connects from its right side.
                y = roomBPosition.y + roomB.anchorPosition.y;
            }


            // Find the X positions between the rooms.
            int startX =
                Mathf.Min(
                    roomAPosition.x,
                    roomBPosition.x
                ) + roomSize;

            int endX =
                Mathf.Max(
                    roomAPosition.x,
                    roomBPosition.x
                );


            // Make the hall 3 tiles wide.
            for (int x = startX; x < endX; x++)
            {
                for (int offset = -1; offset <= 1; offset++)
                {
                    mainTilemap.SetTile(
                        new Vector3Int(
                            x,
                            y + offset,
                            0
                        ),
                        hallTile
                    );
                }
            }
        }


        // ************************
        // Vertical Hall
        // ************************

        else
        {
            // Use the anchor's X position.
            int x;

            if (direction == Vector2Int.up)
            {
                // Room A connects from its top side.
                x = roomAPosition.x + roomA.anchorPosition.x;
            }
            else
            {
                // Room B connects from its top side.
                x = roomBPosition.x + roomB.anchorPosition.x;
            }


            // Find the Y positions between the rooms.
            int startY =
                Mathf.Min(
                    roomAPosition.y,
                    roomBPosition.y
                ) + roomSize;

            int endY =
                Mathf.Max(
                    roomAPosition.y,
                    roomBPosition.y
                );


            // Make the hall 3 tiles wide.
            for (int y = startY; y < endY; y++)
            {
                for (int offset = -1; offset <= 1; offset++)
                {
                    mainTilemap.SetTile(
                        new Vector3Int(
                            x + offset,
                            y,
                            0
                        ),
                        hallTile
                    );
                }
            }
        }
    }

}
