using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class CanvasToCameraResolutionMatcher : MonoBehaviour
{
    [SerializeField]
    private PixelPerfectCamera ppCamera;

    private CanvasScaler scaler;

    void OnEnable()
    {   
        Scale();
    }

    void Awake()
    {
        Scale();
    }

    void OnValidate()
    {
        Scale();
    }

    private void Scale()
    {
        if (ppCamera != null)
        {
            scaler = GetComponent<CanvasScaler>();
            scaler.referenceResolution = new(ppCamera.refResolutionX, ppCamera.refResolutionY);
        }
    }
}
