using UnityEngine;

public class _RoomGenerator : MonoBehaviour
{

    // ?: Rooms that can generate
    [SerializeField] private RoomData[] rooms;

    [SerializeField] private Grid mainGrid;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // First room always starts at MainGrid (0,0)
        GenerateRoomAt(Vector2Int.zero, rooms[0]);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    

    // ?: Generates specified room at Position
    public bool GenerateRoomAt(Vector2Int roomPosition, RoomData roomToPlace)
    {
        // Check if the room is valid
        if (roomToPlace == null) return false;

        // Get the position of the StartRoom tile (within its own grid which is SEPARATE FROM 'mainGrid')
        Vector3Int startPosition = roomToPlace.GetStartPosition();

        // Convert both positions to world space (worldSpace = ATTACHED to 'mainGrid')
        Vector3 targetWorldPosition =
            mainGrid.CellToWorld(
                new Vector3Int(roomPosition.x, roomPosition.y, 0)
            );

        Vector3 startWorldPosition = roomToPlace.tilemap.CellToWorld(startPosition);

        // Offset the room so its StartRoom matches the target position
        // (Makes it so rooms don't generate at (0,0) but rather where the function specifies)
        Vector3 worldPosition = targetWorldPosition - startWorldPosition;

        Instantiate(
            roomToPlace.gameObject,
            worldPosition,
            Quaternion.identity
        );

        return true;
    }

}
