using UnityEngine;

public class Readable : MonoBehaviour, BetterInputManager.IInputChangeRequestor
{
    [SerializeField] private string message;
    [SerializeField] private string senderName;
    [SerializeField] private int inputMapChangePriority = 90;

    [Space]
    [Header("Debug")]

    [SerializeField] private DialogManager dialogManager;

    public int Priority => inputMapChangePriority;
    public string Name => $"{name} ({GetType().Name})";
    public BetterInputManager.InputType InputType => BetterInputManager.InputType.UI;

    private BetterInputManager inputManager;

    private void Awake()
    {
        if (senderName == null || senderName.Length == 0)
        {
            senderName = gameObject.name;
        }
    }

    private void Start()
    {
        dialogManager = FindFirstObjectByType<DialogManager>(FindObjectsInactive.Include);

        if (dialogManager == null)
        {
            Debug.LogError($"{name} ({GetType().Name}): DialogManager not found!");
        }
    }

    public void ReadMessage(BetterInputManager inputManager)
    {
        this.inputManager = inputManager;
        inputManager.AddInputChangeRequest(this);
        dialogManager.ShowMessage(message);
    }

    public void HideMessage()
    {
        dialogManager.Hide(onDone: () =>
        {
            inputManager.RemoveInputChangeRequest(this);
            inputManager = null;
        });
    }
}
