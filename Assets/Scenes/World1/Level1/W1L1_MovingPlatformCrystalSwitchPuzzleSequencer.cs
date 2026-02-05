using UnityEngine;

[RequireComponent(typeof(GamePauser))]
public class W1L1_MovingPlatformCrystalSwitchPuzzleSequencer : MonoBehaviour, BetterInputManager.IInputChangeRequestor
{
    [SerializeField][Range(0, 100)] private int inputMapSwitchingPriority = 80;
    [SerializeField] private BetterInputManager inputManager;
    [SerializeField] private float maxCameraSwitchSpeed = 20f;
    [SerializeField] private float dramaticPause = 0.5f;
    [SerializeField] private CrystalSwitch cs1;
    [SerializeField] private CrystalSwitch cs2;
    [SerializeField] private CrystalSwitch cs3;
    [SerializeField] private RemovableBarrier barrier;
    [SerializeField] private TargetFollowerCameraRequestEvent targetFollowerCameraRequestEvent;

    private GamePauser gamePauser;
    private int currentSwitched = 0;

    private static readonly int MaxSwitches = 3;

    public int Priority => inputMapSwitchingPriority;

    public string Name => $"{name} ({GetType().Name})";

    public BetterInputManager.InputType InputType => BetterInputManager.InputType.None;

    private void Awake()
    {
        gamePauser = GetComponent<GamePauser>();
        cs1.SetState(CrystalSwitch.State.A);
        cs2.SetState(CrystalSwitch.State.A);
        cs3.SetState(CrystalSwitch.State.A);
    }

    private void OnEnable()
    {
        cs1.onStateChange.AddListener(OnCrystalStateChange);
        cs2.onStateChange.AddListener(OnCrystalStateChange);
        cs3.onStateChange.AddListener(OnCrystalStateChange);
        barrier.onRemoved.AddListener(OnBarrierRemoved);
    }

    private void OnDisable()
    {
        cs1.onStateChange.RemoveAllListeners();
        cs2.onStateChange.RemoveAllListeners();
        cs3.onStateChange.RemoveAllListeners();
        barrier.onRemoved.RemoveListener(OnBarrierRemoved);
    }

    public void UnityEvent_OnCameraTargetCentered(Transform target)
    {
        if (target == barrier.transform)
        {
            StartCoroutine(SequencingUtilities.DelayRealtime(dramaticPause, () => barrier.Remove()));
        }
        else
        {
            PuzzleFinished();
        }
    }

    private void OnCrystalStateChange(CrystalSwitch.State state)
    {
        if (state == CrystalSwitch.State.A) --currentSwitched;
        if (state == CrystalSwitch.State.B) ++currentSwitched;

        if (currentSwitched == MaxSwitches)
        {
            gamePauser.PauseGame();
            inputManager.AddInputChangeRequest(this);

            StartCoroutine(SequencingUtilities.DelayRealtime(dramaticPause, () =>
                targetFollowerCameraRequestEvent.Raise(new(
                    TargetFollowerCamera.Command.SetTemporaryTarget,
                    barrier.transform,
                    maxCameraSwitchSpeed
                ))
            ));
        }
    }

    private void OnBarrierRemoved() => StartCoroutine(SequencingUtilities.DelayRealtime(dramaticPause, onRun: () =>
        targetFollowerCameraRequestEvent.Raise(new(
            TargetFollowerCamera.Command.SwitchBackToMainTarget,
            null,
            maxCameraSwitchSpeed
        ))
    ));

    private void PuzzleFinished()
    {
        inputManager.RemoveInputChangeRequest(this);
        gamePauser.UnpauseGame();
        cs1.Lock();
        cs2.Lock();
        cs3.Lock();
    }
}
