using UnityEngine;

public class Conveyer : MonoBehaviour
{
    [SerializeField]
    [Range(1, 100)]
    private int length = 1;

    [SerializeField]
    private CardinalDirection direction;

    [SerializeField]
    private bool autoStart = true;

    [SerializeField]
    private float speed = 2f;

    [SerializeField]
    private bool showStartEdge = true;

    [SerializeField]
    private bool showEndEdge = true;

    [Space]
    [Header("Debug")]

    [SerializeField]
    private bool active = false;

    private LinearAnimator animator;
    private Transform startEdge;
    private Transform track;
    private Transform endEdge;

    private void Awake()
    {
        UpdatePositionAndRotation();
    }

    private void Start()
    {
        animator = GetComponentInChildren<LinearAnimator>();

        UpdateAnimationSpeed();

        if (autoStart) Activate();
    }

    private void OnTriggerEnter2D(Collider2D other) => other.WhenFound<ExternalForceReceiver>(
        efr => efr.AddForce(this, direction.ToVector2() * speed)
    );

    private void OnTriggerExit2D(Collider2D other) => other.WhenFound<ExternalForceReceiver>(
        efr => efr.RemoveForce(this)
    );

    private void OnValidate()
    {
        UpdatePositionAndRotation();
        UpdateAnimationSpeed();
    }

    public void Activate()
    {
        if (active || animator == null) return;
        active = true;

        if (direction == CardinalDirection.Right || direction == CardinalDirection.Down)
        {
            animator.Animate("Default");
        }
        else
        {
            animator.AnimateReverse("Default");
        }

        track.WhenFound<Collider2D>(c => c.enabled = true);
    }

    public void Stop()
    {
        if (!active || animator == null) return;
        active = false;
        animator.Stop();
        track.WhenFound<Collider2D>(c => c.enabled = false);
    }

    private void UpdateAnimationSpeed()
    {
        if (animator != null)
        {
            float duration = 1f / speed;
            animator.SetDuration("Default", duration);
        }
    }


    private void UpdatePositionAndRotation()
    {
        startEdge = transform.Find("StartEdge");
        track = transform.Find("Track");
        endEdge = transform.Find("EndEdge");

        if (track.TryGetComponent(out SpriteRenderer trackSR)
            && startEdge.TryGetComponent(out SpriteRenderer startSR)
            && endEdge.TryGetComponent(out SpriteRenderer endSR))
        {
            startSR.flipX = direction == CardinalDirection.Left || direction == CardinalDirection.Up;
            trackSR.flipY = direction == CardinalDirection.Up || direction == CardinalDirection.Down;
            endSR.flipX = direction == CardinalDirection.Left || direction == CardinalDirection.Up;

            trackSR.size = new(length, trackSR.size.y);

            if (direction == CardinalDirection.Right || direction == CardinalDirection.Left)
            {
                transform.rotation = Quaternion.identity;
            }
            else
            {
                transform.rotation = Quaternion.FromToRotation(CardinalDirection.Right.ToVector2(), CardinalDirection.Down.ToVector2());
            }


            startEdge.localPosition = new(trackSR.localBounds.min.x - startSR.sprite.bounds.extents.x, trackSR.localBounds.center.y);
            endEdge.localPosition = new(trackSR.localBounds.max.x + endSR.sprite.bounds.extents.x, trackSR.localBounds.center.y);
        }

        startEdge.gameObject.SetActive(showStartEdge);
        endEdge.gameObject.SetActive(showEndEdge);

        if (Application.isPlaying)
        {
            active = false;
            Activate();
        }
    }
}
