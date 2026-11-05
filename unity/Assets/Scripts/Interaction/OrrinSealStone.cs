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
            if (Keyboard.current == null ||
                !Keyboard.current.eKey.wasPressedThisFrame || IsActivated)
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

            Activate();
            TempleAudio.Play(TempleSound.OrrinSeal, transform.position);
            Debug.Log("Orrin mührü etkinleşti. Orrin'e geri dön, F ile konuş ve kapıyı açmasını iste.", this);
        }
    }
}
