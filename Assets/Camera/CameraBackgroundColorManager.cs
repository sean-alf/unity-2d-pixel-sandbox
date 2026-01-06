using UnityEngine;
using UnityEngine.Tilemaps;

public static class ColorExtensions
{
    public static Color ToLinearSpace(this Color c)
    {
        return new(
            r: Mathf.GammaToLinearSpace(c.r),
            g: Mathf.GammaToLinearSpace(c.g),
            b: Mathf.GammaToLinearSpace(c.b),
            a: c.a
        );
    }

    public static Color ToGammaSpace(this Color c)
    {
        return new(
            r: Mathf.LinearToGammaSpace(c.r),
            g: Mathf.LinearToGammaSpace(c.g),
            b: Mathf.LinearToGammaSpace(c.b),
            a: c.a
        );
    }
}

[RequireComponent(typeof(Camera))]
public class CameraBackgroundColorManager : MonoBehaviour
{
    [SerializeField]
    private Tilemap tilemap;

    [SerializeField]
    [Range(0f, 1.0f)]
    private float shadeAdjustment;

    private new Camera camera;

    private void OnEnable()
    {
        SetBackgroundColor();
    }

    private void OnValidate()
    {
        SetBackgroundColor();
    }

    private void SetBackgroundColor()
    {
        if (camera == null) camera = GetComponent<Camera>();

        Color newTintedColor = tilemap.color * new Color(shadeAdjustment, shadeAdjustment, shadeAdjustment, tilemap.color.a);

        camera.backgroundColor = newTintedColor;
    }
}
