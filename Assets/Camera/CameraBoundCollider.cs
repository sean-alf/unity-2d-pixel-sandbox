using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(BoxCollider2D))]
public class CameraBoundCollider : MonoBehaviour
{
    public bool OverrideManager => overrideManager;

    public enum Position
    {
        TOP,
        LEFT,
        RIGHT,
        BOTTOM,
    }

    [SerializeField]
    private Position position;

    [SerializeField]
    private bool overrideManager = false;

    [SerializeField]
    [Range(0, 128)]
    private int pixelThickness = 32;

    [SerializeField]
    [Range(0, 128)]
    private int pixelOffset = 32;

    private Camera c;
    private BoxCollider2D bc;
    private Vector2 size = new();

    private void OnEnable()
    {
        if (bc == null) bc = GetComponent<BoxCollider2D>();
    }

    private void Awake()
    {
        c = GetComponentInParent<Camera>();

        if (bc == null) bc = GetComponent<BoxCollider2D>();

        // Make sure to clear out any offset
        bc.offset = new(0, 0);
        // Make sure the x and y scale is always 1
        transform.localScale = new(1, 1);
    }

    public void ResetBound()
    {
        if ((!Application.isPlaying && !Application.isEditor) || c == null) return;

        float halfWidth = c.orthographicSize * c.aspect;
        float halfHeight = c.orthographicSize;

        SetSize(halfWidth, halfHeight);
        SetPosition(halfWidth, halfHeight);
    }

    public void ResetBound(int thickness, int offset)
    {
        if ((!Application.isPlaying && !Application.isEditor) || c == null) return;

        pixelThickness = thickness;
        pixelOffset = offset;

        float halfWidth = c.orthographicSize * c.aspect;
        float halfHeight = c.orthographicSize;

        SetSize(halfWidth, halfHeight);
        SetPosition(halfWidth, halfHeight);
    }

    private void SetSize(float halfWidth, float halfHeight)
    {
        size.x = pixelThickness;
        size.y = pixelThickness;

        switch (position)
        {
            case Position.TOP:
            case Position.BOTTOM:
                {
                    size.x = (size.x + pixelOffset + halfWidth) * 2;
                    break;
                }
            case Position.LEFT:
            case Position.RIGHT:
                {
                    size.y = (size.y + pixelOffset + halfHeight) * 2;
                    break;
                }
        }

        // Set the box collider size now that the sprite's size has been updated
        bc.size = size;
    }

    private void SetPosition(float halfWidth, float halfHeight)
    {
        float x = c.transform.position.x;
        float y = c.transform.position.y;

        switch (position)
        {
            case Position.LEFT:
                x += -(halfWidth + (bc.size.x / 2) + pixelOffset);
                break;
            case Position.RIGHT:
                x += halfWidth + (bc.size.x / 2) + pixelOffset;
                break;
            case Position.TOP:
                y += halfHeight + (bc.size.y / 2) + pixelOffset;
                break;
            case Position.BOTTOM:
                y += -(halfHeight + (bc.size.y / 2) + pixelOffset);
                break;
        }

        transform.position = new(x, y, transform.position.z);
    }

    private void OnValidate()
    {
        ResetBound();
    }
}
