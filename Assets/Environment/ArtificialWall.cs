using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class ArtificialWall : MonoBehaviour
{
    [SerializeField]
    private float fadeOutDuration = 1f;

    [SerializeField]
    private int fadeOutSteps = 10;

    private static WaitForSeconds _waitForSeconds;
    public UnityEvent onDone;

    private Tilemap artificialWall;

    private void Awake()
    {
        _waitForSeconds = new(fadeOutDuration / fadeOutSteps);
    }

    private void Start()
    {
        var artificialWallGO = GameObject.Find("Artificial Wall");

        if (artificialWallGO != null && !artificialWallGO.TryGetComponent(out artificialWall))
        {
            Debug.LogError($"ArtificialWall: no Artificial Wall tilemap found!", this);
        }
    }

    public void RemoveWall()
    {
        var list = new List<Vector3Int>();
        var initialPos = artificialWall.WorldToCell(transform.position);
        GetAllNeighbors(initialPos, list);
        StartCoroutine(FadeOut(list));
    }

    private void GetAllNeighbors(Vector3Int pos, List<Vector3Int> list)
    {
        var tile = artificialWall.GetTile(pos);
        if (list.Contains(pos) || tile == null) return;
        list.Add(pos);

        // World units are 1
        GetAllNeighbors(pos + Vector3Int.up, list);
        GetAllNeighbors(pos + Vector3Int.right, list);
        GetAllNeighbors(pos + Vector3Int.down, list);
        GetAllNeighbors(pos + Vector3Int.left, list);
    }

    private IEnumerator FadeOut(List<Vector3Int> list)
    {
        float alpha = 1f;

        do
        {
            alpha -= 0.1f;

            foreach (var pos in list)
            {
                var c = artificialWall.GetColor(pos);
                c.a = alpha;
                artificialWall.SetColor(pos, c);
            }

            yield return _waitForSeconds;
        } while (alpha > 0.05);

        foreach (var pos in list) artificialWall.SetTile(pos, null);
        onDone?.Invoke();
    }
}
