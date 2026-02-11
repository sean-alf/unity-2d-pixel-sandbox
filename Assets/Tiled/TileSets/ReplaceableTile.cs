using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "ReplaceableTile", menuName = "Tiles/ReplaceableTile")]
public class ReplaceableTile : TileBase
{
    public enum TileType
    {
        Bush,
    }

    [SerializeField] private TileType type;
    [SerializeField] private Sprite initialTile;
    [SerializeField] private Sprite replaceWith;
    [SerializeField] private GameObject optionalReplaceAnimationTemplate;

    public TileType Type => type;

    private readonly List<Vector3Int> tileState = new();

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        bool isReplaced = tileState.Contains(position);
        tileData.sprite = isReplaced ? replaceWith : initialTile;
        tileData.colliderType = isReplaced ? Tile.ColliderType.None : Tile.ColliderType.Sprite;
    }

    public void Replace(Tilemap tilemap, Vector3Int position, Action replaceAnimationDone = null)
    {
        if (tileState.Contains(position)) return;

        tileState.Add(position);

        if (optionalReplaceAnimationTemplate)
        {
            var worldSpace = tilemap.CellToWorld(position);
            var template = Instantiate(optionalReplaceAnimationTemplate, worldSpace.Add(0.5f, 0.5f), Quaternion.identity);
            var animator = template.GetComponent<LinearAnimator>();
            animator.Animate("Default", replaceAnimationDone);
        }

        tilemap.RefreshTile(position);
    }
}
