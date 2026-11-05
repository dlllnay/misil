using UnityEngine;
using UnityEngine.InputSystem;
using MuhurluKapi.Dialogue;

namespace MuhurluKapi.Interaction
{
    // Only Orrin's stone uses this input handler. SealStone keeps ownership of
    // activation state, so DoorController and companion stones remain compatible.
    public sealed class OrrinSealStone : SealStone
    {
        [SerializeField] private CharacterController interactionPlayer;
        [SerializeField] private OrrinDialogueUI orrinDialogue;
        [SerializeField] private QuestManager quest;
        [SerializeField, Min(0.1f)] private float reachDistance = 2.5f;
        public static readonly TempleRune[] Order = { TempleRune.Turquoise, TempleRune.Purple, TempleRune.Gold };
        private static readonly Vector3[] RuneOffsets =
        {
            new(-3.25f, 0, 1.5f), new(3.1f, 0, 2.3f), new(1.1f, 0, -4.0f)
        };
        public const int TrialSeconds = 45;
        public bool TrialRunning { get; private set; }
        public int RuneProgress { get; private set; }
        public string TrialFeedback { get; private set; }
        private float deadline;
        public int SecondsRemaining => Mathf.CeilToInt(Mathf.Max(0, deadline - Time.time));
        public Vector3 RunePosition(TempleRune rune) => transform.position + RuneOffsets[(int)rune];

        public bool HasRequiredDiamonds => quest != null && quest.HasRequiredDiamonds;
        public bool IsPlayerInReach =>
            interactionPlayer != null && interactionPlayer.enabled &&
            interactionPlayer.gameObject.activeInHierarchy &&
            Vector3.Distance(transform.position, interactionPlayer.ClosestPoint(transform.position)) <= reachDistance;

        private void Awake()
        {
            if (!TryGetComponent(out Renderer renderer))
                return;

            var colors = new MaterialPropertyBlock();
            colors.SetColor("_BaseColor", new Color(0.38f, 0.12f, 0.74f));
            colors.SetColor("_EmissionColor", new Color(0.65f, 0.20f, 1.6f));
            renderer.SetPropertyBlock(colors);

            var glow = gameObject.AddComponent<Light>();
            glow.type = LightType.Point;
            glow.color = new Color(0.48f, 0.24f, 1f);
            glow.range = 3f;
            glow.intensity = 1.2f;
            glow.shadows = LightShadows.None;
        }

        // Unity uses this message instead of SealStone's private Update.
        private void Update()
        {
            if (TrialRunning && Time.time >= deadline)
            {
                FailTrial("Süre doldu. Taşa dön ve E ile yeniden başlat.");
                return;
            }
            if (TrialRunning && (orrinDialogue == null || !orrinDialogue.IsOrrinConvinced))
            {
                FailTrial("Güven düştü. Orrin'in güvenini yeniden kazan.");
                return;
            }

            if (Keyboard.current == null ||
                !Keyboard.current.eKey.wasPressedThisFrame || IsActivated || Time.timeScale == 0f)
                return;

            if ((orrinDialogue != null && orrinDialogue.isActiveAndEnabled) ||
                (quest != null && quest.IsDialogueOpen) || PlayerInteraction.IsTextInputFocused())
                return;

            if (interactionPlayer == null || !interactionPlayer.enabled ||
                !interactionPlayer.gameObject.activeInHierarchy)
            {
                Debug.LogWarning("Orrin muhru: etkin Player CharacterController baglantisi eksik.", this);
                return;
            }

            Interact();
        }

        private void Interact()
        {
            if (IsActivated) return;
            if (TrialRunning)
            {
                TryRune();
                return;
            }

            // A nearby diamond can be collected while the stone is still locked.
            if (quest != null && quest.TryCollectDiamond(interactionPlayer))
                return;

            if (!IsPlayerInReach)
                return;

            if (orrinDialogue == null)
            {
                Debug.LogWarning("Orrin mührü etkinleştirilemedi: OrrinDialogueUI bağlantısı eksik.", this);
                return;
            }

            // The panel may be closed: read its existing state without requiring
            // an active UI, and never maintain a separate trust counter here.
            if (!orrinDialogue.IsOrrinConvinced)
            {
                Debug.Log($"Mühür kilitli: Önce Orrin'le konuşup güvenini 3'e çıkar. Mevcut güven: {orrinDialogue.TrustPoints}/3.", this);
                return;
            }

            if (!HasRequiredDiamonds)
            {
                Debug.Log("Mühür kilitli: Önce üç kadim elması topla.", this);
                return;
            }

            TrialRunning = true;
            RuneProgress = 0;
            TrialFeedback = null;
            deadline = Time.time + TrialSeconds;
        }

        private void TryRune()
        {
            for (int i = 0; i < RuneOffsets.Length; i++)
            {
                TempleRune rune = (TempleRune)i;
                Vector3 position = RunePosition(rune);
                if (Vector3.Distance(position, interactionPlayer.ClosestPoint(position)) > 1.25f)
                    continue;
                if (rune != Order[RuneProgress])
                {
                    FailTrial("Yanlış rün. Taşa dön ve E ile yeniden başlat.");
                    return;
                }
                RuneProgress++;
                if (RuneProgress == Order.Length)
                    Activate();
                else
                    TempleAudio.Play(TempleSound.Crystal, position);
                return;
            }
        }

        private void FailTrial(string message)
        {
            TrialRunning = false;
            RuneProgress = 0;
            TrialFeedback = message;
            TempleAudio.Play(TempleSound.PuzzleError, transform.position);
        }

        public override void Activate()
        {
            if (IsActivated || !TrialRunning || RuneProgress != Order.Length ||
                Time.time >= deadline || orrinDialogue == null ||
                !orrinDialogue.IsOrrinConvinced || !HasRequiredDiamonds)
                return;
            TrialRunning = false;
            base.Activate();
            TempleAudio.Play(TempleSound.OrrinSeal, transform.position);
            Debug.Log("Orrin mührü etkinleşti. Orrin'e geri dön, F ile konuş ve kapıyı açmasını iste.", this);
        }
    }
}
