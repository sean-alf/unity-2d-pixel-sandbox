using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Spear : MonoBehaviour, BetterInputManager.IInputChangeRequestor, IMeleeWeapon
{
    [SerializeField][Range(0, 100)] int inputChangeRequestPriority;
    [SerializeField] private float jabSpeed = 1f;
    [SerializeField] private float offset = 0.25f;
    [SerializeField] private float jabDistance = 0.5f;

    [Space]
    [Header("Debug")]

    [SerializeField] private Vector2 originPos;
    [SerializeField] private Vector2 targetPos;
    [SerializeField] private bool isAttacking = false;
    [SerializeField] private bool isForward = false;

    public int Priority => inputChangeRequestPriority;
    public string Name => $"{name} ({GetType().Name})";
    public BetterInputManager.InputType InputType => BetterInputManager.InputType.None;

    private readonly UnityEvent onAttackFinished = new();

    private Rigidbody2D rb;
    private Collider2D damageCollider;
    private IMeleeWeaponWielder wielder;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        damageCollider = GetComponent<Collider2D>();

        damageCollider.enabled = false;
        gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        if (!isAttacking || wielder == null) return;

        if (isForward)
        {
            // Jab forward
            rb.MoveTowards(targetPos, Time.fixedDeltaTime * jabSpeed);

            if (Vector2.Distance(rb.position, targetPos) <= 0.01f)
            {
                rb.position = targetPos;
                isForward = false;
            }
        }
        else
        {
            // Pull back
            rb.MoveTowards(originPos, Time.fixedDeltaTime * jabSpeed);

            if (Vector2.Distance(rb.position, originPos) <= 0.01f)
            {
                // Snap to position
                rb.position = originPos;
                EndJab();
            }
        }
    }

    private void StartJab(IMeleeWeaponWielder wielder)
    {
        if (isAttacking) return;

        wielder.InputManager.AddInputChangeRequest(this);

        this.wielder = wielder;
        isAttacking = true;
        isForward = true;

        transform.SetPositionAndRotation(wielder.Transform.position + wielder.Transform.right * offset, wielder.Transform.rotation);
        originPos = rb.position;
        targetPos = rb.position + ((Vector2)wielder.Transform.up * jabDistance);

        damageCollider.enabled = true;

        gameObject.SetActive(true);
    }

    private void EndJab()
    {
        damageCollider.enabled = false;
        isAttacking = false;
        wielder.InputManager.RemoveInputChangeRequest(this);
        wielder = null;
        gameObject.SetActive(false);
        onAttackFinished?.Invoke();
    }

    // IMeleeWeapon
    public void Attack(IMeleeWeaponWielder wielder) => StartJab(wielder);

    // IMeleeWeapon
    public void RegisterOnAttackFinished(UnityAction onFinished) => onAttackFinished.AddListener(onFinished);

    // IMeleeWeapon
    public void UnregisterOnAttackFinished(UnityAction onFinished) => onAttackFinished.RemoveListener(onFinished);
}
