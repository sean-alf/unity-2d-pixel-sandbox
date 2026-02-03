using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(HealthManager))]
[RequireComponent(typeof(LinearAnimator))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(ProjectileManager))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour,
    AutoMover.IAutoMoverTarget,
    ILoggerProvider,
    ITriggerer,
    Interactable.IInteractor,
    IMeleeWeaponWielder,
    BetterInputManager.IInputChangeRequestor
{
    public BetterInputManager.InputType CurrentInputType => inputType;
    public bool IsInputReady => input != null;
    public bool IsInputActive
    {
        get
        {
            if (input) return input.inputIsActive;
            return false;
        }
    }

    [SerializeField][Range(0, 100)] private int inputMapSwitchingPriority;
    [SerializeField][Range(1, 20)] private float speed = 1;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private LinearAnimator effectAnimator;
    [SerializeField] private Sword sword;
    [SerializeField] private Spear spear;
    [SerializeField] private bool useSword = false;

    [Space]
    [Header("Debug")]

    [SerializeField] private BetterInputManager.InputType inputType = BetterInputManager.InputType.Full;
    [SerializeField] private BetterInputManager.InputType storedInputType = BetterInputManager.InputType.Full;
    [SerializeField] private Vector2 currentDirection;
    [SerializeField] private float speedFactor = 1f;
    [SerializeField] private List<Interactable> interactables = new();
    [SerializeField] private Logger logger;

    public Action<Vector2> OnDirectionChange;

    public float Speed => speed;
    public Logger Logger => logger;

    // IInteractor
    public GameObject GameObject => gameObject;
    // IMeleeWeaponWielder
    public Transform Transform => transform;
    // IInteractor, IMeleeWeaponWielder
    public BetterInputManager InputManager => inputManager;

    // BetterInputManager.IInputChangeRequestor
    public int Priority => inputMapSwitchingPriority;
    // BetterInputManager.IInputChangeRequestor
    public string Name => $"{name} ({GetType().Name})";
    // BetterInputManager.IInputChangeRequestor
    public BetterInputManager.InputType InputType => BetterInputManager.InputType.Aiming;

    private BetterInputManager inputManager;
    private Rigidbody2D rb;
    private LinearAnimator animator;
    private PlayerInput input;
    private ProjectileManager projectileManager;
    private HealthManager healthManager;
    private ExternalForceReceiver efr;
    private InteractIndicator interactIndicator;
    private bool swingForward = true;

    void Awake()
    {
        inputManager = GetComponent<BetterInputManager>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<LinearAnimator>();
        input = GetComponent<PlayerInput>();
        projectileManager = GetComponent<ProjectileManager>();
        healthManager = GetComponent<HealthManager>();

        efr = GetComponentInChildren<ExternalForceReceiver>();

        projectileManager.SetShootingLayer(LayerNames.Projectile);
    }

    private void Start()
    {
        interactIndicator = FindFirstObjectByType<InteractIndicator>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var scaledSpeed = speedFactor * speed;
        rb.linearVelocity = efr.AppliedForce + scaledSpeed * currentDirection;
    }

    public void UnityEvent_OnMove(InputAction.CallbackContext context)
    {
        currentDirection = context.ReadValue<Vector2>().normalized;
        OnDirectionChange?.Invoke(currentDirection);
        OnDirectionChanged(currentDirection);
        HandleAnimations(currentDirection);
    }

    public void UnityEvent_OnAim(InputAction.CallbackContext context)
    {
        var direction = context.ReadValue<Vector2>().normalized;
        OnDirectionChange?.Invoke(direction);
        OnDirectionChanged(direction);
    }

    public void UnityEvent_OnAimEnable(InputAction.CallbackContext context) => EnableAim(enable: context.ReadValueAsButton());

    public void UnityEvent_OnInteract(InputAction.CallbackContext context)
    {
        foreach (var i in interactables)
        {
            if (i == null) continue;

            // Only handle one thing per interaction, otherwise it might be confusing
            // Hence the "break" below
            i.Interact(this);
            break;
        }
    }

    public void UnityEvent_OnAttack(InputAction.CallbackContext context)
    {
        if (useSword)
        {
            sword.StartSwing(this, swingForward);
            swingForward = !swingForward;
        }
        else
        {
            spear.StartJab(this);
        }
    }

    public void UnityEvent_OnShoot(InputAction.CallbackContext context)
    {
        projectileManager.Shoot(new ProjectileManager.StartingPointWithDirection
        {
            direction = transform.rotation * Vector2.up,
            position = projectileSpawnPoint.position,
        });
    }

    public void UnityEvent_OnPrevious(InputAction.CallbackContext context)
    {
        projectileManager.SelectPrevious();
    }

    public void UnityEvent_OnNext(InputAction.CallbackContext context)
    {
        projectileManager.SelectNext();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        rb.linearVelocity = Vector2.zero;
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
        else if (other.gameObject.TryGetComponent(out CollisionData data))
        {
            if (data.Type == CollisionData.CollisionType.Damage)
            {
                logger.D($"Hit by {LayerMask.LayerToName(data.gameObject.layer)}");
                healthManager.DoDamage(data.Strength);
                return;
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

    private void EnableAim(bool enable)
    {
        if (enable)
        {
            inputManager.AddInputChangeRequest(this);
        }
        else
        {
            inputManager.RemoveInputChangeRequest(this);
        }
    }

    public void UnityEvent_OnInputTypeChanged(BetterInputManager.InputType type)
    {
        switch (type)
        {
            case BetterInputManager.InputType.None:
                rb.linearVelocity = Vector2.zero;
                currentDirection = Vector2.zero;
                break;
            case BetterInputManager.InputType.Aiming:
                rb.linearVelocity = Vector2.zero;
                currentDirection = Vector2.zero;
                var direction = inputManager.MoveAction.ReadValue<Vector2>();
                OnDirectionChange?.Invoke(direction);
                OnDirectionChanged(direction);
                break;
            case BetterInputManager.InputType.Full:
                currentDirection = inputManager.MoveAction.ReadValue<Vector2>();
                OnDirectionChange?.Invoke(currentDirection);
                OnDirectionChanged(currentDirection);
                EnableAim(inputManager.AimAction.IsPressed());
                break;
        }

        HandleAnimations(currentDirection);
    }

    public void RestoreInput(GameObject from) => UpdateInputType(storedInputType);

    public void DisableInput(GameObject from)
    {
        if (inputType == BetterInputManager.InputType.None) return;
        UpdateInputType(BetterInputManager.InputType.None);
    }

    public void UpdateInputType(BetterInputManager.InputType type, bool shouldAnimate = true)
    {
        if (input == null || inputType == type) return;

        storedInputType = inputType;
        inputType = type;

        switch (inputType)
        {
            case BetterInputManager.InputType.Full:
                input.ActivateInput();
                break;

            case BetterInputManager.InputType.Aiming:
                rb.linearVelocity = Vector2.zero;
                input.ActivateInput();
                break;
            case BetterInputManager.InputType.None:
                // Stop the movement
                rb.linearVelocity = Vector2.zero;
                currentDirection = Vector2.zero;
                input.DeactivateInput();
                break;

        }

        if (shouldAnimate && !currentDirection.IsIdle())
        {
            animator.Animate("Default");
            effectAnimator.Animate();
        }
        else
        {
            animator.Stop();
            effectAnimator.Stop();
        }
    }

    public void TileTracker_OnTileChanged(Vector3Int cell)
    {
        logger.D($"new tile cell {cell}");
    }

    public void TileTracker_OnTileTypeChanged(Vector3Int cell, TileTracker.TileType type)
    {
        logger.D($"new tile cell {cell} type {type}");

        switch (type)
        {
            case TileTracker.TileType.None:
                speedFactor = 1f;
                effectAnimator.Stop(clearSprite: true);
                effectAnimator.ClearCurrentAnimation();
                break;
            case TileTracker.TileType.DeepMud:
                speedFactor = 0.5f;
                effectAnimator.Animate("Mud");
                break;
        }
    }

    void AutoMover.IAutoMoverTarget.OnDirectionChanged(Vector2 direction)
    {
        OnDirectionChanged(direction);
        HandleAnimations(direction);
    }

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

    private void OnDirectionChanged(Vector2 direction)
    {
        if (!direction.IsIdle())
        {
            var lookRotation = Quaternion.LookRotation(Vector3.forward, direction);

            if (transform.parent != null)
            {
                transform.localRotation = lookRotation;
            }
            else
            {
                rb.SetRotation(lookRotation);
            }
        }

        logger.I($"Direction changed = {direction}");
    }

    private void HandleAnimations(Vector2 direction)
    {
        if (direction.IsIdle())
        {
            animator.Stop();
            effectAnimator.Stop();
        }
        else
        {
            animator.Animate("Default");
            effectAnimator.Animate();
        }
    }
}
