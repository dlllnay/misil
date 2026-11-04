using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace MuhurluKapi.Interaction
{
    // Only Orrin's stone uses this input handler. SealStone keeps ownership of
    // activation state, so DoorController and companion stones remain compatible.
    public sealed class OrrinSealStone : SealStone
    {
        [SerializeField] private CharacterController interactionPlayer;
        [SerializeField, Min(0.1f)] private float reachDistance = 2.5f;

        // Unity uses this message instead of SealStone's private Update.
        private void Update()
        {
            if (Keyboard.current == null ||
                !Keyboard.current.eKey.wasPressedThisFrame || IsActivated)
                return;

            // Typing the letter E in dialogue must not solve the seal.
            GameObject selected = EventSystem.current != null
                ? EventSystem.current.currentSelectedGameObject : null;
            if (selected != null &&
                selected.GetComponentInParent<TMPro.TMP_InputField>() != null)
                return;

            if (interactionPlayer == null || !interactionPlayer.enabled ||
                !interactionPlayer.gameObject.activeInHierarchy)
            {
                Debug.LogWarning("Orrin muhru: etkin Player CharacterController baglantisi eksik.", this);
                return;
            }

            // Measure reach from the actual body, not the model's elevated pivot.
            float distance = Vector3.Distance(transform.position,
                interactionPlayer.ClosestPoint(transform.position));
            if (distance > reachDistance)
            {
                Debug.Log($"Orrin muhru: tas uzakta ({distance:F2} m / {reachDistance:F2} m).", this);
                return;
            }

            Activate();
            Debug.Log("Orrin muhru etkin: kapi icin guven ve LLM karari da gerekiyor.", this);
        }
    }
}
