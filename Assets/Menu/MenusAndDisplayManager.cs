using UnityEngine;

public class MenusAndDisplayManager : MonoBehaviour
{
    [SerializeField]
    private TopLeftContainer topLeftContainer;

    public TopLeftContainer GetTopLeftContainer() => topLeftContainer;
}
