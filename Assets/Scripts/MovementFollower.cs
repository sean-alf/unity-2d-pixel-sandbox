using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovementFollower : MonoBehaviour
{
    [SerializeField]
    private PlayerMovement playerMovement;
    [SerializeField]
    [Tooltip("Should the movement be in the opposite direction?")]
    private bool opposite = false;
    [SerializeField]
    [Tooltip("The speed scale factor applied to the speed of the Game Object being followed")]
    private float speedScaleFactor = 1.0f;

    private Rigidbody2D rb;
    private PixelMovement2D movement2D;
    private PlayerMovement.PlayerMovementData data;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        movement2D = new(rb);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerMovement.OnDirectionChange += OnDirectionChange;
    }

    void FixedUpdate()
    {
        if (!data.IsPaused() && !data.currentDirection.IsIdle())
        {
            movement2D.Move((opposite ? -1 : 1) * data.currentDirection, data.blockedDirection, data.speed * speedScaleFactor);
        }
    }

    private void OnDirectionChange(PlayerMovement.PlayerMovementData data)
    {
        this.data = data;
    }
}
