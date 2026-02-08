using System;
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
        UI,
    }

    public interface IInputChangeRequestor
    {
        public int Priority { get; }
        public string Name { get; }
        public InputType InputType { get; }
    }

    [Serializable]
    public struct InputChangeRequestor
    {
        public int priority;
        public string name;
        public InputType inputType;
    }

    [SerializeField] private UnityEvent<InputType> onInputTypeChange;

    [Header("Player Input")]

    [SerializeField] private UnityEvent<InputAction.CallbackContext> onAim;
    [SerializeField] private UnityEvent<InputAction.CallbackContext> onAimEnable;
    [SerializeField] private UnityEvent<InputAction.CallbackContext> onAttack;
    [SerializeField] private UnityEvent<InputAction.CallbackContext> onInteract;
    [SerializeField] private UnityEvent<InputAction.CallbackContext> onMove;
    [SerializeField] private UnityEvent<InputAction.CallbackContext> onNext;
    [SerializeField] private UnityEvent<InputAction.CallbackContext> onPrevious;
    [SerializeField] private UnityEvent<InputAction.CallbackContext> onShoot;

    [Header("UI Input")]

    [SerializeField] private UnityEvent<InputAction.CallbackContext> onSubmit;

    [Space]
    [Header("Debug")]

    [SerializeField] private InputType currentInputType = InputType.Full;
    [SerializeField] private string topRequestorName;
    [SerializeField] private List<InputChangeRequestor> requestors;

    public PlayerInput PlayerInput => input;
    public InputAction AimAction => aimAction;
    public InputAction MoveAction => moveAction;
    public InputAction ModifyAction => modifyAction;

    private PlayerInput input;
    private InputAction aimAction;
    private InputAction attackAction;
    private InputAction interactAction;
    private InputAction moveAction;
    private InputAction nextAction;
    private InputAction previousAction;
    private InputAction shootAction;
    private InputAction modifyAction;
    private InputAction submitAction;

    private readonly List<IInputChangeRequestor> inputChangeRequestors = new();

    private void Awake()
    {
        // Debug.Log($"{name} ({GetType().Name}): Awake");

        input = GetComponent<PlayerInput>();

        // Player Input Actions
        aimAction = InputSystemActionsNames.PlayerMap.GetAimAction(input);
        attackAction = InputSystemActionsNames.PlayerMap.GetAttackAction(input);
        interactAction = InputSystemActionsNames.PlayerMap.GetInteractAction(input);
        moveAction = InputSystemActionsNames.PlayerMap.GetMoveAction(input);
        nextAction = InputSystemActionsNames.PlayerMap.GetNextAction(input);
        previousAction = InputSystemActionsNames.PlayerMap.GetPreviousAction(input);
        shootAction = InputSystemActionsNames.PlayerMap.GetShootAction(input);
        modifyAction = InputSystemActionsNames.PlayerMap.GetModifyAction(input);

        // UI Input Actions
        submitAction = InputSystemActionsNames.UIMap.GetSubmitAction(input);

        input.SwitchCurrentActionMap(InputSystemActionsNames.PlayerMap.Name);
    }

    private void OnEnable()
    {
        // Debug.Log($"{name} ({GetType().Name}): OnEnable");

        SetInputToType(currentInputType);

        // No need to register/unregister based on input type
        // Because it is only used with the UI action map
        submitAction.performed += OnSubmit;
    }

    private void OnDisable()
    {
        // Debug.Log($"{name} ({GetType().Name}): OnDisable");

        // Clear all registered callbacks, just in case
        UnregisterAllCallbacks();
        submitAction.performed -= OnSubmit;
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

        // For debug purposes
        requestors = inputChangeRequestors.Select(i => new InputChangeRequestor()
        {
            priority = i.Priority,
            name = i.Name,
            inputType = i.InputType,
        }).ToList();

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

        // For debug purposes
        requestors = inputChangeRequestors.Select(i => new InputChangeRequestor()
        {
            priority = i.Priority,
            name = i.Name,
            inputType = i.InputType,
        }).ToList();

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

        // For debug purposes
        requestors = inputChangeRequestors.Select(i => new InputChangeRequestor()
        {
            priority = i.Priority,
            name = i.Name,
            inputType = i.InputType,
        }).ToList();

        UpdateInputToType();
    }

    private void UpdateInputToType()
    {
        var topRequestor = inputChangeRequestors.FirstOrDefault();
        var type = topRequestor != null ? topRequestor.InputType : InputType.Full;

        topRequestorName = topRequestor != null ? topRequestor.Name : "None";

        if (type == currentInputType) return;

        currentInputType = type;
        SetInputToType(currentInputType);
        onInputTypeChange?.Invoke(currentInputType);
    }

    private void SetInputToType(InputType type)
    {
        // Debug.Log($"{name} ({GetType().Name}): SetInputTypeTo: type {type}");

        // Default to player action map
        string mapName = InputSystemActionsNames.PlayerMap.Name;

        // Clear any current callbacks so that we don't have any duplication
        UnregisterAllCallbacks();

        switch (type)
        {
            case InputType.Full:
                SetFullInput();
                break;
            case InputType.Aiming:
                SetAimingInput();
                break;
            case InputType.UI:
                mapName = InputSystemActionsNames.UIMap.Name;
                break;
            case InputType.None:
                // We already unregistered all callbacks, so do nothing
                break;
        }

        SwitchToMap(mapName);
    }

    private void SwitchToMap(string name)
    {
        if (input.currentActionMap.name == name) return;
        input.SwitchCurrentActionMap(name);
    }

    private void SetFullInput()
    {
        aimAction.performed += OnAimEnable;
        aimAction.canceled += OnAimEnable;
        attackAction.performed += OnAttack;
        interactAction.performed += OnInteract;
        moveAction.performed += OnMove;
        moveAction.canceled += OnMove;
        moveAction.performed -= OnAim;
        moveAction.canceled -= OnAim;
        nextAction.performed += OnNext;
        previousAction.performed += OnPrevious;
        shootAction.performed += OnShoot;
    }

    private void SetAimingInput()
    {
        aimAction.performed += OnAimEnable;
        aimAction.canceled += OnAimEnable;
        attackAction.performed -= OnAttack;
        interactAction.performed -= OnInteract;
        moveAction.performed -= OnMove;
        moveAction.canceled -= OnMove;
        moveAction.performed += OnAim;
        moveAction.canceled += OnAim;
        nextAction.performed += OnNext;
        previousAction.performed += OnPrevious;
        shootAction.performed += OnShoot;
    }

    private void UnregisterAllCallbacks()
    {
        aimAction.performed -= OnAimEnable;
        aimAction.canceled -= OnAimEnable;
        attackAction.performed -= OnAttack;
        interactAction.performed -= OnInteract;
        moveAction.performed -= OnMove;
        moveAction.canceled -= OnMove;
        moveAction.performed -= OnAim;
        moveAction.canceled -= OnAim;
        nextAction.performed -= OnNext;
        previousAction.performed -= OnPrevious;
        shootAction.performed -= OnShoot;
    }

    // Player Input Actions
    private void OnAim(InputAction.CallbackContext context) => onAim?.Invoke(context);
    private void OnAimEnable(InputAction.CallbackContext context) => onAimEnable?.Invoke(context);
    private void OnAttack(InputAction.CallbackContext context) => onAttack?.Invoke(context);
    private void OnInteract(InputAction.CallbackContext context) => onInteract?.Invoke(context);
    private void OnMove(InputAction.CallbackContext context) => onMove?.Invoke(context);
    private void OnNext(InputAction.CallbackContext context) => onNext?.Invoke(context);
    private void OnPrevious(InputAction.CallbackContext context) => onPrevious?.Invoke(context);
    private void OnShoot(InputAction.CallbackContext context) => onShoot?.Invoke(context);

    // UI Input Actions
    private void OnSubmit(InputAction.CallbackContext context) => onSubmit?.Invoke(context);
}
