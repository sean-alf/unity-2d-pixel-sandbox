using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ReflectingWall : MonoBehaviour
{
    [SerializeField] private float colorLerpSpeed = 5f;
    [SerializeField] private float colorDistanceThreshold = 0.3f;
    [SerializeField] private bool useGreenTileFeedback = false;

    [Space]
    [Header("Debug")]

    public GameObject reflectionAnimationTemplate;

    public Tilemap tilemap;
    public Color defaultTileColor;

    private readonly Dictionary<Vector3Int, Coroutine> coroutines = new();

    private Vector3Int tileCell = default;
    private Vector2 point = default;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contactCount == 0) return;

        if (collision.gameObject.TryGetComponent(out IReflectable r))
        {
            var dotProduct = Vector2.Dot(collision.contacts[0].normal, collision.relativeVelocity.normalized);

            if (Mathf.Abs(dotProduct) <= 0.01f) return;

            var reflect = Vector2.Reflect(collision.relativeVelocity, collision.contacts[0].normal);

            var reflected = r.Reflect(reflect);
            if (reflected) RunTileFeedback(collision);
        }
    }

    private void RunTileFeedback(Collision2D collision)
    {
        if (collision.contactCount > 1)
        {
            // Reset point
            point = default;

            // Just take the average
            foreach (var contact in collision.contacts)
            {
                point.x += contact.point.x;
                point.y += contact.point.y;
            }

            point.x /= collision.contactCount;
            point.y /= collision.contactCount;
        }
        else
        {
            point = collision.contacts[0].point;
        }

        var cell = tilemap.WorldToCell(collision.contacts[0].point);
        var tile = tilemap.GetTile(cell);

        if (tile == null) return;

        tileCell = cell;

        // var point = tilemap.CellToWorld(cell).Add(0.5f, 0.25f);
        var go = Instantiate(reflectionAnimationTemplate, point, Quaternion.identity);
        var animator = go.GetComponent<LinearAnimator>();
        animator.Animate("Default", onFinished: () => Destroy(go));

        if (useGreenTileFeedback) PlayCoroutine(cell);
    }

    private void PlayCoroutine(Vector3Int cell)
    {
        if (coroutines.ContainsKey(cell))
        {
            if (coroutines[cell] != null)
            {
                StopCoroutine(coroutines[cell]);
            }

            coroutines[cell] = StartCoroutine(PulseColor(cell));
        }
        else
        {
            coroutines.Add(cell, StartCoroutine(PulseColor(cell)));
        }
    }

    private IEnumerator PulseColor(Vector3Int tileCell)
    {
        Color currentColor = tilemap.GetColor(tileCell);
        Color targetColor = Color.green;

        // Pulse to target color
        while (ColorDistance(currentColor, targetColor) > colorDistanceThreshold)
        {
            currentColor = Color.Lerp(currentColor, targetColor, colorLerpSpeed * Time.deltaTime);
            tilemap.SetColor(tileCell, currentColor);
            yield return null;
        }

        // Snap to target color
        tilemap.SetColor(tileCell, targetColor);
        currentColor = targetColor;

        // Pulse back to original
        while (ColorDistance(currentColor, defaultTileColor) > colorDistanceThreshold)
        {
            currentColor = Color.Lerp(currentColor, defaultTileColor, colorLerpSpeed * Time.deltaTime);
            tilemap.SetColor(tileCell, currentColor);
            yield return null;
        }

        // Snap to default color
        tilemap.SetColor(tileCell, defaultTileColor);
        // Clear the coroutine to signal that it's finished
        coroutines[tileCell] = null;
    }

    private float ColorDistance(Color a, Color b)
    {
        float dr = a.r - b.r;
        float dg = a.g - b.g;
        float db = a.b - b.b;
        float da = a.a - b.a;
        return Mathf.Sqrt(dr * dr + dg * dg + db * db + da * da);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(tilemap.CellToWorld(tileCell).Add(0.5f, 0.5f), 0.5f);
        Gizmos.color = Color.skyBlue;
        Gizmos.DrawWireSphere(point, 0.5f);

    }

    public interface IReflectable
    {
        public bool Reflect(Vector2 reflect);
    }
}
