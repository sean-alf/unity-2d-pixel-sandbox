using UnityEngine;

[RequireComponent(typeof(GamePauser))]
public class W1L1_MovingPlatformCrystalSwitchPuzzleSequencer : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private TargetFollowingCamera followerCamera;
    [SerializeField] private float maxCameraSwitchSpeed = 20f;
    [SerializeField] private float dramaticPause = 0.5f;
    [SerializeField] private CrystalSwitch cs1;
    [SerializeField] private CrystalSwitch cs2;
    [SerializeField] private CrystalSwitch cs3;
    [SerializeField] private RemovableBarrier barrier;

    private GamePauser gamePauser;
    private int currentSwitched = 0;

    private static readonly int MaxSwitches = 3;

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

    private void OnCrystalStateChange(CrystalSwitch.State state)
    {
        if (state == CrystalSwitch.State.A) --currentSwitched;
        if (state == CrystalSwitch.State.B) ++currentSwitched;

        if (currentSwitched == MaxSwitches)
        {
            gamePauser.PauseGame();
            playerController.UpdateInputType(BetterInputManager.InputType.None);

            StartCoroutine(SequencingUtilities.DelayRealtime(dramaticPause, () =>
                followerCamera.SetNewTarget(barrier.transform, maxCameraSwitchSpeed, onCentered: () =>
                    StartCoroutine(SequencingUtilities.DelayRealtime(dramaticPause, () => barrier.Remove()))
                )
            ));
        }
    }

    private void OnBarrierRemoved() => StartCoroutine(SequencingUtilities.DelayRealtime(dramaticPause, onRun: () =>
        followerCamera.SetOriginalTarget(maxCameraSwitchSpeed, onCentered: () => PuzzleFinished())
    ));

    private void PuzzleFinished()
    {
        playerController.UpdateInputType(BetterInputManager.InputType.Aiming);
        gamePauser.UnpauseGame();
        cs1.Lock();
        cs2.Lock();
        cs3.Lock();
    }
}
