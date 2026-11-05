
using UnityEngine;

public class CrystalDoorPuzzle : MonoBehaviour
{
    public const int MaxMistakes = 3;
    public int MistakesLeft { get; private set; } = MaxMistakes;
    public string Feedback { get; private set; }
    private float feedbackUntil;
    [SerializeField] private CrystalTarget crystal1;
    [SerializeField] private CrystalTarget crystal2;
    [SerializeField] private CrystalTarget crystal3;
    [SerializeField] private NyraRune nyraRune;

    [SerializeField] private Transform door;
    [SerializeField] private float openDistance = 3f;
    [SerializeField] private float openSpeed = 2f;

    private Vector3 openPosition;
    private bool opening;

    private CrystalTarget[] sequence;
    public CrystalTarget[] Sequence => sequence;
    public string ActiveFeedback => Time.time < feedbackUntil ? Feedback : "";

    private void Awake()
    {
        sequence = new[] { crystal1, crystal2, crystal3 };
        for (int i = 0; i < sequence.Length; i++)
            if (sequence[i] != null)
                sequence[i].Bind(this, (TempleRune)i);
    }

    public string Clue(int index)
    {
        string[] lead = { "Önce", "Ardından", "Son olarak" };
        return $"{lead[index]} {sequence[index].Rune.Phrase()}.";
    }

    public string Hint(int level)
    {
        if (level == 1) return "Üç ayrı duvar yazıtını ara. Her biri bir sırayı ve o sıraya ait kristali anlatır.";
        if (level == 2) return Clue(0) + " İkinci ve üçüncü adı tapınakta ara.";
        return $"{sequence[0].Rune.Label()} → {sequence[1].Rune.Label()} → {sequence[2].Rune.Label()}.";
    }

    public void Hit(CrystalTarget target)
    {
        int index = ActivatedCrystalCount;
        if (target.IsActivated || index >= sequence.Length) return;
        if (sequence[index] == target)
        {
            target.AcceptHit();
            return;
        }

        TempleAudio.Play(TempleSound.PuzzleError, target.transform.position);
        MistakesLeft--;
        Feedback = "Yanlış kristal. Yazıtları kontrol et.";
        if (MistakesLeft == 0)
        {
            foreach (var crystal in sequence)
                if (crystal != null) crystal.ResetCrystal();
            MistakesLeft = MaxMistakes;
            Feedback = "Üç hata. Kristal dizilimi sıfırlandı.";
        }
        target.FlashMistake();
        feedbackUntil = Time.time + 3f;
    }

    public int ActivatedCrystalCount =>
        (crystal1 != null && crystal1.IsActivated ? 1 : 0) +
        (crystal2 != null && crystal2.IsActivated ? 1 : 0) +
        (crystal3 != null && crystal3.IsActivated ? 1 : 0);

    public bool IsGateOpen => opening && door != null && !door.gameObject.activeSelf;
    public bool IsRuneActive => nyraRune != null && nyraRune.IsActivated;
    public Transform Gate => door;

    private void Start()
    {
        if (door == null)
        {
            Debug.LogWarning("DungeonGateDoor atanmamis!");
            return;
        }

        openPosition = door.position + Vector3.down * openDistance;
    }

    private void Update()
    {
        if (door == null || crystal1 == null ||
            crystal2 == null || crystal3 == null)
            return;

        if (!opening &&
            crystal1.IsActivated &&
            crystal2.IsActivated &&
            crystal3.IsActivated &&
            nyraRune != null &&
            nyraRune.IsActivated)
        {
            opening = true;
            TempleAudio.Play(TempleSound.FirstGate, door.position);
            Debug.Log("Uc kristal aktif! Muhurlu kapi aciliyor!");
        }

        if (opening && door.gameObject.activeSelf)
        {
            door.position = Vector3.MoveTowards(
                door.position,
                openPosition,
                openSpeed * Time.deltaTime
            );

            // Keep the closed barrier until the puzzle authorizes opening, then
            // remove the sunk model, collider and NavMesh obstacle together.
            if (door.position == openPosition)
                door.gameObject.SetActive(false);
        }
    }
}
