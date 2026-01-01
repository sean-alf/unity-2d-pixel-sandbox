using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(BoxCollider2D))]
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

    [SerializeField]
    [Tooltip("Dummy bool value  to use as a reset button to reset the bound")]
    private bool reset = false;

    private Camera c;
    private BoxCollider2D bc;

    private void OnEnable()
    {
        if (c == null)
        {
            c = GetComponentInParent<Camera>();
        }

        if (bc == null)
        {
            bc = GetComponent<BoxCollider2D>();
        }

        PositionBound();
    }

    private void OnValidate()
    {
        if (reset)
        {
            PositionBound();
        }
    }

    private void PositionBound()
    {
        if (c == null)
        {
            throw new($"CameraBoundCollider ({gameObject.name}): Camera not found in parent!");
        }

        float x = c.transform.position.x;
        float y = c.transform.position.y;

        switch (position)
        {
            case Position.LEFT:
                x += -((c.orthographicSize * c.aspect) + (bc.size.x / 2) + pixelOffset);
                break;
            case Position.RIGHT:
                x += (c.orthographicSize * c.aspect) + (bc.size.x / 2) + pixelOffset;
                break;
            case Position.TOP:
                y += c.orthographicSize + (bc.size.y / 2) + pixelOffset;
                break;
            case Position.BOTTOM:
                y += -(c.orthographicSize + (bc.size.y / 2) + pixelOffset);
                break;
        }

        transform.position = new(x, y, transform.position.z);
    }
}
