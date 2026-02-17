using UnityEngine;

public class TrailMaker : MonoBehaviour
{
    [SerializeField] private GameObject trailParticleTemplate;
    [SerializeField] private float trailInterval = 0.1f;
    [SerializeField] private bool enable = true;

    [Space]
    [Header("Debug")]

    public float inheritedSpeed;
    public Vector2 inheritedDirection;

    private float trailTimer;

    void Update()
    {
        if (!enable) return;

        trailTimer -= Time.deltaTime;

        if (trailTimer <= 0)
        {
            SpawnTrailParticle();
            trailTimer = trailInterval;
        }
    }

    void SpawnTrailParticle()
    {
        Vector2 offset = Random.insideUnitCircle * 0.1f;
        var drop = Instantiate(trailParticleTemplate, (Vector2)transform.position + offset, Quaternion.identity);
        var droplet = drop.GetComponent<MudDroplet>();
        Vector2 dir = (inheritedDirection + Random.insideUnitCircle * 0.2f).normalized;
        droplet.Init(dir, inheritedSpeed);
    }

    public void Enable() => enable = true;

    public void Disable() => enable = false;
}
