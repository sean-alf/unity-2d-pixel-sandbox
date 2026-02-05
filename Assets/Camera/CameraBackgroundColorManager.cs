using System.Linq;
using SuperTiled2Unity;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class CameraBackgroundColorManager : MonoBehaviour
{
    [SerializeField] private SuperTileLayer tileLayer;
    [SerializeField][Range(0f, 1.0f)] private float shadeAdjustment;

    [Space]
    [Header("Debug")]

    [SerializeField] private Color preShadedColor;
    [SerializeField] private Color postShadedColor;

    void Awake()
    {
        var tileLayers = FindObjectsByType<SuperTileLayer>(FindObjectsSortMode.None);
        tileLayer = tileLayers.First(l => l.gameObject.name == "Wall");
        SetBackgroundColor();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        SetBackgroundColor();
    }
#endif

    private void SetBackgroundColor()
    {
        if (tileLayer == null)
        {
            return;
        }

        var camera = GetComponent<Camera>();
        preShadedColor = tileLayer.CalculateColor();
        postShadedColor = preShadedColor * new Color(shadeAdjustment, shadeAdjustment, shadeAdjustment, preShadedColor.a);
        camera.backgroundColor = postShadedColor;
    }
}
