using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class CameraBoundCollider : MonoBehaviour
{
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
    private int pixelOffset;

    private Camera c;
    private BoxCollider2D bc;
    private SpriteRenderer sr;

    private void Awake()
    {
        c = GetComponentInParent<Camera>();

        bc = GetComponent<BoxCollider2D>();
        sr = GetComponent<SpriteRenderer>();

        // Make sure to clear out any offset
        bc.offset = new(0, 0);
        // Set draw mode to tiled to simplify the sprite sizing
        sr.drawMode = SpriteDrawMode.Tiled;
        // Make sure the x and y scale is always 1
        transform.localScale = new(1, 1);
    }

    public void Reset()
    {
        if ((!Application.isPlaying && !Application.isEditor) || c == null) return;

        float halfWidth = c.orthographicSize * c.aspect;
        float halfHeight = c.orthographicSize;

        SetSize(halfWidth, halfHeight);
        SetPosition(halfWidth, halfHeight);
    }

    private void SetSize(float halfWidth, float halfHeight)
    {
        float x = sr.sprite.bounds.size.x;
        float y = sr.sprite.bounds.size.y;

        switch (position)
        {
            case Position.TOP:
            case Position.BOTTOM:
                {
                    x = (x + pixelOffset + halfWidth) * 2;
                    break;
                }
            case Position.LEFT:
            case Position.RIGHT:
                {
                    y = (y + pixelOffset + halfHeight) * 2;
                    break;
                }
        }

        sr.size = new(x, y);

        // Set the box collider size now that the sprite's size has been updated
        bc.size = sr.size;
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
}
