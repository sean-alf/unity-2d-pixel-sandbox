using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
public class BlasterPellet : MonoBehaviour, BasicBlaster.IBlasterAmmo
{
    private static readonly ILogger logger = Debug.unityLogger;

    [SerializeField]
    [Tooltip("The number of pixels to offset from the center of the blaster sprite. Direction is handled internally.")]
    private int spawnPositionOffset = 10;

    private Animator animator;
    private Rigidbody2D rb;
    private PixelMovement2D movement;
    private Vector2 direction;
    private float speed;
    private Vector2 directionTmp;
    private float speedTmp;

    public bool InUse => gameObject.activeSelf;
    public int SpawnPositionOffset => spawnPositionOffset;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        movement = new(rb)
        {
            enableLogs = false
        };

        gameObject.SetActive(false);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // TODO: do stuff to determine what was hit
        // TODO: animate out
        logger.Log(name, "Collide");
        direction = new(0, 0);
        speed = 0;
        gameObject.SetActive(false);
    }

    public void Animator_InitEnd()
    {
        logger.Log(name, "Init End");
        direction = directionTmp;
        speed = speedTmp;
    }

    void FixedUpdate()
    {
        movement.Move(direction, speed);
    }

    public void Use(Vector2 position, Vector2 direction, float speed)
    {
        logger.Log(name, $"Use position {position} direction {direction} speed {speed}");
        directionTmp = direction;
        speedTmp = speed;

        transform.position = position;
        rb.position = position;
        gameObject.SetActive(true);
        animator.Play("BlasterPelletInit");
    }
}
