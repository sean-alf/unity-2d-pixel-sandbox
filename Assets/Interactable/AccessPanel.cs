using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;

[RequireComponent(typeof(SpriteResolver))]
public class AccessPanel : MonoBehaviour
{
    [SerializeField]
    private List<Door> doors;

    private SpriteResolver sr;
    private bool activated = false;

    private void Awake()
    {
        sr = GetComponent<SpriteResolver>();

        sr.SetCategoryAndLabel("Button", "Unpressed");
    }

    public void Activate()
    {
        if (activated) return;

        foreach (var d in doors)
        {
            d.Toggle();
        }

        sr.SetCategoryAndLabel("Button", "Pressed");
        sr.ResolveSpriteToSpriteRenderer();

        activated = true;
    }
}
