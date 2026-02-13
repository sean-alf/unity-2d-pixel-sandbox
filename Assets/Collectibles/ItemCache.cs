using System;
using System.Collections;
using System.Security.Cryptography;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ItemCache : MonoBehaviour, BetterInputManager.IInputChangeRequestor
{
    private const string NotFacingCorrectlyMessage = "You can't access this from your current position ...";

    [SerializeField] ScriptableObject collectibleSO;
    [SerializeField] Sprite collectedSprite;
    [SerializeField] GameObject collectibleIconTemplate;
    [SerializeField] float fadeDuration = 0.25f;
    [SerializeField] float showItemDuration = 1f;
    [SerializeField] float levitationSpeed = 1f;
    [SerializeField] float afterLevitateDelay = 0.5f;

    [Space]
    [Header("Debug")]
    [SerializeField] private bool isCollected = false;
    [SerializeField][TextArea] private string message;

    public bool IsCollected => isCollected;
    public int Priority => 90;
    public string Name => $"{name} ({GetType().Name})";

    public BetterInputManager.InputType InputType => BetterInputManager.InputType.UI;

    private ICollectible collectible;
    private SpriteRenderer sr;
    private DialogManager dialogManager;
    private SpriteRenderer iconSR;
    private BetterInputManager inputManager;

    private void Awake()
    {
        collectible = collectibleSO as ICollectible;
        sr = GetComponent<SpriteRenderer>();

        if (collectible == null)
        {
            Debug.LogError($"{name} ({GetType().Name}): collectibleSO {collectibleSO.name} is not an ICollectible!");
        }
    }

    private void Start()
    {
        dialogManager = FindFirstObjectByType<DialogManager>(FindObjectsInactive.Include);
    }

    public bool Collect(GameObject collector)
    {
        if (isCollected) return false;

        inputManager = collector.GetComponent<BetterInputManager>();
        Vector2 direction = collector.transform.rotation * Vector2.up;

        if (direction != Vector2.up)
        {
            inputManager.AddInputChangeRequest(this);
            dialogManager.ShowMessage(NotFacingCorrectlyMessage);
            return false;
        }

        isCollected = true;
        sr.sprite = collectedSprite;
        inputManager.AddInputChangeRequest(this);

        var startingPosition = transform.position.Add(new Vector2(0, 0.5f));
        var endingPosition = startingPosition.Add(0f, 1f);
        var icon = Instantiate(collectibleIconTemplate, startingPosition, Quaternion.identity);
        iconSR = icon.GetComponent<SpriteRenderer>();

        iconSR.color = iconSR.color.WithAlpha(0f);
        iconSR.sprite = collectible.SmallIcon;

        message = $"You received the\n{collectible.DisplayName}\n\n<sprite name=\"{collectible.LargeIcon.name}\">\n";
        message += $"<page>{collectible.Description}";

        this.FadeIn(
            iconSR,
            totalDuration: fadeDuration,
            onDone: () => StartCoroutine(Levitate(
                target: icon,
                startingPosition,
                endingPosition,
                onDone: () => HandlePostLevitation())
            )
        );

        collectible.Collect(collector);
        return true;
    }

    public void Acknowledge(Action onDone) => dialogManager.ShowNextMessagePageOrClose(onClosed: () => OnDialogClosed(onDone));

    private void HandlePostLevitation()
    {
        this.StartTimer(showItemDuration, onExpired: () =>
        {
            this.FadeOut(
                iconSR,
                totalDuration: fadeDuration,
                onDone: () =>
                {
                    Destroy(iconSR.gameObject);
                    dialogManager.ShowMessage(
                        message,
                        alignment: TMPro.TextAlignmentOptions.Top
                    );
                }
            );
        });
    }

    private void OnDialogClosed(Action onDone)
    {
        inputManager.RemoveInputChangeRequest(this);

        if (isCollected)
        {
            onDone();
            // Destroy this script to allow GC of any related memory, but leave the rest of the GO intact
            Destroy(this);
        }

    }

    private IEnumerator Levitate(GameObject target, Vector2 startingPosition, Vector2 endingPosition, Action onDone)
    {
        Vector2 position;
        float time = 0f;

        do
        {
            time += Time.deltaTime * levitationSpeed;
            position = Vector2.Lerp(startingPosition, endingPosition, time);
            target.transform.position = position;
            yield return null;

        } while (time < 1f);

        target.transform.position = endingPosition;

        yield return new WaitForSeconds(afterLevitateDelay);

        onDone();
    }

    public interface ICollectible
    {
        public Sprite LargeIcon { get; }
        public Sprite SmallIcon { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public void Collect(GameObject collector);
    }
}
