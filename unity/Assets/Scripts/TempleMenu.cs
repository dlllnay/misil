using MuhurluKapi.Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(-100)]
public sealed class TempleMenu : MonoBehaviour
{
    private const string MenuScene = "Assets/Scenes/MainMenu.unity";
    private const string GameScene = "Assets/Scenes/MainGame.unity";
    internal static readonly Color Night = new(.035f, .024f, .078f);
    private static readonly Color Stone = new(.12f, .085f, .20f);
    internal static readonly Color Gold = new(.90f, .68f, .35f);
    internal static readonly Color Teal = new(.28f, .88f, .85f);
    internal static readonly Color Violet = new(.52f, .28f, .85f);
    internal static readonly Color Ivory = new(.98f, .94f, .86f);

    private static TempleMenu current;
    private static int resumeFrame = -1;
    public static bool HandlesEscape => current != null;
    public static bool IsPaused { get; private set; }
    internal static bool HasWon => current != null && current.portal != null && current.portal.IsVictoryVisible;
    public static bool BlocksGameplay => IsPaused || HasWon || Time.frameCount == resumeFrame;

    private bool mainMenu;
    private bool loading;
    private RectTransform overlay;
    private RectTransform help;
    private PlayerInteraction interaction;
    private FinalPortalTrigger portal;
    private CursorLockMode cursorLock;
    private bool cursorVisible;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Register()
    {
        current = null;
        resumeFrame = -1;
        IsPaused = false;
        SceneManager.sceneLoaded -= Install;
        SceneManager.sceneLoaded += Install;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallInitialScene()
    {
        if (current == null) Install(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private static void Install(Scene scene, LoadSceneMode mode)
    {
        if (current != null && current.gameObject.scene == scene) return;
        bool menu = scene.path == MenuScene;
        if (!menu && scene.path != GameScene) return;
        Time.timeScale = 1f;
        // Nyra uses the actual mouse position to select a point on the floor.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        var root = new GameObject(menu ? "Mühürlü Kapı • Ana Menü" : "Mühürlü Kapı • Duraklatma");
        SceneManager.MoveGameObjectToScene(root, scene);
        current = root.AddComponent<TempleMenu>();
        current.mainMenu = menu;
        current.Build();
    }

    private void Update()
    {
        if (!loading && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            HandleEscape();
    }

    private void HandleEscape()
    {
        if (mainMenu)
        {
            if (help.gameObject.activeSelf) ShowHelp(false);
            return;
        }
        if (IsPaused) Resume();
        else if (interaction != null && interaction.TryCloseDialogue()) return;
        else if (portal == null || !portal.IsVictoryVisible) Pause();
    }

    internal static void ShowVictory()
    {
        if (current == null || current.mainMenu) return;
        current.interaction?.TryCloseDialogue();
        current.overlay.gameObject.SetActive(false);
        IsPaused = false;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        ClearSelection();
        current.gameObject.AddComponent<TempleVictory>().Build(
            () => current.Load(GameScene), () => current.Load(MenuScene));
    }

    private void Pause()
    {
        cursorLock = Cursor.lockState;
        cursorVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        IsPaused = true;
        Time.timeScale = 0f;
        overlay.gameObject.SetActive(true);
        ClearSelection();
    }

    private void Resume()
    {
        ClearSelection();
        overlay.gameObject.SetActive(false);
        resumeFrame = Time.frameCount;
        IsPaused = false;
        Time.timeScale = 1f;
        Cursor.lockState = cursorLock;
        Cursor.visible = cursorVisible;
    }

    private void Load(string scene)
    {
        if (loading) return;
        int buildIndex = SceneUtility.GetBuildIndexByScenePath(scene);
        if (buildIndex < 0 || !Application.CanStreamedLevelBeLoaded(buildIndex))
        {
            Debug.LogError("Sahne etkin build listesinde bulunamadı: " + scene);
            return;
        }
        loading = true;
        resumeFrame = Time.frameCount;
        ClearSelection();
        IsPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(buildIndex);
    }

    private void OnDestroy()
    {
        if (current != this) return;
        current = null;
        IsPaused = false;
        Time.timeScale = 1f;
    }

    internal static RectTransform CreateCanvas(Transform parent, string title, int sortOrder)
    {
        var canvas = new GameObject(title, typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas.transform.SetParent(parent, false);
        canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.GetComponent<Canvas>().sortingOrder = sortOrder;
        var scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        return (RectTransform)canvas.transform;
    }

    private void Build()
    {
        var root = CreateCanvas(transform, "Tapınak menüsü", 200);

        if (!mainMenu)
        {
            interaction = FindAnyObjectByType<PlayerInteraction>();
            portal = FindAnyObjectByType<FinalPortalTrigger>();
            overlay = Fill(root, new Color(.024f, .015f, .053f, .86f));
            var card = Frame(overlay, 0, 0, 520, 440);
            Label(card, "KADİM TAPINAK", 0, 158, 430, 32, 19, Teal);
            Label(card, "OYUN DURAKLATILDI", 0, 107, 460, 58, 35, Gold);
            Action(card, "DEVAM ET", -4, Resume);
            Action(card, "YENİDEN BAŞLAT", -78, () => Load(GameScene));
            Action(card, "ANA MENÜ", -152, () => Load(MenuScene));
            overlay.gameObject.SetActive(false);
            return;
        }

        if (EventSystem.current == null)
        {
            var input = new GameObject("Menü EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            input.transform.SetParent(transform, false);
        }
        var background = Fill(root, Night);
        DrawGate(background);
        var menu = Frame(background, -315, 0, 466, 548);
        Label(menu, "KADİM TAPINAĞIN EŞİĞİ", 0, 203, 432, 34, 17, Teal);
        Label(menu, "MÜHÜRLÜ KAPI", 0, 146, 432, 68, 48, Gold);
        Label(menu, "Kadim sırlar seni bekliyor...", 0, 92, 420, 36, 23, Ivory).fontStyle = FontStyles.Italic;
        Action(menu, "OYUNA BAŞLA", 3, () => Load(GameScene));
        Action(menu, "NASIL OYNANIR", -72, () => ShowHelp(true));
        Action(menu, "ÇIKIŞ", -147, Quit);

        help = Fill(root, new Color(.018f, .012f, .037f, .94f));
        var guide = Frame(help, 0, 0, 1040, 604);
        Label(guide, "NASIL OYNANIR", 0, 244, 900, 56, 42, Gold);
        Label(guide, "KONTROLLER", -247, 169, 448, 40, 25, Teal);
        Label(guide, "TAPINAĞIN SINAVLARI", 247, 169, 440, 40, 25, Teal);
        Label(guide, "WASD  ·  Hareket\nOrta fareyi tut + fare  ·  Kamerayı döndür\nSağ fareyi tut + sol tık  ·  Ok at\nF  ·  Orrin ile konuş\nE  ·  Elmas topla, mühür veya rün kullan\nG  ·  Nyra'nın takibini değiştir\nShift + sol tık  ·  Nyra'yı hedefe gönder\nEsc  ·  Diyaloğu kapat veya duraklat",
            -247, 1, 448, 285, 21, Ivory).alignment = TextAlignmentOptions.TopLeft;
        Label(guide, "Yazıtları okuyup kristalleri doğru sırayla uyandır. Nyra'nın yardımıyla ilk mührü aç.\n\nOrrin'in güvenini kazan ve kadim elmasları keşfet. Süreli rün sınavını tamamla, son kapıyı aç ve tapınaktan çık.",
            247, 1, 440, 285, 23, Ivory).alignment = TextAlignmentOptions.TopLeft;
        Action(guide, "GERİ DÖN", -233, () => ShowHelp(false));
        help.gameObject.SetActive(false);

        gameObject.AddComponent<AudioListener>();
        var music = gameObject.AddComponent<AudioSource>();
        music.playOnAwake = false;
        music.clip = Resources.Load<AudioClip>("TempleAudio/TempleAmbient");
        music.loop = true;
        music.volume = .15f;
        if (music.clip != null) music.Play();
    }

    private void ShowHelp(bool show)
    {
        ClearSelection();
        help.gameObject.SetActive(show);
    }

    private static void ClearSelection()
    {
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }

    private static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private static void DrawGate(RectTransform root)
    {
        var gate = Rect(root, 275, -12, 600, 630);
        Tile(gate, 0, -68, 510, 465, new Color(.08f, .049f, .16f));
        for (int i = 0; i < 12; i++)
        {
            float angle = (i + .5f) * 15f;
            float radians = angle * Mathf.Deg2Rad;
            var wedge = Tile(gate, Mathf.Cos(radians) * 228, 76 + Mathf.Sin(radians) * 228,
                67, 57, i % 2 == 0 ? Stone : new Color(.16f, .10f, .25f));
            wedge.localEulerAngles = new Vector3(0, 0, angle - 90);
        }
        for (int side = -1; side <= 1; side += 2)
        {
            Tile(gate, side * 223, -108, 65, 390, Stone);
            Tile(gate, side * 223, -108, 3, 364, Gold * .65f);
            Tile(gate, side * 223, -291, 90, 23, Stone);
            for (int row = 0; row < 5; row++)
                Tile(gate, side * 104, -245 + row * 68, 183, 2, Violet * .30f);
        }
        Tile(gate, 0, -111, 3, 385, Gold * .55f);
        var seal = Rect(gate, 0, 5, 1, 1);
        for (int i = 0; i < 32; i++)
        {
            float angle = i * 360f / 32;
            float radians = angle * Mathf.Deg2Rad;
            var rim = Tile(seal, Mathf.Cos(radians) * 113, Mathf.Sin(radians) * 113, 24, 3, Violet);
            rim.localEulerAngles = new Vector3(0, 0, angle + 90);
            if (i % 4 == 0)
                Tile(seal, Mathf.Cos(radians) * 133, Mathf.Sin(radians) * 133, 6, 6, Gold)
                    .localEulerAngles = new Vector3(0, 0, 45);
        }
        Tile(seal, 0, 0, 110, 110, Violet).localEulerAngles = new Vector3(0, 0, 45);
        Tile(seal, 0, 0, 103, 103, Night).localEulerAngles = new Vector3(0, 0, 45);
        Tile(seal, 0, 0, 34, 34, Teal).localEulerAngles = new Vector3(0, 0, 45);
        for (int i = 0; i < 5; i++)
            Tile(gate, 0, -239 - i * 11, 402 + i * 22, 24,
                new Color(.33f, .17f, .62f, .08f + i * .021f));
    }

    private static RectTransform Frame(RectTransform parent, float x, float y, float w, float h)
    {
        var border = Tile(parent, x, y, w, h, Violet);
        var panel = Tile(border, 0, 0, w - 5, h - 5, Night);
        Tile(panel, 0, h / 2 - 12, w - 40, 1, Gold);
        Tile(panel, 0, -h / 2 + 12, w - 40, 1, Gold);
        return panel;
    }

    internal static Button Action(RectTransform parent, string title, float y, UnityEngine.Events.UnityAction action)
    {
        var rim = Tile(parent, 0, y, 328, 59, Gold);
        var face = Tile(rim, 0, 0, 324, 55, Stone);
        var image = face.GetComponent<Image>();
        image.raycastTarget = true;
        var button = face.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        var colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(2f, 1.75f, 2.9f);
        colors.selectedColor = colors.highlightedColor;
        colors.pressedColor = new Color(.8f, 2.4f, 2.4f);
        button.colors = colors;
        button.onClick.AddListener(action);
        Label(face, title, 0, 0, 302, 51, 23, Ivory);
        return button;
    }

    internal static RectTransform Fill(RectTransform parent, Color color)
    {
        var tile = Tile(parent, 0, 0, 0, 0, color);
        tile.anchorMin = Vector2.zero;
        tile.anchorMax = Vector2.one;
        tile.sizeDelta = Vector2.zero;
        tile.GetComponent<Image>().raycastTarget = true;
        return tile;
    }

    internal static RectTransform Tile(RectTransform parent, float x, float y, float w, float h, Color color)
    {
        var rect = Rect(parent, x, y, w, h);
        var image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return rect;
    }

    internal static TextMeshProUGUI Label(RectTransform parent, string value,
        float x, float y, float w, float h, int size, Color color)
    {
        var rect = Rect(parent, x, y, w, h);
        rect.name = value.Split('\n')[0];
        var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.text = value;
        label.fontSize = size;
        label.color = color;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
        return label;
    }

    internal static RectTransform Rect(RectTransform parent, float x, float y, float w, float h)
    {
        var rect = (RectTransform)new GameObject("Tapınak • Arayüz", typeof(RectTransform)).transform;
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(w, h);
        return rect;
    }
}
