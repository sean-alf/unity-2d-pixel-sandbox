using UnityEngine;
using UnityEngine.Tilemaps;

public class ReplaceableTileTilemapManager : MonoBehaviour
{
    public Tilemap tilemap;

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent(out Sword _))
        {
            Debug.Log($"{name} ({GetType().Name}): got hit by sword!!");

            if (collision.contactCount > 0)
            {
                var cell = tilemap.WorldToCell(collision.GetContact(0).point);
                var tile = tilemap.GetTile<ReplaceableTile>(cell);

                if (tile != null)
                {
                    Debug.Log($"{name} ({GetType().Name}): tile {tile}, cell {cell}, type {tile.Type}");
                    tile.Replace(tilemap, cell);
                }
            }
        }
    }
}
