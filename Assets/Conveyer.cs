using UnityEditor;
using UnityEngine;

public class Conveyer : MonoBehaviour
{
    [SerializeField]
    [Range(1, 100)]
    private int length = 1;

    [SerializeField]
    [Range(2, 50)]
    private int width = 2;

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

#if UNITY_EDITOR
    [SerializeField]
    private bool animateInEditor = false;
#endif

    private Transform startEdge;
    private Transform track;
    private Transform endEdge;
    private MaterialPropertyBlock materialPropertyBlock;

    private static readonly int SpeedID = Shader.PropertyToID("_Speed");

    private void Awake()
    {
        startEdge = transform.Find("StartEdge");
        track = transform.Find("Track");
        endEdge = transform.Find("EndEdge");
        materialPropertyBlock = new();
        UpdatePositionAndRotation();
    }

    private void Start()
    {
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
        startEdge = transform.Find("StartEdge");
        track = transform.Find("Track");
        endEdge = transform.Find("EndEdge");

        materialPropertyBlock ??= new();

#if UNITY_EDITOR
        EditorApplication.delayCall += () =>
        {
            if (this == null) return;
            UpdatePositionAndRotation();
            if (animateInEditor || Application.isPlaying)
            {
                UpdateAnimationSpeed();
            }
            else
            {
                track.WhenFound<SpriteRenderer>(trackSR => StopTrackAnimation(trackSR));
            }
        };
#else
        UpdatePositionAndRotation();
        UpdateAnimationSpeed();
#endif
    }

    public void Activate()
    {
        UpdateAnimationSpeed();
        track.WhenFound<Collider2D>(c => c.enabled = true);
    }

    public void Stop()
    {
        track.WhenFound<SpriteRenderer>(trackSR => StopTrackAnimation(trackSR));
        track.WhenFound<Collider2D>(c => c.enabled = false);
    }

    private void UpdateAnimationSpeed()
    {
        if (track != null && track.TryGetComponent(out SpriteRenderer trackSR))
        {
            var sign = direction == CardinalDirection.Right || direction == CardinalDirection.Down ? -1 : 1;

            trackSR.GetPropertyBlock(materialPropertyBlock);
            materialPropertyBlock.SetVector(SpeedID, new(sign * speed, 0));
            trackSR.SetPropertyBlock(materialPropertyBlock);
        }
    }

    private void StopTrackAnimation(SpriteRenderer trackSR)
    {
        trackSR.GetPropertyBlock(materialPropertyBlock);
        materialPropertyBlock.SetVector(SpeedID, Vector2.zero);
        trackSR.SetPropertyBlock(materialPropertyBlock);
    }

    private void UpdatePositionAndRotation()
    {
        if (track.TryGetComponent(out SpriteRenderer trackSR)
            && startEdge.TryGetComponent(out SpriteRenderer startSR)
            && endEdge.TryGetComponent(out SpriteRenderer endSR))
        {
            startSR.flipX = direction == CardinalDirection.Left || direction == CardinalDirection.Up;
            trackSR.flipY = direction == CardinalDirection.Up || direction == CardinalDirection.Down;
            endSR.flipX = direction == CardinalDirection.Left || direction == CardinalDirection.Up;
            startSR.size = new(startSR.size.x, width);
            trackSR.size = new(length, width);
            endSR.size = startSR.size;

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
        Activate();
    }
}
