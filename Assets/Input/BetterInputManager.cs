using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class BetterInputManager : MonoBehaviour
{
    public enum InputType
    {
        Full,
        Aiming,
        None,
    }

    public interface IInputChangeRequestor
    {
        public int Priority { get; }
        public string Name { get; }
        public InputType InputType { get; }
    }

    [SerializeField] private UnityEvent<InputAction.CallbackContext> onAim;
    [SerializeField] private UnityEvent<InputAction.CallbackContext> onAttack;
    [SerializeField] private UnityEvent<InputAction.CallbackContext> onInteract;
    [SerializeField] private UnityEvent<InputAction.CallbackContext> onMove;
    [SerializeField] private UnityEvent<InputAction.CallbackContext> onNext;
    [SerializeField] private UnityEvent<InputAction.CallbackContext> onPrevious;
    [SerializeField] private UnityEvent<InputAction.CallbackContext> onShoot;

    [Space]
    [Header("Debug")]

    [SerializeField] private InputType currentInputType = InputType.Full;
    [SerializeField] private string topRequestorName;

    public InputType Type => currentInputType;

    private PlayerInput input;
    private InputAction aimAction;
    private InputAction attackAction;
    private InputAction interactAction;
    private InputAction moveAction;
    private InputAction nextAction;
    private InputAction previousAction;
    private InputAction shootAction;

    private readonly List<IInputChangeRequestor> inputChangeRequestors = new();

    private void Awake()
    {
        input = GetComponent<PlayerInput>();
        aimAction = InputSystemActionsNames.PlayerMap.GetAimAction(input);
        attackAction = InputSystemActionsNames.PlayerMap.GetAttackAction(input);
        interactAction = InputSystemActionsNames.PlayerMap.GetInteractAction(input);
        moveAction = InputSystemActionsNames.PlayerMap.GetMoveAction(input);
        nextAction = InputSystemActionsNames.PlayerMap.GetNextAction(input);
        previousAction = InputSystemActionsNames.PlayerMap.GetPreviousAction(input);
        shootAction = InputSystemActionsNames.PlayerMap.GetShootAction(input);
    }

    private void OnEnable()
    {
        SetInputToType(currentInputType);
    }

    private void OnDisable()
    {
        // Clear all registered callbacks, just in case
        SetNoInput();
    }

    /// <summary>
    /// Add an input change request.
    /// If this request is the highest priority, then its input type will be immediately switched to.
    /// Otherwise, this request must wait until all higher priority requests are removed.
    /// This request will be ignored if it removes itself before this happens.
    /// </summary>
    /// <param name="req"></param>
    public void AddInputChangeRequest(IInputChangeRequestor req)
    {
        if (inputChangeRequestors.Contains(req)) return;
        inputChangeRequestors.Add(req);
        inputChangeRequestors.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        UpdateInputToType();
    }

    /// <summary>
    /// Remove the requestor, if it exists, from the list.
    /// The next highest priority requestor will then get its input type set if it
    /// differs from the current.
    /// If there are no requestors left, then input is switched to the default (currently Full).
    /// </summary>
    /// <param name="req"></param>
    public void RemoveInputChangeRequest(IInputChangeRequestor req)
    {
        if (!inputChangeRequestors.Contains(req)) return;
        inputChangeRequestors.Remove(req);
        UpdateInputToType();
    }

    /// <summary>
    /// This should be called if the priority or InputType has changed.
    /// If the input type has changed, and the requestor is the highest priority, its input type
    /// will be immediately switched to.
    /// </summary>
    /// <param name="req"></param>
    public void UpdateInputChangeRequest(IInputChangeRequestor req)
    {
        if (!inputChangeRequestors.Contains(req)) return;
        inputChangeRequestors.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        UpdateInputToType();
    }

    private void UpdateInputToType()
    {
        var topRequestor = inputChangeRequestors.FirstOrDefault();
        var type = topRequestor != null ? topRequestor.InputType : InputType.Full;

        if (type == currentInputType) return;

        currentInputType = type;
        SetInputToType(currentInputType);
    }

    private void SetInputToType(InputType type)
    {
        switch (type)
        {
            case InputType.Full:
                SetFullInput();
                break;
            case InputType.Aiming:
                SetAimingInput();
                break;
            case InputType.None:
                SetNoInput();
                break;
        }
    }

    private void SetFullInput()
    {
        aimAction.performed += OnAim;
        aimAction.canceled += OnAim;
        attackAction.performed += OnAttack;
        interactAction.performed += OnInteract;
        moveAction.performed += OnMove;
        moveAction.canceled += OnMove;
        nextAction.performed += OnNext;
        previousAction.performed += OnPrevious;
        shootAction.performed += OnShoot;
    }

    private void SetAimingInput()
    {
        aimAction.performed += OnAim;
        aimAction.canceled += OnAim;
        attackAction.performed -= OnAttack;
        interactAction.performed -= OnInteract;
        moveAction.performed -= OnMove;
        moveAction.canceled -= OnMove;
        nextAction.performed += OnNext;
        previousAction.performed += OnPrevious;
        shootAction.performed += OnShoot;
    }

    private void SetNoInput()
    {
        aimAction.performed -= OnAim;
        aimAction.canceled -= OnAim;
        attackAction.performed -= OnAttack;
        interactAction.performed -= OnInteract;
        moveAction.performed -= OnMove;
        moveAction.canceled -= OnMove;
        nextAction.performed -= OnNext;
        previousAction.performed -= OnPrevious;
        shootAction.performed -= OnShoot;
    }

    private void OnAim(InputAction.CallbackContext context) => onAim?.Invoke(context);
    private void OnAttack(InputAction.CallbackContext context) => onAttack?.Invoke(context);
    private void OnInteract(InputAction.CallbackContext context) => onInteract?.Invoke(context);
    private void OnMove(InputAction.CallbackContext context) => onMove?.Invoke(context);
    private void OnNext(InputAction.CallbackContext context) => onNext?.Invoke(context);
    private void OnPrevious(InputAction.CallbackContext context) => onPrevious?.Invoke(context);
    private void OnShoot(InputAction.CallbackContext context) => onShoot?.Invoke(context);
}
