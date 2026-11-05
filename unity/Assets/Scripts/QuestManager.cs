using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using MuhurluKapi.Dialogue;
using MuhurluKapi.Interaction;

public sealed class QuestManager : MonoBehaviour
{
    [SerializeField] private CrystalDoorPuzzle puzzle;
    [SerializeField] private Transform player;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TMP_FontAsset font;
    [SerializeField] private TextMeshProUGUI dialogueTextTemplate;
    [SerializeField] private TMP_InputField dialogueInputTemplate;
    [SerializeField] private Button dialogueSendTemplate;
    [SerializeField] private OrrinDialogueUI realOrrin;
    [SerializeField] private OrrinSealStone orrinSeal;
    [SerializeField] private AncientDiamond[] diamonds;
    [SerializeField] private float interactionDistance = 3.5f;

    private bool metProjection;
    private int hintLevel;
    private bool heardNyraInstruction;
    private bool enteredSecondRoom;
    private GameObject projectionPanel;
    private TMP_InputField input;
    private Button sendButton;
    private TempleDialogueView dialogueView;
    private Coroutine pendingReply;
    private Canvas canvas;
    private RectTransform panel;
    private TextMeshProUGUI label;
    private Material glow;

    private void Awake()
    {
        if (TryGetComponent(out Renderer renderer) && renderer.sharedMaterial != null)
        {
            glow = new Material(renderer.sharedMaterial);
            glow.color = new Color(0.28f, 0.20f, 0.85f);
            glow.EnableKeyword("_EMISSION");
            glow.SetColor("_EmissionColor", new Color(0.32f, 0.16f, 1.1f));
            renderer.sharedMaterial = glow;
        }

        var hud = new GameObject("Quest HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        canvas = hud.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = hud.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(960, 540);

        var background = new GameObject("Quest Panel", typeof(RectTransform), typeof(Image));
        panel = background.GetComponent<RectTransform>();
        panel.SetParent(hud.transform, false);
        panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(0, 1);
        panel.anchoredPosition = new Vector2(20, -20);
        var image = background.GetComponent<Image>();
        image.color = new Color(0.052f, 0.027f, 0.097f, 0.94f);
        image.raycastTarget = false;

        var accent = new GameObject("Görev • Altın çizgi", typeof(RectTransform), typeof(Image))
            .GetComponent<Image>();
        accent.rectTransform.SetParent(panel, false);
        accent.rectTransform.anchorMin = Vector2.zero;
        accent.rectTransform.anchorMax = Vector2.up;
        accent.rectTransform.offsetMin = Vector2.zero;
        accent.rectTransform.offsetMax = Vector2.right * 3;
        accent.color = new Color(.82f, .65f, .36f);
        accent.raycastTarget = false;

        var heading = new GameObject("Görev başlığı", typeof(RectTransform), typeof(TextMeshProUGUI))
            .GetComponent<TextMeshProUGUI>();
        heading.rectTransform.SetParent(panel, false);
        heading.rectTransform.anchorMin = Vector2.up;
        heading.rectTransform.anchorMax = Vector2.one;
        heading.rectTransform.offsetMin = new Vector2(14, -33);
        heading.rectTransform.offsetMax = new Vector2(-12, -7);
        heading.font = font;
        heading.fontSize = 16;
        heading.color = accent.color;
        heading.text = "Mevcut Görev";
        heading.raycastTarget = false;

        var textObject = new GameObject("Quest Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        label = textObject.GetComponent<TextMeshProUGUI>();
        label.rectTransform.SetParent(panel, false);
        label.rectTransform.anchorMin = Vector2.zero;
        label.rectTransform.anchorMax = Vector2.one;
        label.rectTransform.offsetMin = new Vector2(14, 8);
        label.rectTransform.offsetMax = new Vector2(-12, -34);
        label.font = font;
        label.fontSize = 18;
        label.color = Color.white;
        label.raycastTarget = false;
        panel.sizeDelta = new Vector2(510, 72);

        CreateDialoguePanel();
    }

    private void CreateDialoguePanel()
    {
        if (dialoguePanel == null || dialogueTextTemplate == null ||
            dialogueInputTemplate == null || dialogueSendTemplate == null)
        {
            Debug.LogWarning("Orrin yansıması diyalog şablonları eksik.", this);
            return;
        }

        projectionPanel = new GameObject("Orrin Projection Dialogue", typeof(RectTransform), typeof(Image));
        projectionPanel.SetActive(false);
        var rect = (RectTransform)projectionPanel.transform;
        rect.SetParent(dialoguePanel.transform.parent, false);
        input = Instantiate(dialogueInputTemplate, rect);
        input.lineType = TMP_InputField.LineType.SingleLine;
        input.characterLimit = 400;
        input.onSubmit = new TMP_InputField.SubmitEvent();
        input.onSubmit.AddListener(_ => Send());

        sendButton = Instantiate(dialogueSendTemplate, rect);
        sendButton.onClick = new Button.ButtonClickedEvent();
        sendButton.onClick.AddListener(Send);
        dialogueView = new TempleDialogueView(rect, font, input, sendButton,
            "Orrin'in Yansıması", "İlk salonun rehberi", CloseDialogue);
    }

    public bool IsDialogueOpen => projectionPanel != null && projectionPanel.activeSelf;

    public const int RequiredDiamonds = 3;

    public int CollectedDiamondCount
    {
        get
        {
            int count = 0;
            if (diamonds != null)
                foreach (var diamond in diamonds)
                    if (diamond != null && diamond.IsCollected)
                        count++;
            return count;
        }
    }

    public bool HasRequiredDiamonds => CollectedDiamondCount >= RequiredDiamonds;

    public bool TryCollectDiamond(CharacterController playerBody)
    {
        if (!enteredSecondRoom || realOrrin == null || !realOrrin.IsOrrinConvinced || diamonds == null)
            return false;

        AncientDiamond nearest = null;
        float closest = 1.5f;
        foreach (var diamond in diamonds)
        {
            if (diamond == null || diamond.IsCollected || !diamond.isActiveAndEnabled)
                continue;

            Vector3 position = diamond.transform.position;
            float distance = Vector3.Distance(position, playerBody.ClosestPoint(position));
            if (distance > closest)
                continue;

            nearest = diamond;
            closest = distance;
        }

        if (nearest == null)
            return false;

        nearest.Collect();
        Debug.Log($"Kadim elmas toplandı: {CollectedDiamondCount}/{RequiredDiamonds}", nearest);
        return true;
    }

    public bool TryInteract(Vector3 playerPosition)
    {
        if (projectionPanel == null || puzzle == null || enteredSecondRoom ||
            (dialoguePanel != null && dialoguePanel.activeInHierarchy) ||
            Vector3.Distance(playerPosition, transform.position) > interactionDistance)
            return false;

        if (IsDialogueOpen)
            CloseDialogue();
        else
        {
            projectionPanel.SetActive(true);
            dialogueView.ScrollToLatest();
            input.ActivateInputField();
        }
        return true;
    }

    private void Send()
    {
        if (!IsDialogueOpen || pendingReply != null)
            return;

        string message = input.text.Trim();
        if (message.Length == 0)
            return;

        pendingReply = StartCoroutine(Reply(message));
    }

    private IEnumerator Reply(string message)
    {
        input.interactable = false;
        sendButton.interactable = false;
        dialogueView.Begin(message);
        yield return new WaitForSecondsRealtime(0.45f);

        string answer;
        if (puzzle.IsGateOpen)
            answer = "İlk sınavı geçtin. İkinci salona ilerle; gerçek suretim seni orada bekliyor.";
        else if (puzzle.ActivatedCrystalCount == 3)
        {
            heardNyraInstruction = true;
            answer = "Güzel iş çıkardın. Şimdi Nyra'yı yükselen büyülü mührün üzerine yönlendir. Kapı açıldığında ikinci salonda benimle yeniden buluş.";
        }
        else if (message.IndexOf("ipucu", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                 message.IndexOf("sıra", System.StringComparison.OrdinalIgnoreCase) >= 0)
            answer = puzzle.Hint(hintLevel = Mathf.Min(3, hintLevel + 1));
        else
            answer = "Üç kadim kristali doğru sırayla okla etkinleştir. Üç yazıtı ara. İstersen bana ipucu sor.";

        metProjection = true;
        dialogueView.Complete(answer);
        input.SetTextWithoutNotify("");
        input.interactable = true;
        sendButton.interactable = true;
        pendingReply = null;
    }

    public bool TryCloseDialogue()
    {
        if (!IsDialogueOpen) return false;
        CloseDialogue();
        return true;
    }

    private void CloseDialogue()
    {
        if (pendingReply != null)
        {
            StopCoroutine(pendingReply);
            dialogueView.Cancel();
            pendingReply = null;
        }

        input.DeactivateInputField();
        var eventSystem = EventSystem.current;
        if (eventSystem != null && eventSystem.currentSelectedGameObject != null &&
            eventSystem.currentSelectedGameObject.transform.IsChildOf(projectionPanel.transform))
            eventSystem.SetSelectedGameObject(null);
        projectionPanel.SetActive(false);
        input.interactable = true;
        sendButton.interactable = true;
    }

    private void LateUpdate()
    {
        if (puzzle != null && puzzle.IsGateOpen && player != null)
        {
            Transform gate = puzzle.Gate;
            float nearSide = gate.InverseTransformPoint(transform.position).z;
            float playerSide = gate.InverseTransformPoint(player.position).z;
            if (nearSide * playerSide < 0f)
                enteredSecondRoom = true;
        }

        if (IsDialogueOpen &&
            (enteredSecondRoom || (!TempleMenu.HandlesEscape && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) ||
             (player != null && Vector3.Distance(player.position, transform.position) > interactionDistance)))
            CloseDialogue();

        canvas.enabled = !TempleMenu.IsPaused && !TempleMenu.HasWon && puzzle != null && (!enteredSecondRoom || realOrrin != null) && !IsDialogueOpen &&
            (dialoguePanel == null || !dialoguePanel.activeInHierarchy) &&
            (victoryPanel == null || !victoryPanel.activeInHierarchy);
        if (!canvas.enabled)
            return;

        string objective = enteredSecondRoom ? SecondRoomObjective() : Objective();
        if (label.text != objective)
        {
            label.text = objective;
            panel.sizeDelta = new Vector2(510, Mathf.Max(72, 43 + label.GetPreferredValues(objective, 484, 0).y));
        }
    }

    private string SecondRoomObjective()
    {
        if (realOrrin.IsDoorOpen) return "Final mührüne ilerle";
        if (!realOrrin.IsOrrinConvinced) return $"Orrin'in güvenini kazan: {realOrrin.TrustPoints}/3";
        if (orrinSeal != null && orrinSeal.IsActivated) return "Orrin'e dön ve kapıyı açmasını iste";
        if (orrinSeal != null && orrinSeal.TrialRunning)
            return $"Mühür sınavı: {orrinSeal.SecondsRemaining} sn\nRün dizilimi: {orrinSeal.RuneProgress}/3 • Yakında E";
        if (orrinSeal != null && !string.IsNullOrEmpty(orrinSeal.TrialFeedback))
            return orrinSeal.TrialFeedback;
        string progress = $"Kadim elmasları topla: {CollectedDiamondCount}/{RequiredDiamonds}";
        if (!HasRequiredDiamonds) return progress;
        return progress + (orrinSeal != null && orrinSeal.IsPlayerInReach
            ? "\nE - 45 saniyelik mühür sınavını başlat"
            : "\nParlayan mühür taşına yaklaş");
    }

    private string Objective()
    {
        if (puzzle.IsGateOpen) return "İkinci odaya ilerle";
        if (puzzle.IsRuneActive) return "İlk kapının açılmasını bekle";
        int count = puzzle.ActivatedCrystalCount;
        if (count == 3)
            return heardNyraInstruction ? "Nyra'yı mühre yönlendir" : "Orrin'e dön ve sonraki görevini öğren (3/3)";
        if (!metProjection && count == 0 && puzzle.MistakesLeft == 3 &&
            string.IsNullOrEmpty(puzzle.ActiveFeedback)) return "Orrin'e yaklaş ve görevini öğren";
        string progress = $"Kristal Dizilimi: {count}/3\nHata Hakkı: {puzzle.MistakesLeft}/3";
        return string.IsNullOrEmpty(puzzle.ActiveFeedback) ? progress : progress + "\n" + puzzle.ActiveFeedback;
    }

    private void OnDestroy()
    {
        if (projectionPanel != null)
            Destroy(projectionPanel);
        if (canvas != null)
            Destroy(canvas.gameObject);
        if (glow != null)
            Destroy(glow);
    }
}
