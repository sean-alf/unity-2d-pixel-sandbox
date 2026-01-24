using System;
using System.Collections.Generic;
using System.Linq;
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

    [SerializeField][Range(1, 20)] private int speed = 1;
    [SerializeField] private Transform projectileSpawnPoint;

    [Space]
    [Header("Debug")]

    [SerializeField] private Logger logger;
    [SerializeField] private List<Interactable> interactables = new();

    public Action<Vector2> OnDirectionChange;
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
        if (inputType != InputType.Full) return;

        rb.linearVelocity = efr.AppliedForce + speed * currentDirection;
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        var direction = context.ReadValue<Vector2>().normalized;
        currentDirection = direction;
        OnDirectionChange?.Invoke(direction);
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
            if (!interactables.Contains(i))
            {
                interactables.Add(i);
                i.onInteractableStateChange += OnInteractableStateChange;
            }
        }

        if (interactIndicator)
        {
            // If at least one Interactable that is currently interactable
            // Then show the interact indicator
            var found = interactables.Find(i => !i.TryGetComponent(out Interactable.IOverride o) || o.IsInteractable);
            if (found) interactIndicator.Show();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out Interactable i))
        {
            if (interactables.Contains(i))
            {
                interactables.Remove(i);
                i.onInteractableStateChange += OnInteractableStateChange;
            }
        }

        if (interactIndicator)
        {
            // If NOT at least one Interactable that is currently interactable
            // Then hide the interact indicator
            var found = interactables.Find(i => !i.TryGetComponent(out Interactable.IOverride o) || o.IsInteractable);
            if (!found) interactIndicator.Hide();
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

    public void UnityEvent_EnableInput() => UpdateInputType(InputType.Full);

    public void UnityEvent_DisableInput() => UpdateInputType(InputType.None);

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
                rb.linearVelocity = Vector2.zero;
                input.ActivateInput();
                break;

            case InputType.AutoMoving:
            case InputType.None:
                // Stop the movement
                rb.linearVelocity = Vector2.zero;
                currentDirection = Vector2.zero;
                input.DeactivateInput();
                break;

        }

        if (ShouldAnimate && !currentDirection.IsIdle())
        {
            animator.Animate("Default");
        }
        else
        {
            animator.Stop();
        }
    }

    void AutoMover.IAutoMoverTarget.OnDirectionChanged(Vector2 direction) => OnDirectionChanged(direction);

    private void OnInteractableStateChange(Interactable i)
    {
        if (!interactables.Contains(i)) return;

        var found = interactables.Find(i => !i.TryGetComponent(out Interactable.IOverride o) || o.IsInteractable);

        if (found)
        {
            interactIndicator.Show();
        }
        else
        {
            interactIndicator.Hide();
        }
    }
}
