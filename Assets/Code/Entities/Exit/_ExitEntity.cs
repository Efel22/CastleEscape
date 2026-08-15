using UnityEngine;
using UnityEngine.Tilemaps;

public class _ExitEntity : MonoBehaviour
{

    // **********************************
    //             REFERENCES 
    // **********************************
    private BoxCollider2D boxCollider;
    private Tilemap WallGroundTilemap;
    private Tilemap DecorTilemap;
    private _RoomGenerator roomGenScriptRef;

    void Start()
    {
        // Assign references since this gameObject is used as a PREFAB 
        boxCollider = GetComponent<BoxCollider2D>();   
        WallGroundTilemap = GameObject.FindGameObjectWithTag("WallsTilemap").GetComponent<Tilemap>();
        DecorTilemap = GameObject.FindGameObjectWithTag("DecoTilemap").GetComponent<Tilemap>();
        roomGenScriptRef = GameObject.FindGameObjectWithTag("MainGrid").GetComponent<_RoomGenerator>(); 
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player has entered the Exit!");

            // Clear all tiles so map resets
            WallGroundTilemap.ClearAllTiles();
            DecorTilemap.ClearAllTiles();

            // Store all gameObjects inside 'entities'
            GameObject[] entities = GameObject.FindGameObjectsWithTag("Entity");

            // Iterate through each object and destroy them
            foreach (GameObject entity in entities)
            {
                Destroy(entity);
            }

            // Start Dungeon generation
            roomGenScriptRef.GenerateDungeon();

            // Destroy the Exit Tile Entity to prevent duplication when the dungeon generates again
            Destroy(gameObject);
        }

        
        
    }
}
