using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(LinearAnimator))]
[RequireComponent(typeof(ProjectileManager))]
[RequireComponent(typeof(HealthManager))]
public class PlayerController : MonoBehaviour, AutoMover.IAutoMoverTarget, ILoggerProvider
{
    public bool IsInputReady => input != null;
    public bool IsInputActive
    {
        get
        {
            if (input) return input.inputIsActive;
            return false;
        }
    }

    [SerializeField]
    [Range(1, 10)]
    private int speed = 1;

    [SerializeField]
    private Transform projectileSpawnPoint;

    [Header("Debug")]
    [SerializeField]
    private Logger logger;

    [SerializeField]
    [Tooltip("Only exposed for debugging purposes. Not intended for modification via the inspector.")]
    private List<GameObject> others = new();

    public float Speed => actualSpeed;

    public Logger Logger => logger;

    private Rigidbody2D rb;
    private LinearAnimator animator;
    private PlayerInput input;
    private ProjectileManager projectileManager;
    private HealthManager healthManager;
    private Vector2 currentDirection;
    private Vector2 lastNonIdleDirection = Vector2.up;
    private float actualSpeed;

    void Awake()
    {
        actualSpeed = speed * Time.fixedDeltaTime;

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<LinearAnimator>();
        input = GetComponent<PlayerInput>();
        projectileManager = GetComponent<ProjectileManager>();
        healthManager = GetComponent<HealthManager>();

        InputSystem_Actions_Names.Player.Move(input).performed += OnMove;
        InputSystem_Actions_Names.Player.Move(input).canceled += OnMove;
        InputSystem_Actions_Names.Player.Interact(input).performed += OnInteract;
        InputSystem_Actions_Names.Player.Jump(input).performed += OnJump;
        InputSystem_Actions_Names.Player.Previous(input).performed += OnPrevious;
        InputSystem_Actions_Names.Player.Next(input).performed += OnNext;

        projectileManager.SetShootingLayer(LayerNames.Projectile);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!currentDirection.IsIdle())
        {
            Vector2 movementOffset = actualSpeed * currentDirection;
            rb.MovePosition(movementOffset + rb.position);
        }
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        currentDirection = context.ReadValue<Vector2>().normalized;
        OnDirectionChanged(currentDirection);
    }

    private void OnDirectionChanged(Vector2 direction)
    {
        if (direction.IsIdle())
        {
            animator.Stop();
        }
        else
        {
            // This is for determining which way the player is facing even when stopped
            lastNonIdleDirection = direction;
            rb.SetRotation(Quaternion.LookRotation(Vector3.forward, direction));
            animator.Animate();
        }

        logger.I($"Direction changed = {direction}");
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        foreach (var o in others)
        {
            if (o == null) continue;

            // Only handle one thing per interaction, otherwise it might be confusing
            // Hence the "break"s
            if (o.TryGetComponent(out HealthPickup h))
            {
                healthManager.Heal(h.HealAmount);
                Destroy(h.gameObject);
                break;
            }
            else if (o.TryGetComponent(out AccessPanel p))
            {
                p.Activate();
                break;
            }
        }
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        projectileManager.Shoot(new ProjectileManager.StartingPointWithDirection
        {
            direction = lastNonIdleDirection,
            position = projectileSpawnPoint.position,
        });
    }

    private void OnPrevious(InputAction.CallbackContext context)
    {
        projectileManager.SelectPrevious();
    }

    private void OnNext(InputAction.CallbackContext context)
    {
        projectileManager.SelectNext();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        rb.linearVelocity = Vector2.zero;

        if (other.gameObject.TryGetComponent(out CollisionData data))
        {
            if (data.type == CollisionData.Type.Damage)
            {
                logger.D($"Hit by {LayerMask.LayerToName(data.gameObject.layer)}");
                healthManager.DoDamage(data.strength);
                return;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Teleport t))
        {
            t.AnimateAndTeleportToNextScene(gameObject);
        }
        else if (!others.Contains(other.gameObject))
        {
            others.Add(other.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (others.Contains(other.gameObject))
        {
            others.Remove(other.gameObject);
        }
    }

    private void OnDestroy()
    {
        InputSystem_Actions_Names.Player.Move(input).performed -= OnMove;
        InputSystem_Actions_Names.Player.Move(input).canceled -= OnMove;
        InputSystem_Actions_Names.Player.Interact(input).performed -= OnInteract;
        InputSystem_Actions_Names.Player.Jump(input).performed -= OnJump;
        InputSystem_Actions_Names.Player.Previous(input).performed -= OnPrevious;
        InputSystem_Actions_Names.Player.Next(input).performed -= OnNext;
    }

    public void EnableInput()
    {
        if (input == null) return;
        input.ActivateInput();
    }

    public void DisableInput()
    {
        if (input == null) return;

        // Stop the movement
        currentDirection = Vector2.zero;

        input.DeactivateInput();
    }

    void AutoMover.IAutoMoverTarget.OnDirectionChanged(Vector2 direction)
    {
        OnDirectionChanged(direction);
    }
}
