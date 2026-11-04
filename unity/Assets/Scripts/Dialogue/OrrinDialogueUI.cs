
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using MuhurluKapi.Llm;
using MuhurluKapi.Door;

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

        private int trustPoints = 0;
        private const int RequiredTrust = 3;

        private bool IsOrrinConvinced =>
            trustPoints >= RequiredTrust;

        private const string OrrinSystemPrompt =
            "Sen Orrin'sin. Kadim mühürlü kapıyı koruyan yaşlı, " +
            "bilge ve gizemli bir bekçisin. " +
            "Yalnızca Türkçe, doğal ve en fazla iki kısa cümleyle konuş. " +
            "Oyuncuya doğrudan hitap et. Bir yapay zekâ asistanı gibi davranma. " +
            "Önceki konuşma mesajlarını dikkate al. " +
            "Yalnızca dialogue, action ve trust_delta alanlarını içeren JSON üret. " +
            "action yalnızca none veya open_door olabilir. " +
            "trust_delta yalnızca -1, 0 veya 1 olabilir. " +
            "Oyuncu samimi, saygılı veya ikna edici bir gerekçe sunuyorsa " +
            "trust_delta 1 seç. " +
            "Oyuncu tehdit ediyor veya hakaret ediyorsa trust_delta -1 seç. " +
            "Sıradan sorularda ve yalnızca kapıyı açma isteğinde trust_delta 0 seç. " +
            "Oyuncunun son mesajında kapıyı açman açıkça isteniyorsa " +
            "action open_door seç, diğer durumlarda none seç. " +
            "Kapının fiziksel kilidini ve güven koşulunu oyun sistemi kontrol eder. " +
            "Kapının ardındaki sırları uydurma.";

        private void Update()
        {
            if (playerInput == null ||
                Keyboard.current == null ||
                isWaitingForResponse)
                return;

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

        public async void SendMessage()
        {
            if (isWaitingForResponse ||
                playerInput == null ||
                orrinText == null ||
                sendButton == null)
                return;

            string message = playerInput.text.Trim();

            if (string.IsNullOrWhiteSpace(message))
                return;

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

                conversationHistory.Add(new OllamaClient.ChatMessage
                {
                    role = "assistant",
                    content = response.Dialogue
                });

                trustPoints = Mathf.Clamp(
                    trustPoints + response.TrustDelta,
                    0,
                    RequiredTrust
                );

                Debug.Log(
                    $"Orrin karari: {response.Action}, " +
                    $"guven: {trustPoints}/{RequiredTrust}, " +
                    $"degisim: {response.TrustDelta}"
                );

                if (response.Action == "open_door")
                {
                    if (!IsOrrinConvinced)
                    {
                        orrinText.text =
                            "Orrin: Henüz güvenimi kazanmadın, yolcu. " +
                            "Neden sana inanmalıyım?\n\n" +
                            $"[Güven: {trustPoints}/{RequiredTrust}]";
                    }
                    else if (sealedDoor == null)
                    {
                        orrinText.text =
                            "Orrin: Kapının bağlantısı bulunamadı.";

                        Debug.LogWarning(
                            "SealedDoor bağlantısı eksik!");
                    }
                    else if (!sealedDoor.IsPuzzleCompleted)
                    {
                        orrinText.text =
                            "Orrin: Sana güveniyorum, ancak kapının " +
                            "mührü henüz çözülmedi.\n\n" +
                            $"[Güven: {trustPoints}/{RequiredTrust}]";
                    }
                    else
                    {
                        sealedDoor.OpenDoor();

                        orrinText.text = sealedDoor.IsOpen
                            ? "Orrin: " + response.Dialogue +
                              "\n\n[Mühürlü kapı açıldı.]"
                            : "Orrin: Kapı henüz açılamıyor.";
                    }
                }
                else
                {
                    orrinText.text =
                        "Orrin: " + response.Dialogue +
                        $"\n\n[Güven: {trustPoints}/{RequiredTrust}]";
                }
            }
            catch (Exception exception)
            {
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
                    sendButton.interactable = true;
            }
        }
    }
}
