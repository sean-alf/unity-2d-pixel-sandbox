using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SpiralPathFollower : MonoBehaviour
{
    [SerializeField] private float duration = 3f;
    [SerializeField] private float rotations = 3f;
    [SerializeField] private float startingRadius = 0.5f;
    [SerializeField] private float targetRadius = 10f;

    [Header("Trail")]
    [SerializeField] private GameObject particleTemplate;
    [SerializeField] private float trailInterval = 0.1f;

    [Space]
    [Header("Debug")]
    [SerializeField] private Vector2 startingPosition;
    [SerializeField] private float startingAngle;
    [SerializeField] private float targetAngle;
    [SerializeField] private float currentAngle = 0f;
    [SerializeField] private float currentRadius = 0f;
    [SerializeField] private float timer;
    [SerializeField] private float trailTimer;

    private Rigidbody2D rb;
    private Action onDone;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (timer <= 0) return;

        trailTimer -= Time.deltaTime;

        if (trailTimer <= 0f)
        {
            trailTimer = trailInterval;
            Instantiate(particleTemplate, transform.position, Quaternion.identity);
        }
    }

    private void FixedUpdate()
    {
        if (timer <= 0f) return;
        var t = CalculateT();
        currentRadius = CalculateRadius(t);
        currentAngle = CalculateAngle(t);
        var pos = CalculatePosition(currentRadius, currentAngle);
        rb.MovePosition(startingPosition + pos);
        if (timer <= 0f) onDone?.Invoke();
    }

    public void StartSpiralPath(Vector2 startingPosition, float startingAngle = 0f, bool reverse = false, Action onDone = null)
    {
        this.onDone = onDone;
        this.startingPosition = startingPosition;
        this.startingAngle = startingAngle;

        currentRadius = startingRadius;
        currentAngle = startingAngle;
        CalculateTargetAngle(startingAngle, reverse);
        timer = duration;
    }

    private void CalculateTargetAngle(float startingAngle, bool reverse = false)
    {
        targetAngle = startingAngle + (reverse ? -1f : 1f) * rotations * 360f;
    }

    private float CalculateT()
    {
        timer -= Time.fixedDeltaTime;
        return (duration - timer) / duration;
    }

    private float CalculateRadius(float t) => Mathf.Lerp(startingRadius, targetRadius, t);

    private float CalculateAngle(float t) => Mathf.Lerp(startingAngle, targetAngle, t);

    private Vector2 CalculatePosition(float radius, float angle) => radius * new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
}
