using System.Linq;
using SuperTiled2Unity;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class CameraBackgroundColorManager : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField][Range(0f, 1.0f)] private float shadeAdjustment;

    [Space]
    [Header("Debug")]

    [SerializeField] string tileLayerName;
    [SerializeField] private Color preShadedColor;
    [SerializeField] private Color postShadedColor;

    private SuperTileLayer tileLayer;

    private void OnEnable()
    {
        SetBackgroundColor();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            SetBackgroundColor();
        }
        else
        {
            EditorApplication.delayCall += () =>
            {
                if (this == null) return;
                SetBackgroundColor();
            };
        }
    }
#endif

    private void SetBackgroundColor()
    {
        var tileLayers = FindObjectsByType<SuperTileLayer>(FindObjectsSortMode.None);
        tileLayer = tileLayers.FirstOrDefault(l => l.m_TiledName == "Wall");
        tileLayerName = tileLayer != null ? tileLayer.m_TiledName : "[Not Set]";

        if (tileLayer)
        {
            preShadedColor = tileLayer.CalculateColor();
            postShadedColor = preShadedColor * new Color(shadeAdjustment, shadeAdjustment, shadeAdjustment, preShadedColor.a);
            cam.backgroundColor = postShadedColor;
        }
    }
}
