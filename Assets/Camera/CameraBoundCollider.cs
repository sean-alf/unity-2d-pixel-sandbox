using UnityEngine;
using UnityEngine.Rendering.Universal;

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
    private PixelPerfectCamera ppc;
    private BoxCollider2D bc;
    private Vector2 size = new();
    private float _pixelThickness;
    private float _pixelOffset;

    private void Awake()
    {
        bc = GetComponent<BoxCollider2D>();

        // Make sure to clear out any offset
        bc.offset = new(0, 0);
        // Make sure the x and y scale is always 1
        transform.localScale = new(1, 1);
    }

    private void Start()
    {
        c = GetComponentInParent<Camera>();
        ppc = GetComponentInParent<PixelPerfectCamera>();

        UpdateDimensProperties();
    }

    private void OnValidate()
    {
        ResetBound();
    }

    public void ResetBound()
    {
        c = GetComponentInParent<Camera>();
        ppc = GetComponentInParent<PixelPerfectCamera>();
        bc = GetComponent<BoxCollider2D>();

        if ((!Application.isPlaying && !Application.isEditor) || c == null || ppc == null) return;

        UpdateDimensProperties();

        float halfWidth = c.orthographicSize * c.aspect;
        float halfHeight = c.orthographicSize;

        SetSize(halfWidth, halfHeight);
        SetPosition(halfWidth, halfHeight);
    }

    public void ResetBound(int thickness, int offset)
    {
        c = GetComponentInParent<Camera>();
        ppc = GetComponentInParent<PixelPerfectCamera>();
        bc = GetComponent<BoxCollider2D>();

        if ((!Application.isPlaying && !Application.isEditor) || c == null || ppc == null) return;

        UpdateDimensProperties(thickness, offset);

        float halfWidth = c.orthographicSize * c.aspect;
        float halfHeight = c.orthographicSize;

        SetSize(halfWidth, halfHeight);
        SetPosition(halfWidth, halfHeight);
    }

    private void SetSize(float halfWidth, float halfHeight)
    {
        size.x = _pixelThickness;
        size.y = _pixelThickness;

        switch (position)
        {
            case Position.TOP:
            case Position.BOTTOM:
                {
                    size.x = (size.x + _pixelOffset + halfWidth) * 2;
                    break;
                }
            case Position.LEFT:
            case Position.RIGHT:
                {
                    size.y = (size.y + _pixelOffset + halfHeight) * 2;
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
                x += -(halfWidth + (bc.size.x / 2) + _pixelOffset);
                break;
            case Position.RIGHT:
                x += halfWidth + (bc.size.x / 2) + _pixelOffset;
                break;
            case Position.TOP:
                y += halfHeight + (bc.size.y / 2) + _pixelOffset;
                break;
            case Position.BOTTOM:
                y += -(halfHeight + (bc.size.y / 2) + _pixelOffset);
                break;
        }

        transform.position = new(x, y, transform.position.z);
    }

    private void UpdateDimensProperties()
    {
        _pixelThickness = pixelThickness / (float)ppc.assetsPPU;
        _pixelOffset = pixelOffset / (float)ppc.assetsPPU;
    }

    private void UpdateDimensProperties(int thickness, int offset)
    {
        pixelThickness = thickness;
        pixelOffset = offset;

        _pixelThickness = pixelThickness / (float)ppc.assetsPPU;
        _pixelOffset = pixelOffset / (float)ppc.assetsPPU;
    }
}
