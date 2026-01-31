using SuperTiled2Unity;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AboveGroundTilemapManager : MonoBehaviour
{
    public Tilemap tilemap;

    [SerializeField] private TileBase bushChoppedTile;
    [SerializeField] private GameObject bushChoppedAnimationTemplate;

    private void Awake()
    {
        if (bushChoppedTile == null)
        {
            Debug.LogError("AboveGroundTilemapManager: bushChoppedTile must not be null!");
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
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
                    var worldSpace = tilemap.CellToWorld(cell);
                    var tileCenterInWorld = worldSpace + new Vector3(0.5f, 0.5f, worldSpace.z);
                    var template = Instantiate(bushChoppedAnimationTemplate, tileCenterInWorld, Quaternion.identity);
                    var animator = template.GetComponent<LinearAnimator>();
                    animator.Animate("Default");
                    tilemap.SetTile(cell, bushChoppedTile);
                }
            }
        }
    }
}
