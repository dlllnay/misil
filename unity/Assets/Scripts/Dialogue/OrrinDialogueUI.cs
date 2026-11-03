
using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MuhurluKapi.Llm;

namespace MuhurluKapi.Dialogue
{
    public class OrrinDialogueUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text orrinText;
        [SerializeField] private TMP_InputField playerInput;
        [SerializeField] private Button sendButton;

        private readonly OllamaClient ollamaClient = new OllamaClient();
        private bool isWaitingForResponse;

        public async void SendMessage()
        {
            if (isWaitingForResponse || playerInput == null ||
                orrinText == null || sendButton == null)
            {
                return;
            }

            string message = playerInput.text.Trim();
            playerInput.text = "";

            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            isWaitingForResponse = true;
            sendButton.interactable = false;
            //playerInput.interactable = false;
            orrinText.text = "Orrin düşünüyor...";

            try
            {
                NpcResponse response =
                    await ollamaClient.GenerateNpcResponseAsync(message);

                orrinText.text = "Orrin: " + response.Dialogue;

                Debug.Log("Orrin karari: " + response.Action);
            }
            catch (Exception exception)
            {
                orrinText.text =
                    "Orrin şu anda yanıt veremiyor. Lütfen tekrar dene.";

                Debug.LogError(
                    "Ollama baglanti hatasi: " + exception.Message
                );
            }
            finally
            {
                isWaitingForResponse = false;
                sendButton.interactable = true;
                playerInput.interactable = true;
            }
        }
    }
}
