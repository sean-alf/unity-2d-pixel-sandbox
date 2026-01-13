using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Ensure this script is always last to run via Execution Order.
/// </summary>
public class Synchronizer : MonoBehaviour
{
    [SerializeField]
    [Tooltip("How much extra time to wait after all scripts have finished calling Start?")]
    private float startDelay = 0.5f;

    private bool ready = false;
    private WaitForSeconds waitSeconds;

    private void Awake()
    {
        waitSeconds = new(startDelay);
    }

    private void OnValidate()
    {
        waitSeconds = new(startDelay);
    }

    // Update is called once per frame
    void Update()
    {
        ready = true;
    }

    public void WaitForSync(Action onDone)
    {
        StartCoroutine(Run(onDone));
    }

    private IEnumerator Run(Action onDone)
    {
        yield return new WaitUntil(() => ready);
        yield return waitSeconds;
        onDone();
        enabled = false;
    }
}
