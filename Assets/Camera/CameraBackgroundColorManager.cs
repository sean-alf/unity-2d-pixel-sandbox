using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Camera))]
public class CameraBackgroundColorManager : MonoBehaviour
{
    [SerializeField]
    private Tilemap tilemap;

    [SerializeField]
    [Range(0f, 1.0f)]
    private float shadeAdjustment;

    [Space]
    [Header("Debug")]

    private new Camera camera;

    private void OnEnable()
    {
        SetBackgroundColor();
    }

    void Awake()
    {
        SetBackgroundColor();
    }

    private void OnValidate()
    {
        SetBackgroundColor();
    }

    private void SetBackgroundColor()
    {
        if (tilemap == null)
        {
            return;
        }

        if (camera == null) camera = GetComponent<Camera>();

        Color newTintedColor = tilemap.color * new Color(shadeAdjustment, shadeAdjustment, shadeAdjustment, tilemap.color.a);

        camera.backgroundColor = newTintedColor;
    }
}
