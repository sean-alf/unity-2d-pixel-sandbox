using System;
using System.Collections.Generic;
using UnityEngine;

public class W1L1_WallCannonRedirectionPanelPuzzleSequencer : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private BasicTrigger gameStartTrigger;
    [SerializeField] private WallCannon wallCannon;
    [SerializeField] private Switch wallCannonSwitch;
    [SerializeField] private EnemyDamageHandler finalShockOrbDamageHandler;
    [SerializeField] private float initialGameStartDelay = 0.5f;
    [SerializeField] private float wallCannonShotDelay = 0.25f;
    [SerializeField] private PanelSwitchCombo[] panelSwitchCombos;

    private ProjectileManager wallCannonProjectileManager;
    private CameraTarget cameraTarget;
    private Switch.SwitchEvent eventA = new()
    {
        forPosition = Switch.Position.A
    };
    private Switch.SwitchEvent eventB = new()
    {
        forPosition = Switch.Position.B
    };

    private readonly Dictionary<PanelSwitchCombo, Switch.SwitchEvent> aEvents = new();
    private readonly Dictionary<PanelSwitchCombo, Switch.SwitchEvent> bEvents = new();

    private bool isFinished = false;

    private void Awake()
    {
        cameraTarget = wallCannon.GetComponent<CameraTarget>();
        wallCannonProjectileManager = wallCannon.GetComponent<ProjectileManager>();
    }

    private void OnEnable()
    {
        gameStartTrigger.onTriggerEnter += OnPuzzleStart;
        cameraTarget.onSwitchedBack += CameraBackOnPlayer;
        wallCannonProjectileManager.onProjectileInstantiated += OnCannonBallShot;
        wallCannonSwitch.onToggleImmediateEvent += OnPlayerInteractedWithSwitch;
        finalShockOrbDamageHandler.onDeath.AddListener(OnFinalShockOrbDeath);

        eventA.whenSelected += WhenSwitchedToA;
        wallCannonSwitch.AddEvent(eventA);
        eventB.whenSelected += WhenSwitchedToB;
        wallCannonSwitch.AddEvent(eventB);

        AddSwitchEventsForPositionA();
        AddSwitchEventsForPositionB();
    }

    private void AddSwitchEventsForPositionA()
    {
        foreach (var combo in panelSwitchCombos)
        {
            Switch.SwitchEvent e;

            if (aEvents.ContainsKey(combo))
            {
                e = aEvents[combo];
            }
            else
            {
                e = new Switch.SwitchEvent() { forPosition = Switch.Position.A };
                aEvents.Add(combo, e);
            }

            e.whenSelected += () => OnPlayerHitPanelSwitch(combo.panelSwitch, combo.panel, combo.onDuration);
            combo.panelSwitch.AddEvent(e);
        }
    }

    private void AddSwitchEventsForPositionB()
    {
        foreach (var combo in panelSwitchCombos)
        {
            Switch.SwitchEvent e;

            if (bEvents.ContainsKey(combo))
            {
                e = bEvents[combo];
            }
            else
            {
                e = new Switch.SwitchEvent() { forPosition = Switch.Position.B };
                bEvents.Add(combo, e);
            }

            e.whenSelected += () => combo.panel.DeactivatePanel();
            combo.panelSwitch.AddEvent(e);
        }
    }

    private void OnDisable()
    {
        gameStartTrigger.onTriggerEnter -= OnPuzzleStart;
        cameraTarget.onSwitchedBack -= CameraBackOnPlayer;
        wallCannonProjectileManager.onProjectileInstantiated -= OnCannonBallShot;
        wallCannonSwitch.onToggleImmediateEvent -= OnPlayerInteractedWithSwitch;
        finalShockOrbDamageHandler.onDeath.RemoveListener(OnFinalShockOrbDeath);
        wallCannonSwitch.ClearEvents();

        foreach (var combo in panelSwitchCombos)
        {
            combo.panelSwitch.ClearEvents();
        }
    }

    private void OnPuzzleStart()
    {
        playerController.DisableInput();
        StartCoroutine(SequencingUtilities.Delay(initialGameStartDelay, onRun: () =>
        {
            wallCannon.ShootOnce(wallCannonShotDelay);
            wallCannonSwitch.Unlock();
        }));
    }

    private void OnCannonBallShot(Transform t)
    {
        var d = t.GetComponent<Destroyable>();
        d.onDestroyed += OnCannonBallDestroyed;
        cameraTarget.SwitchTo(t);
    }

    private void OnCannonBallDestroyed()
    {
        cameraTarget.SwitchBack();
    }

    private void CameraBackOnPlayer()
    {
        wallCannonSwitch.Toggle();
    }

    private void OnPlayerInteractedWithSwitch()
    {
        playerController.DisableInput();
    }

    private void WhenSwitchedToA()
    {
        if (isFinished) wallCannonSwitch.Lock();
        playerController.EnableInput();
    }

    private void WhenSwitchedToB()
    {
        wallCannon.ShootOnce(wallCannonShotDelay);
    }

    private void OnPlayerHitPanelSwitch(Switch s, RedirectionPanel p, float onDuration)
    {
        p.ActivatePanel();
        StartCoroutine(SequencingUtilities.Delay(onDuration, onRun: () => s.Toggle()));
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
