
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MuhurluKapi.Llm;
using MuhurluKapi.Door;
using UnityEngine.InputSystem;

namespace MuhurluKapi.Dialogue
{
    public class OrrinDialogueUI : MonoBehaviour
    {
        [Header("Konuşma Arayüzü")]
        [SerializeField] private TMP_Text orrinText;
        [SerializeField] private TMP_InputField playerInput;
        [SerializeField] private Button sendButton;

        [Header("Kapı")]
        [SerializeField] private DoorController sealedDoor;

        private readonly OllamaClient ollamaClient = new OllamaClient();

        private readonly List<OllamaClient.ChatMessage> conversationHistory =
            new List<OllamaClient.ChatMessage>();

        private bool isWaitingForResponse;


        private void Update()
        {
            if (playerInput == null ||
                Keyboard.current == null ||
                isWaitingForResponse)
            {
                return;
            }

            bool enterPressed =
                Keyboard.current.enterKey.wasPressedThisFrame ||
                Keyboard.current.numpadEnterKey.wasPressedThisFrame;

            if (enterPressed &&
                gameObject.activeInHierarchy &&
                !string.IsNullOrWhiteSpace(playerInput.text))
            {
                SendMessage();
            }
        }



        private const string OrrinSystemPrompt =
            "Sen Orrin'sin. Fantastik bir dünyada kadim mühürlü kapıyı " +
            "koruyan yaşlı, bilge ve gizemli bir bekçisin. " +
            "Bir yapay zekâ veya yardımcı asistan gibi konuşma. " +
            "Yalnızca Türkçe, doğal ve en fazla iki kısa cümleyle cevap ver. " +
            "Oyuncuya doğrudan hitap et. Kendi adın Orrin'dir. " +
            "Oyuncunun adını kendi adınla karıştırma. " +
            "Önceki konuşma mesajlarını dikkate al. " +
            "Oyuncu kendi adını sorarsa önceki user mesajlarını incele. " +
            "Oyuncunun yerine konuşma ve önceki cevaplarını gereksiz tekrarlama. " +
            "'Size nasıl yardımcı olabilirim?' gibi asistan ifadeleri kullanma. " +
            "Yalnızca dialogue ve action alanları olan geçerli JSON üret. " +
            "action yalnızca none veya open_door olabilir. " +
            "Oyuncunun yalnızca en son mesajında kapıyı açman açıkça " +
            "isteniyorsa open_door seç. Diğer durumlarda none seç. " +
            "open_door seçtiğinde kapıyı açmayı kabul ettiğini söyle. " +
            "Kapının ardındaki bilinmeyen sırları uydurma.";

        public async void SendMessage()
        {
            if (isWaitingForResponse ||
                playerInput == null ||
                orrinText == null ||
                sendButton == null)
            {
                return;
            }

            string message = playerInput.text.Trim();

            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            // Aynı mesajın tekrar gönderilmesini önle.
            isWaitingForResponse = true;
            sendButton.interactable = false;

            playerInput.SetTextWithoutNotify("");
            orrinText.text = "Orrin düşünüyor...";

            var playerMessage = new OllamaClient.ChatMessage
            {
                role = "user",
                content = message
            };

            conversationHistory.Add(playerMessage);

            try
            {
                var messages = new List<OllamaClient.ChatMessage>
                {
                    new OllamaClient.ChatMessage
                    {
                        role = "system",
                        content = OrrinSystemPrompt
                    }
                };

                messages.AddRange(conversationHistory);

                NpcResponse response =
                    await ollamaClient.GenerateNpcChatResponseAsync(messages);

                // Gerçek LLM cevabını konuşma geçmişinde sakla.
                conversationHistory.Add(new OllamaClient.ChatMessage
                {
                    role = "assistant",
                    content = response.Dialogue
                });

                Debug.Log("Orrin karari: " + response.Action);

                if (response.Action == "open_door")
                {
                    if (sealedDoor != null)
                    {
                        sealedDoor.OpenDoor();

                        // Oyun olayını NPC konuşmasından ayrı bildir.
                        orrinText.text =
                            "Orrin: " + response.Dialogue +
                            "\n\n[Mühürlü kapı açıldı.]";
                    }
                    else
                    {
                        orrinText.text = "Orrin: " + response.Dialogue;

                        Debug.LogWarning(
                            "SealedDoor bağlantısı eksik!");
                    }
                }
                else
                {
                    orrinText.text = "Orrin: " + response.Dialogue;
                }
            }
            catch (Exception exception)
            {
                // Başarısız mesajı geçmişten kaldır.
                conversationHistory.Remove(playerMessage);

                orrinText.text =
                    "Orrin şu anda yanıt veremiyor. " +
                    "Lütfen tekrar dene.";

                Debug.LogError(
                    "Ollama bağlantı hatası: " + exception.Message);
            }
            finally
            {
                isWaitingForResponse = false;

                if (sendButton != null)
                {
                    sendButton.interactable = true;
                }
            }
        }
    }
}
