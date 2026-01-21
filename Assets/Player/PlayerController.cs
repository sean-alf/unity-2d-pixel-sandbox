using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(HealthManager))]
[RequireComponent(typeof(LinearAnimator))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(ProjectileManager))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour, AutoMover.IAutoMoverTarget, ILoggerProvider, ITriggerer
{
    public enum InputType
    {
        Full,
        Riding,
        AutoMoving,
        None,
    }

    public InputType CurrentInputType => inputType;
    public bool IsInputReady => input != null;
    public bool IsInputActive
    {
        get
        {
            if (input) return input.inputIsActive;
            return false;
        }
    }

    private bool ShouldAnimate => inputType == InputType.Full || inputType == InputType.AutoMoving;

    [SerializeField]
    [Range(1, 20)]
    private int speed = 1;

    [SerializeField]
    private Transform projectileSpawnPoint;

    [Space]
    [Header("Debug")]

    [SerializeField] private Logger logger;
    [SerializeField] private List<Interactable> interactables = new();

    public float Speed => speed;

    public Logger Logger => logger;

    private Rigidbody2D rb;
    private LinearAnimator animator;
    private PlayerInput input;
    private ProjectileManager projectileManager;
    private HealthManager healthManager;
    private ExternalForceReceiver efr;
    private InteractIndicator interactIndicator;
    private Vector2 currentDirection;
    private InputType inputType;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<LinearAnimator>();
        input = GetComponent<PlayerInput>();
        projectileManager = GetComponent<ProjectileManager>();
        healthManager = GetComponent<HealthManager>();

        efr = GetComponentInChildren<ExternalForceReceiver>();

        InputSystem_Actions_Names.Player.Move(input).performed += OnMove;
        InputSystem_Actions_Names.Player.Move(input).canceled += OnMove;
        InputSystem_Actions_Names.Player.Interact(input).performed += OnInteract;
        InputSystem_Actions_Names.Player.Jump(input).performed += OnJump;
        InputSystem_Actions_Names.Player.Previous(input).performed += OnPrevious;
        InputSystem_Actions_Names.Player.Next(input).performed += OnNext;

        projectileManager.SetShootingLayer(LayerNames.Projectile);
    }

    private void Start()
    {
        interactIndicator = FindFirstObjectByType<InteractIndicator>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.linearVelocity = efr.AppliedForce + speed * currentDirection;
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        var direction = context.ReadValue<Vector2>().normalized;
        if (inputType == InputType.Full) currentDirection = direction;
        OnDirectionChanged(direction);
    }

    private void OnDirectionChanged(Vector2 direction)
    {
        if (direction.IsIdle())
        {
            animator.Stop();
        }
        else
        {
            var lookRotation = Quaternion.LookRotation(Vector3.forward, direction);

            if (inputType == InputType.Riding)
            {
                transform.localRotation = lookRotation;
            }
            else
            {
                rb.SetRotation(lookRotation);
            }

            if (ShouldAnimate) animator.Animate("Default");
        }

        logger.I($"Direction changed = {direction}");
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        foreach (var i in interactables)
        {
            if (i == null) continue;

            // Only handle one thing per interaction, otherwise it might be confusing
            // Hence the "break"s
            i.Interact(gameObject);
            break;
        }
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        projectileManager.Shoot(new ProjectileManager.StartingPointWithDirection
        {
            direction = transform.rotation * Vector2.up,
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
            if (data.Type == CollisionData.CollisionType.Damage)
            {
                logger.D($"Hit by {LayerMask.LayerToName(data.gameObject.layer)}");
                healthManager.DoDamage(data.Strength);
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
        else if (other.TryGetComponent(out Interactable i))
        {
            if (!interactables.Contains(i)) interactables.Add(i);
        }

        if (interactables.Count > 0) interactIndicator.Show();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out Interactable i))
        {
            if (interactables.Contains(i))
            {
                interactables.Remove(i);
            }
        }

        if (interactables.Count == 0) interactIndicator.Hide();
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

    public void UpdateInputType(InputType type)
    {
        inputType = type;

        if (input == null) return;

        switch (inputType)
        {
            case InputType.Full:
                input.ActivateInput();
                break;

            case InputType.Riding:
                // Just make sure that input is enabled
                // All we do with this type is stop movement
                // But still allow rotation, shooting, etc.
                currentDirection = Vector2.zero;
                input.ActivateInput();
                break;

            case InputType.AutoMoving:
            case InputType.None:
                // Stop the movement
                currentDirection = Vector2.zero;
                input.DeactivateInput();
                break;

        }
    }

    void AutoMover.IAutoMoverTarget.OnDirectionChanged(Vector2 direction) => OnDirectionChanged(direction);
}
