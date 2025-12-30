using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AccessPanel : MonoBehaviour
{
    [SerializeField]
    private List<Door> doors;

    private Animator animator;
    private bool activated = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Activate()
    {
        if (activated) return;

        foreach (var d in doors)
        {
            d.Toggle();
        }

        animator.Play(AccessPanelAnimatorStates.BaseLayer.ACCESS_PANEL_ACTIVATED);

        activated = true;
    }
}
