using SuperTiled2Unity;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AboveGroundTilemapManager : MonoBehaviour
{
    public Tilemap tilemap;

    [SerializeField] private TileBase bushChoppedTile;

    private void Awake()
    {
        if (bushChoppedTile == null)
        {
            Debug.LogError("AboveGroundTilemapManager: bushChoppedTile must not be null!");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent(out Sword _))
        {
            Debug.Log($"AboveGroundTilemapManager: got hit by sword!!");

            if (collision.contactCount > 0)
            {
                var cell = tilemap.WorldToCell(collision.GetContact(0).point);
                var tile = tilemap.GetTile<SuperTile>(cell);
                var type = tile.GetPropertyValueAsString("destroyableType", "");

                Debug.Log($"AboveGroundTilemapManager: tile {tile}, cell {cell}, destroyableType {type}");

                if (type == "Bush")
                {
                    tilemap.SetTile(cell, bushChoppedTile);
                }
            }
        }
    }
}
