
using UnityEngine;
using UnityEngine.InputSystem;

namespace MuhurluKapi.Interaction
{
    public class PlayerInteraction : MonoBehaviour
    {
        [SerializeField] private Transform orrin;
        [SerializeField] private float interactionDistance = 3.5f;
        [SerializeField] private GameObject dialoguePanel;

        private void Update()
        {
            if (orrin == null || Keyboard.current == null)
            {
                return;
            }

            float distance = Vector3.Distance(
                transform.position,
                orrin.position
            );

            if (distance <= interactionDistance &&
                Keyboard.current.fKey.wasPressedThisFrame)
            {
                if (dialoguePanel != null)
                {
                    bool isOpen = dialoguePanel.activeSelf;
                    dialoguePanel.SetActive(!isOpen);

                    Debug.Log(
                        dialoguePanel.activeSelf
                            ? "Orrin konusma paneli acildi!"
                            : "Orrin konusma paneli kapandi!"
                    );
                }
            }
        }
    }
}
