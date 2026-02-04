using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class RedirectionPanel : MonoBehaviour
{
    [SerializeField] private CardinalDirection direction;
    [SerializeField] private bool panelActivated = true;
    [SerializeField] private Sprite activatedSprite;
    [SerializeField] private Sprite deactivatedSprite;

    private readonly Dictionary<BasicProjectile, float> targets = new();

    private void Awake() => Init();

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            Init();
        }
        else
        {
            EditorApplication.delayCall += () =>
            {
                if (this == null) return;
                Init();
            };
        }
#endif
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // If it's a projectile, then cached a reference to it
        if (other.gameObject.TryGetComponent(out BasicProjectile p) && !targets.ContainsKey(p))
        {
            var v = other.attachedRigidbody.linearVelocity;
            targets.Add(p, Mathf.Max(Mathf.Abs(v.x), Mathf.Abs(v.y)));
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.TryGetComponent(out BasicProjectile p) && targets.ContainsKey(p)) targets.Remove(p);
    }

    private void FixedUpdate()
    {
        if (!panelActivated) return;

        foreach (var target in targets)
        {
            var p = target.Key;

            if (p.TryGetComponent(out Rigidbody2D rb))
            {
                var v = rb.linearVelocity;
                bool movingX = Mathf.Abs(v.x) > Mathf.Abs(v.y);
                bool movingY = Mathf.Abs(v.y) > Mathf.Abs(v.x);
                bool movingBoth = Mathf.Abs(v.y) == Mathf.Abs(v.x);

                float xDistance = Mathf.Abs(target.Key.transform.position.x - transform.position.x);
                float yDistance = Mathf.Abs(target.Key.transform.position.y - transform.position.y);

                if (movingX && xDistance <= 0.1 || movingY && yDistance <= 0.1)
                {
                    rb.linearVelocity = target.Value * direction.ToVector2();
                }
                else if (movingBoth && xDistance <= 1f / 2f && yDistance <= 1f / 2f)
                {
                    rb.linearVelocity = target.Value * direction.ToVector2();
                }

                p.UpdateRotation();
            }
        }
    }

    public void ActivatePanel()
    {
        if (panelActivated) return;
        panelActivated = true;
        UpdateSpriteBaseOnActivationState();
    }

    public void DeactivatePanel()
    {
        if (!panelActivated) return;
        panelActivated = false;
        UpdateSpriteBaseOnActivationState();
    }

    private void Init()
    {
        transform.rotation = Quaternion.LookRotation(Vector3.forward, direction.ToVector2());
        UpdateSpriteBaseOnActivationState();
    }

    private void UpdateSpriteBaseOnActivationState()
    {
        var sr = GetComponent<SpriteRenderer>();
        sr.sprite = panelActivated ? activatedSprite : deactivatedSprite;
    }
}
