using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(HealthManager))]
[RequireComponent(typeof(LinearAnimator))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(ProjectileManager))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BetterInputManager))]
[RequireComponent(typeof(ActionableItemGroupsManager))]
[RequireComponent(typeof(MeleeWeaponManager))]
public class PlayerController : MonoBehaviour,
    AutoMover.IAutoMoverTarget,
    ILoggerProvider,
    ITriggerer,
    Interactable.IInteractor,
    IMeleeWeaponWielder,
    BetterInputManager.IInputChangeRequestor
{
    // ──────────────────────────────────────────────────────────────
    // Serialized Fields
    // ──────────────────────────────────────────────────────────────

    [SerializeField][Range(0, 100)] private int inputMapSwitchingPriority;
    [SerializeField][Range(1, 20)] private float speed = 1;
    [SerializeField][Range(1, 20)] private float recoilDuration = 0.25f;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private LinearAnimator effectAnimator;
    [SerializeField] private BoolChangeEvent interactIndicatorVisibilityEvent;

    [Space]
    [Header("Debug")]

    [SerializeField] private Vector2 currentDirection;
    [SerializeField] private float speedFactor = 1f;
    [SerializeField] private List<Interactable> interactables = new();
    [SerializeField] private Readable readable;
    [SerializeField] private ItemCache itemCache;
    [SerializeField] private Logger logger;

    // ──────────────────────────────────────────────────────────────
    // Public UnityEvents
    // ──────────────────────────────────────────────────────────────

    public UnityEvent<Vector2> onDirectionChange;

    // ──────────────────────────────────────────────────────────────
    // Public Properties
    // ──────────────────────────────────────────────────────────────

    public float Speed => speed;
    public Logger Logger => logger;

    // ──────────────────────────────────────────────────────────────
    // Interface Implementation Properties
    // ──────────────────────────────────────────────────────────────

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
    public BetterInputManager.InputType InputType => inputType;

    // ──────────────────────────────────────────────────────────────
    // Private Variables
    // ──────────────────────────────────────────────────────────────

    private BetterInputManager inputManager;
    private Rigidbody2D rb;
    private LinearAnimator animator;
    private ActionableItemGroupsManager itemGroupsManager;
    private MeleeWeaponManager meleeWeaponManager;
    private ProjectileManager projectileManager;
    private HealthManager healthManager;
    private GameObject head;
    private GameObject eye;
    private ExternalForceReceiver efr;
    private Vector3 headInitialLocalPosition;
    private Vector3 headRecoilLocalPosition;
    private float recoilTimerCounter;
    private bool shouldHandleRecoil = false;
    private BetterInputManager.InputType inputType = BetterInputManager.InputType.Aiming;

    // ──────────────────────────────────────────────────────────────
    // GameObject Lifecycle
    // ──────────────────────────────────────────────────────────────

    void Awake()
    {
        inputManager = GetComponent<BetterInputManager>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<LinearAnimator>();
        itemGroupsManager = GetComponent<ActionableItemGroupsManager>();
        meleeWeaponManager = GetComponent<MeleeWeaponManager>();
        projectileManager = GetComponent<ProjectileManager>();
        healthManager = GetComponent<HealthManager>();

        head = transform.Find("Head").gameObject;
        eye = head.transform.Find("Eye").gameObject;
        efr = GetComponentInChildren<ExternalForceReceiver>();

        projectileManager.SetShootingLayer(LayerNames.Projectile);

        headInitialLocalPosition = head.transform.localPosition;
        headRecoilLocalPosition = head.transform.localPosition.SubtractY(4f / 32f);
    }

    private void OnEnable()
    {
        projectileManager.onProjectileChanged.AddListener(ProjectileManager_OnProjectilChanged);
        projectileManager.onProjectileInstantiated.AddListener(ProjectileManager_OnShoot);
    }

    private void OnDisable()
    {
        projectileManager.onProjectileChanged.RemoveListener(ProjectileManager_OnProjectilChanged);
        projectileManager.onProjectileInstantiated.RemoveListener(ProjectileManager_OnShoot);
    }

    private void Update()
    {
        HandleRecoil();
    }

    private void FixedUpdate()
    {
        var scaledSpeed = speedFactor * speed;
        rb.linearVelocity = efr.AppliedForce + scaledSpeed * currentDirection;
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
        else if (other.TryGetComponent(out Readable r))
        {
            readable = r;
        }
        else if (other.gameObject.TryGetComponent(out ItemCache ic))
        {
            itemCache = ic;
        }

        UpdateInteractIndicatorVisibility();
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
        else if (other.TryGetComponent(out Readable _))
        {
            readable = null;
        }
        else if (other.TryGetComponent(out ItemCache _))
        {
            itemCache = null;
        }

        UpdateInteractIndicatorVisibility();
    }

    // ──────────────────────────────────────────────────────────────
    // Public Methods
    // ──────────────────────────────────────────────────────────────

    // Only use this when the player's direction needs to be changed immediately
    // Never use this when the player should have control of movement/direction.
    public void UpdateDirection(Vector2 direction)
    {
        OnDirectionChanged(direction);
    }

    // ──────────────────────────────────────────────────────────────
    // Input System Unity Event Methods
    // ──────────────────────────────────────────────────────────────

    public void UnityEvent_OnMove(InputAction.CallbackContext context)
    {
        currentDirection = context.ReadValue<Vector2>().normalized;
        onDirectionChange?.Invoke(currentDirection);
        OnDirectionChanged(currentDirection);
        HandleAnimations(currentDirection);
    }

    public void UnityEvent_OnAim(InputAction.CallbackContext context)
    {
        var direction = context.ReadValue<Vector2>().normalized;
        onDirectionChange?.Invoke(direction);
        OnDirectionChanged(direction);
    }

    public void UnityEvent_OnAimEnable(InputAction.CallbackContext context) => EnableAim(enable: context.ReadValueAsButton());

    public void UnityEvent_OnInteract(InputAction.CallbackContext _)
    {
        foreach (var i in interactables)
        {
            if (i == null) continue;

            // Only handle one thing per interaction, otherwise it might be confusing
            // Hence the "break" below
            i.Interact(this);
            return;
        }

        if (itemCache != null)
        {
            Vector2 direction = transform.rotation * Vector2.up;

            if (direction == Vector2.up)
            {
                itemCache.Collect(gameObject);
                itemCache = null;
                UpdateInteractIndicatorVisibility();
            }
            else
            {
                logger.W("TODO: handle player trying to collect item cache but not facing its front");
            }
        }

        if (readable != null)
        {
            readable.ReadMessage(inputManager);
        }

    }

    public void UnityEvent_OnAttack(InputAction.CallbackContext _) => meleeWeaponManager.Attack(this);

    public void UnityEvent_OnShoot(InputAction.CallbackContext _)
    {
        projectileManager.Shoot(new ProjectileManager.StartingPointWithDirection
        {
            direction = transform.rotation * Vector2.up,
            position = projectileSpawnPoint.position,
        });
    }

    public void UnityEvent_OnPrevious(InputAction.CallbackContext _)
    {
        if (inputManager.ModifyAction.IsPressed())
        {
            itemGroupsManager.CycleToPreviousGroup();
        }
        else
        {
            itemGroupsManager.CycleToPreviousItem();
        }
    }

    public void UnityEvent_OnNext(InputAction.CallbackContext _)
    {
        if (inputManager.ModifyAction.IsPressed())
        {
            itemGroupsManager.CycleToNextGroup();
        }
        else
        {
            itemGroupsManager.CycleToNextItem();
        }
    }

    public void UnityEvent_UI_OnSubmit(InputAction.CallbackContext _)
    {
        if (readable != null) readable.ShowNextMessagePageOrClose();
    }

    public void UnityEvent_OnInputTypeChanged(BetterInputManager.InputType type)
    {
        switch (type)
        {
            case BetterInputManager.InputType.UI:
            case BetterInputManager.InputType.None:
                rb.linearVelocity = Vector2.zero;
                currentDirection = Vector2.zero;
                break;
            case BetterInputManager.InputType.Aiming:
                rb.linearVelocity = Vector2.zero;
                currentDirection = Vector2.zero;
                var direction = inputManager.MoveAction.ReadValue<Vector2>();
                onDirectionChange?.Invoke(direction);
                OnDirectionChanged(direction);
                break;
            case BetterInputManager.InputType.Full:
                currentDirection = inputManager.MoveAction.ReadValue<Vector2>();
                onDirectionChange?.Invoke(currentDirection);
                OnDirectionChanged(currentDirection);
                EnableAim(inputManager.AimAction.IsPressed());
                break;
        }

        HandleAnimations(currentDirection);
    }

    // ──────────────────────────────────────────────────────────────
    // Other UnityEvent Methods
    // ──────────────────────────────────────────────────────────────

    private void ProjectileManager_OnProjectilChanged(ProjectileSO projectile) => eye.GetComponent<SpriteRenderer>().sprite = projectile.PlayerEye;

    private void ProjectileManager_OnShoot(Transform _) => RecoilBegin();

    public void TileTracker_OnTileChanged(Tilemap tilemap, Vector3Int cell)
    {
        logger.D($"tilemap {tilemap.name}, new tile cell {cell}");
    }

    public void TileTracker_OnTileTypeChanged(Tilemap tilemap, Vector3Int cell, TileTracker.TileType type)
    {
        logger.D($"tilemap {tilemap.name}, new tile cell {cell} type {type}");

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
            case TileTracker.TileType.Stairs:
                speedFactor = 0.75f;
                effectAnimator.Stop(clearSprite: true);
                effectAnimator.ClearCurrentAnimation();
                break;
        }
    }

    // ──────────────────────────────────────────────────────────────
    // Interface Implementation Methods
    // ──────────────────────────────────────────────────────────────

    // AutoMover.IAutoMoverTarget
    void AutoMover.IAutoMoverTarget.OnDirectionChanged(Vector2 direction)
    {
        OnDirectionChanged(direction);
        HandleAnimations(direction);
    }

    // ──────────────────────────────────────────────────────────────
    // Callbacks
    // ──────────────────────────────────────────────────────────────

    private void OnInteractableStateChange(Interactable i)
    {
        if (!interactables.Contains(i)) return;

        var found = interactables.Find(i => !i.TryGetComponent(out Interactable.IOverride o) || o.IsInteractable);

        if (found)
        {
            interactIndicatorVisibilityEvent.Raise(true);
        }
        else
        {
            interactIndicatorVisibilityEvent.Raise(false);
        }
    }

    // ──────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────

    private void UpdateInteractIndicatorVisibility()
    {
        var found = interactables.Find(i => !i.TryGetComponent(out Interactable.IOverride o) || o.IsInteractable)
            || readable != null
            || (itemCache != null && !itemCache.IsCollected);
        interactIndicatorVisibilityEvent.Raise(found);
    }

    private void HandleRecoil()
    {
        if (!shouldHandleRecoil) return;

        if (recoilTimerCounter <= 0)
        {
            RecoilEnd();
        }
        else
        {
            recoilTimerCounter -= Time.deltaTime;
        }
    }

    private void RecoilBegin()
    {
        recoilTimerCounter = recoilDuration;
        shouldHandleRecoil = true;
        inputType = BetterInputManager.InputType.None;
        inputManager.AddInputChangeRequest(this);
        head.transform.localPosition = headRecoilLocalPosition;
    }

    private void RecoilEnd()
    {
        shouldHandleRecoil = false;
        recoilTimerCounter = 0;
        head.transform.localPosition = headInitialLocalPosition;
        inputManager.RemoveInputChangeRequest(this);
    }

    private void EnableAim(bool enable)
    {
        if (enable)
        {
            inputType = BetterInputManager.InputType.Aiming;
            inputManager.AddInputChangeRequest(this);
        }
        else
        {
            inputManager.RemoveInputChangeRequest(this);
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
