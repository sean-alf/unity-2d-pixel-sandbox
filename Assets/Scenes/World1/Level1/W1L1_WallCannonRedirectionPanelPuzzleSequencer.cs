using System;
using System.Linq;
using UnityEngine;

public class W1L1_WallCannonRedirectionPanelPuzzleSequencer : MonoBehaviour, BetterInputManager.IInputChangeRequestor
{
    [SerializeField][Range(0, 100)] private int inputMapSwitchingPriority = 80;
    [SerializeField] private BetterInputManager inputManager;
    [SerializeField] private BasicTrigger gameStartTrigger;
    [SerializeField] private WallCannon wallCannon;
    [SerializeField] private Switch wallCannonSwitch;
    [SerializeField] private EnemyDamageHandler finalShockOrbDamageHandler;
    [SerializeField] private float initialGameStartDelay = 0.5f;
    [SerializeField] private float wallCannonShotDelay = 0.25f;
    [SerializeField] private float maxCameraSwitchSpeed = 20f;
    [SerializeField] private float afterSpinningBladeDestroyedDelay = 0.5f;
    [SerializeField] private PanelSwitchCombo[] panelSwitchCombos;
    [SerializeField] private TargetFollowerCameraRequestEvent targetFollowerCameraRequestEvent;

    private ProjectileManager wallCannonProjectileManager;
    private Transform cannonBallTransform;
    private bool isFinished = false;

    public int Priority => inputMapSwitchingPriority;
    public string Name => $"{name} ({GetType().Name})";
    public BetterInputManager.InputType InputType => BetterInputManager.InputType.None;

    private void Awake()
    {
        wallCannonProjectileManager = wallCannon.GetComponent<ProjectileManager>();
    }

    private void OnEnable()
    {
        gameStartTrigger.onTriggerEnter += OnPuzzleStart;
        wallCannonProjectileManager.onProjectileInstantiated.AddListener(OnCannonBallShot);
        wallCannonSwitch.onToggleImmediateEvent.AddListener(OnPlayerInteractedWithSwitch);
        finalShockOrbDamageHandler.onDeath.AddListener(OnFinalShockOrbDeath);
        wallCannonSwitch.AddListener(Switch.Position.A, WhenWallCannonSwitchSwitchedToA);
        wallCannonSwitch.AddListener(Switch.Position.B, WhenWallCannonSwitchSwitchedToB);
        AddSwitchListeners();
    }

    private void OnDisable()
    {
        gameStartTrigger.onTriggerEnter -= OnPuzzleStart;
        wallCannonProjectileManager.onProjectileInstantiated.RemoveListener(OnCannonBallShot);
        wallCannonSwitch.onToggleImmediateEvent.RemoveAllListeners();
        finalShockOrbDamageHandler.onDeath.RemoveListener(OnFinalShockOrbDeath);
        wallCannonSwitch.RemoveAllListeners();
        RemoveSwitchListeners();
    }

    private void AddSwitchListeners()
    {
        foreach (var combo in panelSwitchCombos)
        {
            combo.panelSwitch.AddStateChangeListener(OnPlayerHitPanelSwitch);
        }
    }

    private void RemoveSwitchListeners()
    {
        foreach (var combo in panelSwitchCombos)
        {
            combo.panelSwitch.RemoveStateChangeListener(OnPlayerHitPanelSwitch);
        }
    }

    private void OnPuzzleStart()
    {
        inputManager.AddInputChangeRequest(this);
        StartCoroutine(SequencingUtilities.Delay(initialGameStartDelay, onRun: () =>
        {
            wallCannon.ShootOnce(wallCannonShotDelay);
            wallCannonSwitch.Unlock();
        }));
    }

    private void OnCannonBallShot(Transform t)
    {
        cannonBallTransform = t;
        var d = t.GetComponent<Destroyable>();
        d.onDestroyed += OnCannonBallDestroyed;
        targetFollowerCameraRequestEvent.Raise(new(
            command: TargetFollowerCamera.Command.SetTemporaryTarget,
            target: t,
            maxSpeed: maxCameraSwitchSpeed
        ));
    }

    private void OnCannonBallDestroyed()
    {
        this.StartTimer(afterSpinningBladeDestroyedDelay, onExpired: () =>
        {
            targetFollowerCameraRequestEvent.Raise(new(
                command: TargetFollowerCamera.Command.SwitchBackToMainTarget,
                target: null,
                maxSpeed: maxCameraSwitchSpeed
            ));
        });
    }

    public void UnityEvent_OnCameraTargetCentered(Transform target)
    {
        // Only toggle if it's the original player target
        if (target != cannonBallTransform) wallCannonSwitch.Toggle();
    }

    private void OnPlayerInteractedWithSwitch()
    {
        inputManager.AddInputChangeRequest(this);
    }

    private void WhenWallCannonSwitchSwitchedToA(Switch s)
    {
        if (isFinished) s.Lock();
        inputManager.RemoveInputChangeRequest(this);
    }

    private void WhenWallCannonSwitchSwitchedToB(Switch _)
    {
        wallCannon.ShootOnce(wallCannonShotDelay);
    }

    private void OnPlayerHitPanelSwitch(Switch s, Switch.Position p)
    {
        var combo = panelSwitchCombos.FirstOrDefault(p => p.panelSwitch == s);

        switch (p)
        {
            case Switch.Position.A:
                combo.panel.ActivatePanel();
                StartCoroutine(SequencingUtilities.Delay(combo.onDuration, onRun: () => combo.panelSwitch.Toggle()));
                break;
            case Switch.Position.B:
                combo.panel.DeactivatePanel();
                break;
        }
    }

    private void OnFinalShockOrbDeath()
    {
        isFinished = true;

        foreach (var combo in panelSwitchCombos)
        {
            combo.panelSwitch.Lock();
        }
    }

    [Serializable]
    public struct PanelSwitchCombo
    {
        public Switch panelSwitch;
        public RedirectionPanel panel;
        public float onDuration;
    }
}
