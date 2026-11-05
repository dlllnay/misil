
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.EventSystems;

namespace MuhurluKapi.Interaction
{
    public class PlayerInteraction : MonoBehaviour
    {
        [SerializeField] private Transform orrin;
        [SerializeField] private float interactionDistance = 3.5f;
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TMP_InputField playerInput;
        [SerializeField] private QuestManager projection;

        public bool IsDialogueOpen =>
            (dialoguePanel != null && dialoguePanel.activeInHierarchy) ||
            (projection != null && projection.IsDialogueOpen);

        public static bool IsTextInputFocused()
        {
            var selected = EventSystem.current?.currentSelectedGameObject;
            return selected != null &&
                selected.GetComponentInParent<TMP_InputField>() is TMP_InputField input &&
                input.isFocused;
        }

        public bool TryCloseDialogue()
        {
            if (dialoguePanel != null && dialoguePanel.activeInHierarchy)
            {
                dialoguePanel.SetActive(false);
                return true;
            }
            return projection != null && projection.TryCloseDialogue();
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || TempleMenu.BlocksGameplay)
                return;
            bool focused = (playerInput != null && playerInput.isFocused) || IsTextInputFocused();
            if (dialoguePanel != null && dialoguePanel.activeSelf &&
                ((!TempleMenu.HandlesEscape && keyboard.escapeKey.wasPressedThisFrame) || (keyboard.fKey.wasPressedThisFrame && !focused)))
            {
                dialoguePanel.SetActive(false);
                return;
            }
            if (focused || !keyboard.fKey.wasPressedThisFrame)
                return;

            if (projection != null && projection.TryInteract(transform.position))
                return;
            if (orrin == null)
                return;

            float distance = Vector3.Distance(
                transform.position,
                orrin.position
            );

            if (distance <= interactionDistance)
            {
                if (dialoguePanel != null)
                {
                    dialoguePanel.SetActive(true);
                }
            }
        }
    }
}
