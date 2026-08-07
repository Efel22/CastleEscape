using UnityEngine;

public class PlayerController : MonoBehaviour
{

    // ****************************************
    // Player Attributes (Speed, Health, etc.)
    // ****************************************
    public float movementSpeed;

    // ****************************************

    Rigidbody2D rb;
    private PlayerControls controls;
    private Vector2 movement;

    // ****************************************
    //                LIFE CYCLE
    // ****************************************
    //
    //                  Awake()
    //                     ↓
    //                 OnEnable()
    //                     ↓
    //                  Start()
    //                     ↓
    //                  Update()
    //                     ↓
    //                FixedUpdate()
    //
    // ****************************************

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        // Assign References (RigidBody is the "player" itself, its collision box, kinda)
        rb =  GetComponent<Rigidbody2D>();
        controls = new PlayerControls();
    }

    // Optimization (controls should only be enabled when needed)
    void OnEnable() { controls.Enable(); }
    void OnDisable() { controls.Disable(); }

    // Update is called once per frame
    void Update()
    {
        // Get the direction/s to which the player will move to!
        // *'normalized' prevents faster diagonal movement
        movement = controls.Player.Move.ReadValue<Vector2>().normalized;
    }

    // Fixed Update is used for Physics and movement 
    // (better accuracy than 'Update()')
    void FixedUpdate()
    {
        // Move the player
        rb.linearVelocity = movement * movementSpeed;
    }
}
