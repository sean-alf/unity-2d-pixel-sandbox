using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Animator))]
public class Player2Movement : MonoBehaviour
{
    [SerializeField]
    [Range(10, 200)]
    private float speed = 10;

    [Header("Debug")]
    [SerializeField]
    private bool enableLogs = false;

    [SerializeField]
    private List<Door> doors;

    private static readonly ILogger logger = Debug.unityLogger;

    private string Tag => $"Player2Movement:{name}";

    private Rigidbody2D rb;
    private PlayerInput input;
    private Animator animator;
    private Vector2 currentDirection;
    private Vector2 lastNonIdleDirection;
    private bool actionButtonPressed = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        input = GetComponent<PlayerInput>();
        animator = GetComponent<Animator>();

        input.onActionTriggered += OnInput;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!currentDirection.IsIdle())
        {
            Vector2 movementOffset = Time.fixedDeltaTime * speed * currentDirection;
            rb.MovePosition(movementOffset + rb.position);
        }
    }

    private void OnInput(InputAction.CallbackContext context)
    {
        if (context.action.name == "Look") return;

        switch (context.action.name)
        {
            case "Move":
                {
                    currentDirection = context.ReadValue<Vector2>().NormalizeAndRound();

                    if (currentDirection.IsIdle())
                    {
                        animator.Play("Player2Idle");
                    }
                    else
                    {
                        // This is for determining which way the player is facing even when stopped
                        lastNonIdleDirection = currentDirection;
                        rb.SetRotation(Quaternion.LookRotation(Vector3.forward, currentDirection));
                        animator.Play("Player2Move");
                    }

                    if (enableLogs)
                    {
                        logger.Log(Tag, $"Move input = {currentDirection}");
                    }
                    break;
                }
            case "Jump":
                {
                    actionButtonPressed = context.ReadValueAsButton();

                    if (actionButtonPressed)
                    {
                        foreach (var door in doors)
                        {
                            door.Toggle();
                        }
                    }

                    if (enableLogs)
                    {
                        logger.Log(Tag, $"[PlayerMovement] Jump input = {actionButtonPressed}");
                    }
                    break;
                }
        }
    }

    void OnDestroy()
    {
        input.onActionTriggered -= OnInput;
    }

}
