using UnityEngine;

public class SwitchTester : MonoBehaviour
{
    [SerializeField]
    private Switch switchUnderTest;

    public void UnityEvent_OnToggle()
    {
        Debug.Log("UnityEvent_OnToggle");
    }

    public void UnityEvent_SwitchBackImmediately()
    {
        Debug.Log("UnityEvent_SwitchBackImmediately");
        switchUnderTest.Toggle();
    }
}
