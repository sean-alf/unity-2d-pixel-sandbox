using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Sword : MonoBehaviour
{
    [Header("Swing Settings")]
    [SerializeField] private float swingDuration = 0.3f;      // total time for 180° swing
    [SerializeField] private float swordRadius = 0.8f;        // distance from player center to sword tip

    [Header("Visuals (optional)")]
    [SerializeField] private SpriteRenderer arcFlash;         // optional arc effect

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private SpriteRenderer arcFlashSr;
    private Collider2D damageCollider;
    private IMeleeWeaponWielder wielder;
    private bool swingingForward = true;
    private float swingProgress = 0f;
    private bool isSwinging = false;
    private float startAngle;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        damageCollider = GetComponent<Collider2D>();
        damageCollider.enabled = false;

        if (arcFlash != null)
        {
            arcFlashSr = arcFlash.GetComponent<SpriteRenderer>();
            arcFlash.gameObject.SetActive(false);
        }
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"OnTriggerEnter2D: {other.name}");
    }

    // Called by player when attack button is pressed
    public void StartSwing(IMeleeWeaponWielder wielder, bool forwardSwing)
    {
        if (isSwinging) return;

        wielder.DisableInput(gameObject);

        isSwinging = true;
        this.wielder = wielder;
        swingingForward = forwardSwing;
        swingProgress = 0f;

        sr.flipX = forwardSwing; // Ensure sword sprite facing correct direction

        // Start at left or right edge of player
        startAngle = this.wielder.Transform.rotation.eulerAngles.z + (swingingForward ? 90f : -90f);  // 90° = up, -90° = down (relative to right)

        // Snap initial position & rotation
        Vector2 startDir = Quaternion.Euler(0, 0, startAngle) * Vector2.up;
        transform.SetPositionAndRotation(this.wielder.Transform.position.Add(startDir * swordRadius), Quaternion.Euler(0, 0, startAngle));

        damageCollider.enabled = true;

        if (arcFlash != null)
        {
            arcFlash.transform.localScale = Vector3.zero;
            arcFlash.color = Color.white;
            float x = forwardSwing ? -Mathf.Abs(arcFlash.transform.localPosition.x) : Mathf.Abs(arcFlash.transform.localPosition.x);
            arcFlash.transform.localPosition = new(x, arcFlash.transform.localPosition.y);
            arcFlashSr.flipX = forwardSwing;
            arcFlash.gameObject.SetActive(true);
        }

        gameObject.SetActive(true);
    }

    void FixedUpdate()
    {
        if (!isSwinging || wielder == null || wielder.Transform == null) return;

        // Progress 0 → 1
        swingProgress += Time.fixedDeltaTime / swingDuration;
        swingProgress = Mathf.Clamp01(swingProgress);

        // Swing angle: 90° → -90° or -90° → 90°
        float targetAngle = wielder.Transform.rotation.eulerAngles.z + (swingingForward ? -90f : 90f);
        float currentAngle = Mathf.Lerp(startAngle, targetAngle, swingProgress);

        // Position sword tip on arc around player center
        Vector2 direction = Quaternion.Euler(0, 0, currentAngle) * Vector2.up;
        Vector2 tipPosition = (Vector2)wielder.Transform.position + direction * swordRadius;

        // Apply physics movement
        rb.MovePosition(tipPosition);
        rb.MoveRotation(currentAngle);

        // Optional visual arc flash
        if (arcFlash != null)
        {
            float t = swingProgress;
            arcFlash.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 1.8f, t);
            arcFlash.color = new Color(1, 1, 1, Mathf.Lerp(1f, 0f, t));
        }

        // Swing complete
        if (swingProgress >= 1f)
        {
            FinishSwing();
        }
    }

    private void FinishSwing()
    {
        damageCollider.enabled = false;
        if (arcFlash != null) arcFlash.gameObject.SetActive(false);
        isSwinging = false;
        gameObject.SetActive(false);
        wielder.RestoreInput(gameObject);
    }
}