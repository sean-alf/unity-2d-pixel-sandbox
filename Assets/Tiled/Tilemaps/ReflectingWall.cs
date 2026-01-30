using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ReflectingWall : MonoBehaviour
{
    [SerializeField] private float colorLerpSpeed = 5f;

    public Tilemap tilemap;
    public Color defaultTileColor;

    private Vector3Int tileCell = default;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent(out IReflectable r))
        {
            var reflect = Vector2.Reflect(collision.relativeVelocity, collision.contacts[0].normal);
            var reflected = r.Reflect(reflect);
            if (reflected) RunTileFeedback(collision);
        }
    }

    private void RunTileFeedback(Collision2D collision)
    {
        foreach (var c in collision.contacts)
        {
            Vector3 contactPoint = c.point + c.normal * 0.01f;
            var cell = tilemap.WorldToCell(contactPoint);
            var tile = tilemap.GetTile(cell);

            if (cell == tileCell || tile == null) continue;

            tileCell = cell;
            StartCoroutine(PulseColor(tileCell));
        }
    }

    private IEnumerator PulseColor(Vector3Int tileCell)
    {
        tilemap.SetTileFlags(tileCell, TileFlags.None);
        Color startColor = defaultTileColor;
        Color targetColor = Color.green;

        // Pulse to target color
        for (float t = 0; t < 1; t += Time.deltaTime * colorLerpSpeed)
        {
            tilemap.SetColor(tileCell, Color.Lerp(startColor, targetColor, t));
            yield return null;
        }

        // Pulse back to original
        for (float t = 0; t < 1; t += Time.deltaTime * colorLerpSpeed)
        {
            tilemap.SetColor(tileCell, Color.Lerp(targetColor, startColor, t));
            yield return null;
        }

        tilemap.SetColor(tileCell, startColor);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(tilemap.CellToWorld(tileCell).Add(0.5f, 0.5f), 0.5f);
    }

    public interface IReflectable
    {
        public bool Reflect(Vector2 reflect);
    }
}
