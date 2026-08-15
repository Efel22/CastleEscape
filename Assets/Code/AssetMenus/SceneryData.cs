using UnityEngine;

[CreateAssetMenu(fileName = "SceneryData", menuName = "Game/Scenery Data")]
public class SceneryData : ScriptableObject
{

    [Header("Rooms")] 
    public string name;

    [Header("Rooms")]    
    public RoomData[] rooms;
    public SpawnRoomData[] spawnRooms;
    public EndRoomData[] endRooms;

}
