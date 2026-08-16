using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Tilemaps;
public enum Scenery
{
    Dungeon,
    Garden,
    RoyalHalls,
}

[CreateAssetMenu(fileName = "SceneryData", menuName = "Game/Scenery Data")]
public class SceneryData : ScriptableObject
{

    [Header("Scenery")] 
    public Scenery sceneryType;

    public Color backgroundColor;

    [Header("Rooms")]    
    public RoomData[] rooms;
    public SpawnRoomData[] spawnRooms;
    public EndRoomData[] endRooms;

    [Header("Scenery")] 
    public TileBase hallTile; 

}
