
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using MuhurluKapi.Llm;
using MuhurluKapi.Door;
using Stopwatch = System.Diagnostics.Stopwatch;

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
        private const int RequiredTrust = DoorController.RequiredTrust;

        private bool IsOrrinConvinced =>
            trustPoints >= RequiredTrust;


        private const string OrrinSystemPrompt =
            "Sen Orrin'sin. Mühürlü Kapı adlı fantastik oyunda " +
            "kadim kapıyı koruyan yaşlı, bilge ve temkinli bir bekçisin. " +
            "Karşındaki kişi kapıyı geçmek isteyen bir yolcudur. " +
            "Yalnızca Türkçe konuş. En fazla iki kısa cümle kur. " +
            "Yolcuya 'sen' diye hitap et. " +
            "Asla yapay zekâ asistanı, müşteri temsilcisi veya " +
            "otel görevlisi gibi davranma. " +
            "'Size nasıl yardımcı olabilirim?' gibi ifadeler kullanma. " +
            "Otel, resepsiyon, teknoloji veya modern dünya hakkında konuşma. " +
            "Oyuncunun sözlerini aynen tekrarlama. " +
            "Kapıyı koruma görevin, yolcunun niyetini sınamak " +
            "ve kadim mührün güvenliğini sağlamaktır. " +
            "Bilmediğin hikâye ayrıntılarını uydurma. " +
            "Önceki konuşmaları hatırla ve tutarlı cevap ver. " +
            "Yalnızca dialogue, action ve trust_delta alanlarını içeren JSON üret. " +
            "action yalnızca none veya open_door olabilir. " +
            "trust_delta yalnızca -1, 0 veya 1 olabilir. " +
            "Yolcu içten, saygılı ve gerekçeli biçimde konuşursa trust_delta 1 seç. " +
            "Yolcu tehdit veya hakaret ederse trust_delta -1 seç. " +
            "Sıradan sorularda ve yalnızca kapıyı açma isteğinde trust_delta 0 seç. " +
            "Yolcu son mesajında kapının açılmasını açıkça isterse " +
            "action open_door seç, diğer durumlarda none seç. " +
            "Güven ve bulmaca koşullarını oyun sistemi denetler.";


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
            Stopwatch stopwatch = Stopwatch.StartNew();

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
                stopwatch.Stop();

                UnityEngine.Debug.Log(
                    $"Ollama yanit suresi: {stopwatch.Elapsed.TotalSeconds:F2} saniye");

                conversationHistory.Add(new OllamaClient.ChatMessage
                {
                    role = "assistant",
                    content = response.Dialogue
                });

                trustPoints = Mathf.Clamp(
                    trustPoints + response.TrustDelta.Value,
                    0,
                    RequiredTrust
                );

                Debug.Log(
                    $"Orrin karari: {response.Action}, " +
                    $"guven: {trustPoints}/{RequiredTrust}, " +
                    $"degisim: {response.TrustDelta.Value}"
                );

                if (response.Action == "open_door")
                {
                    if (!IsOrrinConvinced)
                    {
                        Debug.Log("Kapı isteği reddedildi: Orrin güveni yetersiz.");
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
                        Debug.Log("Kapı isteği reddedildi: Orrin mührü etkin değil.");
                        orrinText.text =
                            "Orrin: Sana güveniyorum, ancak kapının " +
                            "mührü henüz çözülmedi.\n\n" +
                            $"[Güven: {trustPoints}/{RequiredTrust}]";
                    }
                    else
                    {
                        sealedDoor.OpenDoor(response, trustPoints);

                        orrinText.text = sealedDoor.IsOpen
                            ? "Orrin: " + response.Dialogue +
                              "\n\n[Mühürlü kapı açıldı.]"
                            : "Orrin: Kapı henüz açılamıyor.";
                    }
                }
                else
                {
                    Debug.Log("Kapı açılmadı: LLM action=none.");
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
                stopwatch.Stop();

                UnityEngine.Debug.Log(
                    $"Ollama toplam istek suresi: {stopwatch.Elapsed.TotalSeconds:F2} saniye");

                isWaitingForResponse = false;

                if (sendButton != null)
                    sendButton.interactable = true;
            }
        }
    }
}
